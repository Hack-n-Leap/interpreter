<p align="center">
  <img alt="Contributors" src="https://img.shields.io/github/contributors/Hack-n-Leap/interpreter?style=for-the-badge">
  <img alt="Commit Activity" src="https://img.shields.io/github/commit-activity/m/Hack-n-leap/interpreter?style=for-the-badge">
  <img alt="Stars" src="https://img.shields.io/github/stars/Hack-n-leap/interpreter?style=for-the-badge">
  <img alt="Issues" src="https://img.shields.io/github/issues/Hack-n-leap/interpreter?style=for-the-badge">
  <img alt="License" src="https://img.shields.io/github/license/Hack-n-Leap/interpreter?style=for-the-badge">
</p>

<p align="center">
  <img width="250px" src="https://github.com/Hack-n-Leap/game/assets/79806369/d94f47c7-9f6d-4ecd-8739-44ee7bf6dd6b" alt="Hack'n'Leap LOGO" align="center">
  <h1 align="center">Hack'n'Leap Interpreter</h1>
</p>

> [!WARNING]
> **The project has just been launched.** 
> **The basics of it have not yet been implemented.**

## About the project
Hack'n'Leap's interpreter is the library used in the game of the same name to enable players to code their own functions.
This interpreter supports fairly basic pseudo-code instructions.

## Use
### How to use ?
1. Build the solution
2. Import the `.dll` file as a reference in your project
3. Import the library in your code using `using InterpreterLib;`
4. You can now use the library !

### Sample use
```csharp
using InterpreterLib;
using System.Text;

public class Program
{
    static void Main(string[] args)
    {
        Console.Clear();
        Console.Title = "Interpreteur";
        Console.WriteLine("Code :");
        StringBuilder stringBuilder = new StringBuilder();
        string line;

        while (!string.IsNullOrWhiteSpace(line = Console.ReadLine())) { stringBuilder.AppendLine(line); }

        string myCode = stringBuilder.ToString();
        Interpreter myInterpreter = new Interpreter();

        Console.WriteLine("Exit : ");
        myInterpreter.EvaluateCode(myCode);
    }
}
```
