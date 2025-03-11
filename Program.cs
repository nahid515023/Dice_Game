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

        Console.WriteLine("Try to guess my selection (0/1): ");
        string input;
        while (true)
        {
            input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (input == "0" || input == "1")
                break;
            Console.WriteLine("Invalid input. Enter 0 or 1.");
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
            PrintDiceOptions();
            int choice = GetValidDiceChoice();
            userDice = _diceArray[choice];
            computerDice = _diceArray.First(d => d != userDice);
        }
        else
        {
            int compChoice = CryptoUtil.SecureRandomInt(_diceArray.Count);
            computerDice = _diceArray[compChoice];
            Console.WriteLine($"I choose the [{string.Join(",", computerDice.Faces)}] dice.");
            Console.WriteLine("Choose your dice:");
            PrintDiceOptions(computerDice);
            int choice = GetValidDiceChoice();
            userDice = _diceArray[choice];
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

    private void PrintDiceOptions(Dice? exclude = null)
    {
        for (int i = 0; i < _diceArray.Count; i++)
        {
            if (_diceArray[i] != exclude)
                Console.WriteLine($"{i} - [{string.Join(",", _diceArray[i].Faces)}]");
        }
    }

    private int GetValidDiceChoice()
    {
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 0 && choice < _diceArray.Count)
                return choice;
            Console.WriteLine("Invalid selection. Try again.");
        }
    }

    private int PerformThrow(Dice dice)
    {
        int sides = dice.GetSides();
        var protocol = new FairRandomProtocol(sides);
        Console.WriteLine($"I selected a random value in range 0..{sides - 1} (HMAC={protocol.GetHmac()}).");
        Console.WriteLine($"Enter a number (0-{sides - 1}): ");

        int userNum;
        while (!int.TryParse(Console.ReadLine(), out userNum) || userNum < 0 || userNum >= sides)
        {
            Console.WriteLine($"Invalid input. Enter a number between 0 and {sides - 1}.");
        }

        var reveal = protocol.Reveal();
        Console.WriteLine($"My number is {reveal.Value} (KEY={reveal.Key}).");
        int resultIndex = (reveal.Value + userNum) % sides;
        Console.WriteLine($"Result: {reveal.Value} + {userNum} = {resultIndex} (mod {sides})");
        return dice.GetFace(resultIndex);
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