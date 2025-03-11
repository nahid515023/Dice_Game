# 🎲 Non-Transitive Dice Game (C# Console App)

## 📌 Project Overview
This is a **C# console-based dice game** implementing **non-transitive dice mechanics** with **cryptographically fair random generation** using HMAC-SHA3. The game ensures that the computer does not cheat and allows the user to verify fairness.

---

## 🚀 Features
✅ **Fair Random Generation** - Uses cryptographic HMAC-SHA3 to prove fairness.  
✅ **Non-Transitive Dice** - Supports user-defined dice with arbitrary values.  
✅ **Command-Line Interface (CLI)** - Interactive console-based gameplay.  
✅ **Probabilities Table** - Displays winning probabilities using ASCII tables.  
✅ **Error Handling** - Handles invalid inputs and incorrect configurations.  

---

## 📂 Folder Structure
```
Dice_Game/  
│── .gitignore  
│── README.md  
│── Dice_Game.csproj  
│── Program.cs  
```

---

## 📦 Prerequisites
- **.NET SDK 6+** (Check installation: `dotnet --version`)
- **Git** (Optional, for version control)

---

## 🛠 Installation & Setup

### **1️⃣ Clone the Repository**
```sh
git clone https://github.com/yourusername/Dice_Game.git
cd Dice_Game
```

---

## ▶️ Running the Game
Run the game from the terminal with custom dice configurations:
```sh
dotnet run 2,2,4,4,9,9 1,1,6,6,8,8 3,3,5,5,7,7
```

### 📌 Gameplay Instructions:
- First, the game determines who plays first using a fair HMAC-based method.
- Each player chooses a dice (the second player cannot pick the same dice).
- Players roll the dice, and the highest roll wins.
- The user can verify fairness using the displayed HMAC values.

### 📊 Help Menu (Probability Table)
During gameplay, enter `?` to display win probabilities:

```
+-------------+-------------+-------------+-------------+
| User dice v | 2,2,4,4,9,9 | 1,1,6,6,8,8 | 3,3,5,5,7,7 |
+-------------+-------------+-------------+-------------+
| 2,2,4,4,9,9 | - (0.3333)  |    0.5556   |    0.4444   |
| 1,1,6,6,8,8 |    0.4444   | - (0.3333)  |    0.5556   |
| 3,3,5,5,7,7 |    0.5556   |    0.4444   | - (0.3333)  |
+-------------+-------------+-------------+-------------+
```

---

## 🛠 Example Runs

### ✅ Valid Run
```sh
dotnet run 2,2,4,4,9,9 1,1,6,6,8,8 3,3,5,5,7,7
```

### ❌ Invalid Input Handling
```sh
dotnet run 2,2,4,4,9,9 1,1,6,6,8,8  # ❌ Error: At least 3 dice are required.
dotnet run 2,2,X,X,9,9 1,1,6,6,8,8 3,3,5,5,7,7  # ❌ Error: Invalid integer in dice config.
```

---

## 📞 Contact
- **Developer:** Md Nahid Hasan  
- **GitHub:** [nahid515023](https://github.com/nahid515023)  
- **Email:** nahid515023@gmail.com  
