﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

class Dice
{
    public List<int> Faces { get; }

    public Dice(string faces)
    {
        Faces = faces.Split(',').Select(int.Parse).ToList();
    }

    public int GetFace(int index)
    {
        return Faces[index % Faces.Count];
    }

    public int GetSides()
    {
        return Faces.Count;
    }
}

class DiceParser
{
    public static List<Dice> Parse(string[] args)
    {
        if (args.Length < 3)
        {
            throw new Exception("At least 3 dice are required! Example: dotnet run 2,2,4,4,9,9 1,1,6,6,8,8 3,3,5,5,7,7");
        }

        return args.Select(arg => new Dice(arg)).ToList();
    }
}

class CryptoUtil
{
    public static byte[] GenerateKey(int size = 32)
    {
        var key = new byte[size];
        RandomNumberGenerator.Fill(key);
        return key;
    }

    public static int SecureRandomInt(int max)
    {
        byte[] data = new byte[4];
        RandomNumberGenerator.Fill(data);
        return Math.Abs(BitConverter.ToInt32(data, 0)) % max;
    }

    public static string ComputeHmac(byte[] key, string message)
    {
        using var hmac = new HMACSHA256(key);
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return BitConverter.ToString(hash).Replace("-", "").ToUpper();
    }
}

class FairRandomProtocol
{
    private byte[] _key;
    private int _value;
    private string _hmac;

    public FairRandomProtocol(int range)
    {
        _key = CryptoUtil.GenerateKey();
        _value = CryptoUtil.SecureRandomInt(range);
        _hmac = CryptoUtil.ComputeHmac(_key, _value.ToString());
    }

    public string GetHmac() => _hmac;

    public (int Value, string Key) Reveal()
    {
        return (_value, BitConverter.ToString(_key).Replace("-", "").ToUpper());
    }
}

class ProbabilityCalculator
{
    // Returns the probability that diceA wins over diceB
    public static double CalculateProbability(Dice diceA, Dice diceB)
    {
        int count = 0, total = diceA.GetSides() * diceB.GetSides();
        foreach (var a in diceA.Faces)
        {
            foreach (var b in diceB.Faces)
            {
                if (a > b) count++;
            }
        }
        return (double)count / total;
    }
}

class GameEngine
{
    private List<Dice> _diceArray;

    public GameEngine(List<Dice> diceArray)
    {
        _diceArray = diceArray;
    }

    public void Run()
    {
        Console.WriteLine("Let's determine who makes the first move.");
        var protocol = new FairRandomProtocol(2);
        Console.WriteLine($"I selected a random value in the range 0..1 (HMAC={protocol.GetHmac()}).");

        Console.WriteLine("Try to guess my selection: ");
        Console.WriteLine("0 - 0");
        Console.WriteLine("1 - 1");
        Console.WriteLine("X - exit");
        Console.WriteLine("? - help");

        string input;

        while (true)
        {
            input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (input.Equals("X", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Exiting game.");
                Environment.Exit(0);
            }
            if (input == "?")
            {
                DisplayHelp();
                continue;
            }
            if (input == "0" || input == "1")
                break;
            Console.WriteLine("Invalid input. Enter 0 or 1 (or X to exit, ? for help).");
        }

        int userGuess = int.Parse(input);
        var reveal = protocol.Reveal();
        Console.WriteLine($"My selection: {reveal.Value} (KEY={reveal.Key}).");
        bool userGoesFirst = userGuess == reveal.Value;
        Console.WriteLine(userGoesFirst ? "You make the first move." : "I make the first move.");

        Dice? userDice = null, computerDice = null;
        if (userGoesFirst)
        {
            Console.WriteLine("Choose your dice:");
            var availableIndices = Enumerable.Range(0, _diceArray.Count).ToList();
            PrintDiceOptions(availableIndices);
            int choice = GetValidDiceChoice(availableIndices.Count);
            userDice = _diceArray[availableIndices[choice]];
            computerDice = _diceArray.First(d => d != userDice);
        }
        else
        {
            int compChoice = CryptoUtil.SecureRandomInt(_diceArray.Count);
            computerDice = _diceArray[compChoice];
            Console.WriteLine($"I choose the [{string.Join(",", computerDice.Faces)}] dice.");
            Console.WriteLine("Choose your dice:");
            var availableIndices = GetAvailableDiceIndices(computerDice);
            PrintDiceOptions(availableIndices);
            int choice = GetValidDiceChoice(availableIndices.Count);
            userDice = _diceArray[availableIndices[choice]];
        }

        Console.WriteLine("It's time for my throw.");
        int compThrow = PerformThrow(computerDice);
        Console.WriteLine($"My throw is {compThrow}.");

        Console.WriteLine("It's time for your throw.");
        int userThrow = PerformThrow(userDice);
        Console.WriteLine($"Your throw is {userThrow}.");

        Console.WriteLine(userThrow > compThrow ? $"You win ({userThrow} > {compThrow})!" :
                          userThrow < compThrow ? $"I win ({compThrow} > {userThrow})!" :
                          $"It's a tie ({userThrow} = {compThrow})!");
    }

    private List<int> GetAvailableDiceIndices(Dice exclude)
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < _diceArray.Count; i++)
        {
            if (_diceArray[i] != exclude)
                indices.Add(i);
        }
        return indices;
    }

    private void PrintDiceOptions(List<int> indices)
    {
        for (int j = 0; j < indices.Count; j++)
        {
            int i = indices[j];
            Console.WriteLine($"{j} - [{string.Join(",", _diceArray[i].Faces)}]");
        }
        Console.WriteLine("X - exit");
        Console.WriteLine("? - help");
    }

    private int GetValidDiceChoice(int maxChoice)
    {
        while (true)
        {
            string line = Console.ReadLine()?.Trim() ?? string.Empty;
            if (line.Equals("X", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Exiting game.");
                Environment.Exit(0);
            }
            if (line == "?")
            {
                DisplayHelp();
                continue;
            }
            if (int.TryParse(line, out int choice) && choice >= 0 && choice < maxChoice)
                return choice;
            Console.WriteLine($"Invalid selection. Enter a number between 0 and {maxChoice - 1} (or X to exit, ? for help).");
        }
    }

    private int PerformThrow(Dice dice)
    {
        int sides = dice.GetSides();
        var protocol = new FairRandomProtocol(sides);
        Console.WriteLine($"I selected a random value in range 0..{sides - 1} (HMAC={protocol.GetHmac()}).");
        Console.WriteLine($"Add your number modulo 6.");
        for(int i = 0; i < sides; i++)
        {
            Console.WriteLine($"{i} - {i}");
        }
        Console.WriteLine("X - exit");
        Console.WriteLine("? - help");

        int userNum;
        while (true)
        {
            string line = Console.ReadLine()?.Trim() ?? string.Empty;
            if (line.Equals("X", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Exiting game.");
                Environment.Exit(0);
            }
            if (line == "?")
            {
                DisplayHelp();
                continue;
            }
            if (int.TryParse(line, out userNum) && userNum >= 0 && userNum < sides)
                break;
            Console.WriteLine($"Invalid input. Enter a number between 0 and {sides - 1} (or X to exit, ? for help).");
        }

        var reveal = protocol.Reveal();
        Console.WriteLine($"My number is {reveal.Value} (KEY={reveal.Key}).");
        int resultIndex = (reveal.Value + userNum) % sides;
        Console.WriteLine($"Result: {reveal.Value} + {userNum} = {resultIndex} (mod {sides})");
        return dice.GetFace(resultIndex);
    }

    private void DisplayHelp()
    {
        Console.WriteLine("\nHelp - Dice Game Rules:");
        Console.WriteLine("1. At the start, guess 0 or 1 to decide who goes first.");
        Console.WriteLine("2. You cannot choose the same dice as the opponent.");
        Console.WriteLine("3. During throws, enter a valid number within the specified range.");
        Console.WriteLine("4. At any prompt, type 'X' to exit the game or '?' to display this help message again.\n");
        Console.WriteLine("Probability of the win for the user:");
        Console.WriteLine(DisplayProbabilityTable());
    }

    private string DisplayProbabilityTable()
    {
        List<List<string>> rows = new List<List<string>>();
        List<string> header = new List<string>();
        header.Add("User dice v");
        foreach (var dice in _diceArray)
            header.Add(string.Join(",", dice.Faces));
        rows.Add(header);

        for (int i = 0; i < _diceArray.Count; i++)
        {
            List<string> row = new List<string>();
            string config = string.Join(",", _diceArray[i].Faces);
            row.Add(config);
            for (int j = 0; j < _diceArray.Count; j++)
            {
                if (i == j)
                    row.Add("-");
                else
                {
                    double prob = ProbabilityCalculator.CalculateProbability(_diceArray[i], _diceArray[j]);
                    row.Add(prob.ToString("0.0000"));
                }
            }
            rows.Add(row);
        }

        int colCount = rows[0].Count;
        int[] colWidths = new int[colCount];
        for (int j = 0; j < colCount; j++)
        {
            int maxWidth = 0;
            foreach (var row in rows)
            {
                if (row[j].Length > maxWidth)
                    maxWidth = row[j].Length;
            }
            colWidths[j] = maxWidth;
        }

        StringBuilder sb = new StringBuilder();
        string separator = BuildSeparator(colWidths);
        sb.AppendLine(separator);
        sb.AppendLine(BuildRow(rows[0], colWidths, isHeader: true));
        sb.AppendLine(separator);
        for (int i = 1; i < rows.Count; i++)
        {
            sb.AppendLine(BuildRow(rows[i], colWidths, isHeader: false));
            sb.AppendLine(separator);
        }
        return sb.ToString();
    }

    private string BuildRow(List<string> row, int[] colWidths, bool isHeader)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("|");
        for (int j = 0; j < row.Count; j++)
        {
            string cell = row[j].PadRight(colWidths[j]);
            if (isHeader)
            {
                cell = "\x1b[36m" + cell + "\x1b[0m";
            }
            sb.Append(" " + cell + " |");
        }
        return sb.ToString();
    }

    private string BuildSeparator(int[] colWidths)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("+");
        foreach (int w in colWidths)
        {
            sb.Append(new string('-', w + 2));
            sb.Append("+");
        }
        return sb.ToString();
    }
}

class Program
{
    static void Main(string[] args)
    {
        try
        {
            var diceArray = DiceParser.Parse(args);
            var game = new GameEngine(diceArray);
            game.Run();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }
}
