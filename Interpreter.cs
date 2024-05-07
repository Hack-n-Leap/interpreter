using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace InterpreterLib
{
    public class Type
    {
        public static readonly int STRING = 1;
        public static readonly int INTEGER = 2;
        public static readonly int FLOAT = 3;
        public static readonly int OPERATION = 4;
        public static readonly int VARIABLE = 5;
        public static readonly int FUNCTION = 6;

    }

    public class Variable
    {
        public string Name { get; }
        public string Value { get; }
        public int Type { get; }

        public Variable(string name, string value, int type)
        {
            Name = name; Value = value; Type = type;
        }

        public override string ToString()
        {
            return $"{Value} ({Type})";
        }
    }


    public class Function
    {
        public string Name { get; }
        public string Code { get; }
        public string[] Var { get; }

        public Function(string name, string code, string[] var)
        {
            Name = name; Code = code; Var = var;
        }

        public void Execute(string[] parameters, Interpreter interpreter)
        {
            StringBuilder functionCode = new StringBuilder(Code); // Use of String Builder because the program need to replace a string by another.

            if (parameters.Length < Var.Length) { throw new Exception("No enought parameters was given."); }
            if (parameters.Length > Var.Length) { throw new Exception("Too much parameters was given."); }
            if (Var.Length == 1 && Var[0] == "") { parameters = []; } // Case of a function without parameters

            Interpreter functionInterpreter = new Interpreter(); // Create a new interpreter to execute the function code.

            for (int i = 0; i < parameters.Length; i++) // Create local variables with the parameters given in parameters.
            {
                int parameterType = interpreter.EvaluateType(parameters[i]); 
                if (parameterType == Type.VARIABLE) { parameterType = interpreter.Variables[parameters[i]].Type; parameters[i] = interpreter.Variables[parameters[i]].Value; } // Replace variable name with value and type with the value type
                functionInterpreter.Variables[Var[i]] = new Variable(Var[i], parameters[i], parameterType); 
            }

            foreach (string functionName in interpreter.Functions.Keys) // Add all functions created in the main interpreter to the function interpreter
            {
                functionInterpreter.Functions[functionName] = interpreter.Functions[functionName];
            }

            functionInterpreter.EvaluateCode(functionCode.ToString()); 
        }

    }

    public class Interpreter
    {
        public Dictionary<string, Variable> Variables;
        public Dictionary<string, Function> Functions;

        public Interpreter()
        {
            Variables = new Dictionary<string, Variable>();
            Functions = new Dictionary<string, Function>();
        }

        public void EvaluateCode(string code)
        {
            string[] lines = code.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            int index = 0;

            while (index < lines.Length)
            {
                string line = lines[index];
                string trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("var ")) // Case of the registration / assignation of a value to a variable
                {
                    EvaluateVariable(trimmedLine[4..]);
                }
                else if (trimmedLine.StartsWith("print ")) // Case of a print
                {
                    EvaluatePrint(trimmedLine[6..]);
                }
                else if (trimmedLine.StartsWith("func "))
                { // Case of the registration of a function
                    StringBuilder functionText = new StringBuilder(trimmedLine[5..]);

                    index++;

                    while (index < lines.Length && lines[index].StartsWith("\t"))
                    { // Get all lines until their are no tabulation and the line don't finish by the '}' character.
                        functionText.Append($"\n{lines[index][1..]}");
                        index++;
                    }

                    EvaluateFunctionRegister(functionText.ToString());
                } else if (trimmedLine.StartsWith("for ")) // for var from x to y {}
                { // Case of the registration of a loop
                    string loopFirstLine = trimmedLine[4..];
                    StringBuilder loopCode = new StringBuilder();

                    //if (!(firstLine.EndsWith('{'))) { throw new Exception("Error. Invalid Syntax."); } // Throw new error in case of invalid syntax.
                    string loopVarName = loopFirstLine.Split(' ')[0]; // Get the title of the loopVar
                    string loopFromName = loopFirstLine.Split(' ')[2]; // Get the title of the loopFrom
                    string loopToName = loopFirstLine.Split(' ')[4]; // Get the title of the loopTo

                    int loopFromType = EvaluateType(loopFromName);
                    int loopToType = EvaluateType(loopToName);

                    if (loopFromType == Type.VARIABLE)
                    {
                        loopFromType = Variables[loopFromName].Type;
                        loopFromName = Variables[loopFromName].Value;
                    }
                    if (loopToType == Type.VARIABLE)
                    {
                        loopToType = Variables[loopToName].Type;
                        loopToName = Variables[loopToName].Value;
                    }
                    if (loopFromType != Type.INTEGER || loopToType != Type.INTEGER)
                    {
                        throw new Exception($"Error. {trimmedLine} is not recognized as a correct expression. ");
                    }

                    Variables[loopVarName] = new Variable(loopVarName, loopFromName, Type.INTEGER);

                    index++;

                    while (index < lines.Length && lines[index].StartsWith("\t")) //
                    { // Get all lines until their are no tabulation and the line don't finish by the '}' character.
                        loopCode.Append($"\n{lines[index][1..]}");
                        index++;
                    }

                    int from = int.Parse(loopFromName);
                    int to = int.Parse(loopToName);

                    // Console.WriteLine(loopText.ToString());

                    for (int i = from;  i < to + 1;  i++)
                    {
                        Variables[loopVarName] = new Variable(loopVarName, i.ToString(), Type.INTEGER);
                        this.EvaluateCode(loopCode.ToString());
                    }

                } 
                else if (EvaluateType(trimmedLine) == Type.FUNCTION)
                {
                    EvaluateFunctionCall(trimmedLine);
                 }
                else
                {
                    throw new Exception($"Error. {trimmedLine} is not recognized as a correct expression. ");
                }

                index++;
            }

        }

        public void EvaluateVariable(string line)
        {
            if (!line.Contains(" = ")) { throw new Exception("Unable to create a variable without value assignation."); }

            string[] var = line.Split(" = ");

            int varType = EvaluateType(var[1]);

            if (varType == Type.STRING)
            {
                var[1] = var[1][1..(var[1].Length - 1)];
            }
            else if (varType == Type.VARIABLE)
            {
                varType = Variables[var[1]].Type;
                var[1] = Variables[var[1]].Value;
            }

            Variables[var[0]] = new Variable(var[0], var[1], varType);
        }

        public void EvaluateFunctionRegister(string lines)
        {
            string firstLine = lines.Split("\n")[0]; // Store the first line to use it to get the function name and args
            string functionTitle = firstLine.Split('(')[0]; // Get the title of the function.

            int openBracketIndex = firstLine.IndexOf("(");
            int closeBracketIndex = firstLine.IndexOf(")");

            if (openBracketIndex ==  -1 || closeBracketIndex == -1 || !(firstLine.EndsWith('{')) || functionTitle.Length == 0) { throw new Exception("Error. Invalid Syntax."); } // Throw new error in case of invalid syntax.

            Function function = new Function(functionTitle, lines[(firstLine.Length + 1)..], firstLine[(openBracketIndex + 1)..closeBracketIndex].Split(", ")); // Create a new Function objet that store all the informations about the new function.
            Functions[function.Name] = function; // Register the created function into the program function dictionnary.

        }

        public void EvaluateFunctionCall(string line) 
        { 
            string functionName = line.Split('(')[0]; // Get the title of the function

            int openBracketIndex = line.IndexOf('(');
            int closeBracketIndex = line.IndexOf(')');

            Functions[functionName].Execute(line[(openBracketIndex + 1)..closeBracketIndex].Split(", "), this); // Execute the function with the parameters gives in the function call.

        }

        public void EvaluatePrint(string line)
        {
            int lineType = EvaluateType(line);

            if (lineType == Type.VARIABLE)
            {
                Console.WriteLine(Variables[line].Value);
            }
            else if (lineType == Type.OPERATION)
            {
                Console.WriteLine(EvaluateOperations(line));
            }
            else if (lineType == Type.STRING)
            {
                Console.WriteLine(line[1..(line.Length - 1)]);
            }
            else
            {
                Console.WriteLine(line);
            }
        }

        public double EvaluateOperations(string line)
        // This function manages the system of operations and redirects each operation to the corresponding operations function.
        // It take a calculation in string type as a parameter and return the result of it as a double.
        {
            int openBracketIndex = 0;
            int closeBracketIndex = 0;
            int expressionIndex = 0;
            string newExpression;

            StringBuilder expressionBuilder = new StringBuilder(line);

            if (line.StartsWith('(') && line.EndsWith(')')) { expressionBuilder = new StringBuilder(line[1..(line.Length - 1)]); } // Delete the bracket at the start and the end of the calculus.

            while (expressionIndex < expressionBuilder.Length)
            {
                if (expressionBuilder.ToString()[expressionIndex] == '(') { openBracketIndex = expressionIndex; }
                else if (expressionBuilder.ToString()[expressionIndex] == ')')
                {
                    closeBracketIndex = expressionIndex;
                    expressionBuilder.Replace(expressionBuilder.ToString()[(openBracketIndex)..(closeBracketIndex + 1)], EvaluateOperations(expressionBuilder.ToString()[(openBracketIndex + 1)..(closeBracketIndex)]).ToString());

                    expressionIndex = 0;
                }

                expressionIndex++;
            }

            newExpression = expressionBuilder.ToString();

            if (newExpression.Contains('+')) // Case of an addition
            {
                return EvaluateAddition(newExpression);

            }
            else if (newExpression.Contains('-')) // Case of a substraction
            {
                return EvaluateSubstraction(newExpression);
            }
            else if (newExpression.Contains('*')) // Case of a multiplication
            {
                return EvaluateMultiplication(newExpression);
            }
            else if (newExpression.Contains('/')) // Case of a division
            {
                return EvaluateDivision(newExpression);
            }
            else if (newExpression.Contains('^')) // Case of a power
            {
                return EvaluatePower(newExpression);
            }
            else if (double.TryParse(newExpression, out double _))
            {
                return double.Parse(newExpression);
            }
            else
            {
                throw new Exception($"Unexcepted operation : ${newExpression}");
            }
        }

        public double EvaluateAddition(string line)
        {
            string[] parts = line.Split(" + ");
            double sum = 0;

            foreach (string part in parts)
            {
                int partType = EvaluateType(part);
                if (partType == Type.VARIABLE && Variables[part].Type != Type.STRING)
                {
                    sum += double.Parse(Variables[part].Value, CultureInfo.InvariantCulture);
                }
                else if (partType == Type.INTEGER || partType == Type.FLOAT || partType == Type.OPERATION)
                {
                    sum += double.Parse(part, CultureInfo.InvariantCulture);
                }
                else
                {
                    throw new Exception($"Unable to add {partType} to double !");
                }
            }

            return sum;
        }

        public double EvaluateSubstraction(string line)
        {
            string[] parts = line.Split(" - ")[1..];
            double sum;


            if (EvaluateType(line.Split(" - ")[0]) == Type.VARIABLE && Variables[line.Split(" - ")[0]].Type != Type.STRING)
            {
                sum = double.Parse(Variables[line.Split(" - ")[0]].Value);
            }
            else
            {
                sum = double.Parse(line.Split(" - ")[0]);
            }

            foreach (string part in parts)
            {
                int partType = EvaluateType(part);
                if (partType == Type.VARIABLE && Variables[part].Type != Type.STRING)
                {
                    sum -= double.Parse(Variables[part].Value, CultureInfo.InvariantCulture);
                }
                else if (partType == Type.INTEGER || partType == Type.FLOAT || partType == Type.OPERATION)
                {
                    sum -= double.Parse(part, CultureInfo.InvariantCulture);
                }
                else
                {
                    throw new Exception($"Unable to substract {partType} to double !");
                }
            }

            return sum;
        }

        public double EvaluateMultiplication(string line)
        {
            string[] parts = line.Split(" * ");
            double total = 1;

            foreach (string part in parts)
            {
                int partType = EvaluateType(part);

                if (partType == Type.VARIABLE && Variables[part].Type != Type.STRING)
                {
                    total *= double.Parse(Variables[part].Value, CultureInfo.InvariantCulture);
                }
                else if (partType == Type.INTEGER || partType == Type.FLOAT || partType == Type.OPERATION)
                {
                    total *= double.Parse(part, CultureInfo.InvariantCulture);
                }
                else
                {
                    throw new Exception($"Unable to multiply {partType} to double !");
                }
            }

            return total;
        }

        public double EvaluateDivision(string line)
        {
            string[] parts = line.Split(" / ")[1..];
            double total;

            if (parts.Contains("0")) { throw new Exception("Error. Unable to divide by zero."); }

            if (EvaluateType(line.Split(" / ")[0]) == Type.VARIABLE && Variables[line.Split(" / ")[0]].Type != Type.STRING)
            {
                total = double.Parse(Variables[line.Split(" / ")[0]].Value);
            }
            else
            {
                total = double.Parse(line.Split(" / ")[0]);
            }

            foreach (string part in parts)
            {
                int partType = EvaluateType(part);
                if (partType == Type.VARIABLE && Variables[part].Type != Type.STRING)
                {
                    total /= double.Parse(Variables[part].Value, CultureInfo.InvariantCulture);
                }
                else if (partType == Type.INTEGER || partType == Type.FLOAT || partType == Type.OPERATION)
                {
                    total /= double.Parse(part, CultureInfo.InvariantCulture);
                }
                else
                {
                    throw new Exception($"Unable to divide {partType} to double !");
                }
            }

            return total;
        }

        public double EvaluatePower(string line)
        {
            string[] parts = line.Split(" ^ ")[1..];
            double pow;


            if (EvaluateType(line.Split(" ^ ")[0]) == Type.VARIABLE && Variables[line.Split(" ^ ")[0]].Type != Type.STRING)
            {
                pow = double.Parse(Variables[line.Split(" ^ ")[0]].Value);
            }
            else if (EvaluateType(line.Split(" ^ ")[0]) != Type.STRING)
            {
                pow = double.Parse(line.Split(" ^ ")[0]);
                
            } else
            {
                throw new Exception("Unable to calculate a string power.");
            }

            foreach (string part in parts)
            {
                int partType = EvaluateType(part);

                if (partType == Type.VARIABLE && Variables[part].Type != Type.STRING)
                {
                    pow = Math.Pow(pow, double.Parse(Variables[part].Value, CultureInfo.InvariantCulture));
                }
                else if (partType == Type.INTEGER || partType == Type.FLOAT || partType == Type.OPERATION)
                {
                    pow = Math.Pow(pow, double.Parse(part, CultureInfo.InvariantCulture));
                }
                else
                {
                    throw new Exception($"Unable to power {partType} to double !");
                }
            }

            return pow;
        }

        public int EvaluateType(string value)
        {
            if ((value.StartsWith("'") && value.EndsWith("'")) || (value.StartsWith('"') && value.EndsWith('"')))
            {
                return Type.STRING; // String
            }
            else if (int.TryParse(value, out _))
            {
                return Type.INTEGER; // Integer
            }
            else if (double.TryParse(value, CultureInfo.InvariantCulture, out _))
            {
                return Type.FLOAT; // Float
            }
            else if (Variables.ContainsKey(value))
            {
                return Type.VARIABLE; // Variable
            }
            else if (value.Contains('+') || value.Contains('*') || value.Contains('/') || value.Contains('-') || value.Contains('^') || value.Contains('%'))
            {
                return Type.OPERATION; // Operation
            }
            else if (Functions.ContainsKey(value.Split('(')[0]))
            {
                return Type.FUNCTION; // Function
            }
            else
            {
                throw new Exception($"Error. Unable to get the type of {value}");
            }
        }
    }

}
