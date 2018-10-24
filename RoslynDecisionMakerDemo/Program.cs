using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;

namespace RoslynDecisionMakerDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> inputVariables = new Dictionary<string, string>();
            inputVariables.Add("a", "30");
            inputVariables.Add("b", "40");

            Dictionary<string, string> calculatedVariables = new Dictionary<string, string>();
            calculatedVariables.Add("area", "{{a}} * {{b}}");

            string ruleCondition = "{{area}} > 1000 && {{a}} < 2 * {{b}}";
            string ruleOutput = "The condition was evaluated as true.";

            /// Replace variable names with variable values in all calculated variables
            var calculatedVariableKeys = new List<string>(calculatedVariables.Keys);
            foreach (var calculatedVariableKey in calculatedVariableKeys)
            {
                foreach (var inputVariable in inputVariables)
                {
                    calculatedVariables[calculatedVariableKey] = calculatedVariables[calculatedVariableKey].Replace("{{" + inputVariable.Key + "}}", inputVariable.Value);
                }
            }

            /// Evaluate the values of calculated variables
            /// and store them in a new dictionary
            Dictionary<string, string> calculatedVariablesEvaluated = new Dictionary<string, string>();
            foreach (var calculatedVariable in calculatedVariables)
            {
                string calculatedValue = String.Empty;
                try
                {
                    calculatedValue = CSharpScript.EvaluateAsync(calculatedVariable.Value).Result.ToString();
                }
                catch (CompilationErrorException e)
                {
                    Console.WriteLine(string.Join(Environment.NewLine, e.Diagnostics));
                    return;
                }

                calculatedVariablesEvaluated.Add(calculatedVariable.Key, calculatedValue);
            }

            /// Replace input and calculated variables in rule condition with appropriate values
            foreach (var inputVariable in inputVariables)
            {
                ruleCondition = ruleCondition.Replace("{{" + inputVariable.Key + "}}", inputVariable.Value);
            }
            foreach (var calculatedVariableEvaluated in calculatedVariablesEvaluated)
            {
                ruleCondition = ruleCondition.Replace("{{" + calculatedVariableEvaluated.Key + "}}", calculatedVariableEvaluated.Value);
            }
            bool isConditionTrue;
            try
            {
                isConditionTrue = CSharpScript.EvaluateAsync<bool>(ruleCondition).Result;
            }
            catch (CompilationErrorException e)
            {
                Console.WriteLine(string.Join(Environment.NewLine, e.Diagnostics));
                return;
            }

            /// If condition is true, return the output
            if (isConditionTrue)
            {
                Console.WriteLine(ruleOutput);
            }
            Console.ReadKey();
            
        }
    }
}
