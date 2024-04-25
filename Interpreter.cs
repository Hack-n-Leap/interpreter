using System;
using System.Globalization;
using System.Text;

namespace InterpreterLib
{
    public class Variable
    {
        public string Name { get; }
        public string Value { get; }
        public string Type { get; }

        public Variable(string name, string value, string type)
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

        public void Execute(string[] parameters)
        {
            StringBuilder functionCode = new StringBuilder(Code); // Use of String Builder because the program need to replace a string by another.

            if (parameters.Length <= Var.Length) { throw new Exception("No enought parameters was given."); }
            if (parameters.Length >= Var.Length) { throw new Exception("Too much parameters was given."); }

            for (int i = 0; i < parameters.Length; i++) // Replace all parameters of the function by the corresponding values.
            {
                functionCode.Replace(Var[i], parameters[i]);
            }

            Interpreter functionInterpreter = new Interpreter(); // Create a new interpreter to execute the function code.
            functionInterpreter.EvaluateCode(functionCode.ToString()); 
        }

    }


    public class Interpreter
    {
        public Dictionary<string, Variable> Variables;

        public Interpreter()
        {
            Variables = new Dictionary<string, Variable>();
        }

        public void EvaluateCode(string code)
        {
            string[] lines = code.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("var "))
                {
                    EvaluateVariable(trimmedLine[4..]);
                }
                else if (trimmedLine.StartsWith("print "))
                {
                    EvaluatePrint(trimmedLine[6..]);
                }
                else
                {
                    throw new Exception($"Error. {trimmedLine} is not recognized as a correct expression. ");
                }
            }
        }

        public void EvaluateVariable(string line)
        {
            if (!line.Contains(" = ")) { throw new Exception("Unable to create a variable without value assignation."); }

            string[] var = line.Split(" = ");

            string varType = EvaluateType(var[1]);

            if (varType == "String")
            {
                var[1] = var[1][1..(var[1].Length - 1)];
            }
            else if (varType == "Variable")
            {
                varType = Variables[var[1]].Type;
                var[1] = Variables[var[1]].Value;
            }

            Variables[var[0]] = new Variable(var[0], var[1], varType);
        }

        public void EvaluatePrint(string line)
        {
            string lineType = EvaluateType(EvaluateType(line));

            if (lineType == "Variable")
            {
                Console.WriteLine(Variables[line].Value);
            }
            else if (lineType == "Operation")
            {
                Console.WriteLine(EvaluateOperations(line));
            }
            else if (lineType == "String")
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
                if (EvaluateType(part) == "Variable" && EvaluateType(Variables[part].Value) != "String")
                {
                    sum += double.Parse(Variables[part].Value, CultureInfo.InvariantCulture);
                }
                else if (EvaluateType(part) == "Integer" || EvaluateType(part) == "Float" || EvaluateType(part) == "Operation")
                {
                    sum += double.Parse(part, CultureInfo.InvariantCulture);
                }
                else
                {
                    throw new Exception($"Unable to add {EvaluateType(part)} to double !");
                }
            }

            return sum;
        }

        public double EvaluateSubstraction(string line)
        {
            string[] parts = line.Split(" - ")[1..];
            double sum;


            if (EvaluateType(line.Split(" - ")[0]) == "Variable" && Variables[line.Split(" - ")[0]].Type != "String")
            {
                sum = double.Parse(Variables[line.Split(" - ")[0]].Value);
            }
            else
            {
                sum = double.Parse(line.Split(" - ")[0]);
            }

            foreach (string part in parts)
            {
                if (EvaluateType(part) == "Variable" && EvaluateType(Variables[part].Value) != "String")
                {
                    sum -= double.Parse(Variables[part].Value, CultureInfo.InvariantCulture);
                }
                else if (EvaluateType(part) == "Integer" || EvaluateType(part) == "Float" || EvaluateType(part) == "Operation")
                {
                    sum -= double.Parse(part, CultureInfo.InvariantCulture);
                }
                else
                {
                    throw new Exception($"Unable to substract {EvaluateType(part)} to double !");
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
                if (EvaluateType(part) == "Variable" && EvaluateType(Variables[part].Value) != "String")
                {
                    total *= double.Parse(Variables[part].Value, CultureInfo.InvariantCulture);
                }
                else if (EvaluateType(part) == "Integer" || EvaluateType(part) == "Float" || EvaluateType(part) == "Operation")
                {
                    total *= double.Parse(part, CultureInfo.InvariantCulture);
                }
                else
                {
                    throw new Exception($"Unable to multiply {EvaluateType(part)} to double !");
                }
            }

            return total;
        }

        public double EvaluateDivision(string line)
        {
            string[] parts = line.Split(" / ")[1..];
            double total;

            if (parts.Contains("0")) { throw new Exception("Error. Unable to divide by zero."); }

            if (EvaluateType(line.Split(" / ")[0]) == "Variable" && Variables[line.Split(" / ")[0]].Type != "String")
            {
                total = double.Parse(Variables[line.Split(" / ")[0]].Value);
            }
            else
            {
                total = double.Parse(line.Split(" / ")[0]);
            }

            foreach (string part in parts)
            {
                if (EvaluateType(part) == "Variable" && EvaluateType(Variables[part].Value) != "String")
                {
                    total /= double.Parse(Variables[part].Value, CultureInfo.InvariantCulture);
                }
                else if (EvaluateType(part) == "Integer" || EvaluateType(part) == "Float" || EvaluateType(part) == "Operation")
                {
                    total /= double.Parse(part, CultureInfo.InvariantCulture);
                }
                else
                {
                    throw new Exception($"Unable to divide {EvaluateType(part)} to double !");
                }
            }

            return total;
        }

        public string EvaluateType(string value)
        {
            if ((value.StartsWith("'") && value.EndsWith("'")) || (value.StartsWith('"') && value.EndsWith('"')))
            {
                return "String";
            }
            else if (int.TryParse(value, out _))
            {
                return "Integer";
            }
            else if (double.TryParse(value, CultureInfo.InvariantCulture, out _))
            {
                return "Float";
            }
            else if (Variables.ContainsKey(value))
            {
                return "Variable";
            }
            else if (value.Contains('+') || value.Contains('*') || value.Contains('/') || value.Contains('-') || value.Contains('^') || value.Contains('%'))
            {
                return "Operation";
            }
            else
            {
                throw new Exception($"Error. Unable to get the type of {value}");
            }
        }
    }

}
