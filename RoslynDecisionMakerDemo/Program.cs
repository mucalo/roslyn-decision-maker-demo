using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoslynDecisionMakerDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> inputVariables = new Dictionary<string, string>();
            inputVariables.Add("x", "30");
            inputVariables.Add("y", "40");

            Dictionary<string, string> calculatedVariables = new Dictionary<string, string>();
            calculatedVariables.Add("area", "x * y");

            string[] rules = {
                "area > 1000 && x < 2 * y",
                "area < 1000",
                "area > 1000 && y > 500"
            };

            Task.Run(async () =>
            {
                /// Code for the solution using Roslyn state
                Console.WriteLine("Using Roslyn state:");
                try
                {
                    var state = await CSharpScript.RunAsync("");

                    // Declare input variables
                    foreach (var item in inputVariables)
                    {
                        state = await state.ContinueWithAsync(String.Format("var {0} = {1};", item.Key, item.Value));
                    }

                    // Declare and calculate calculated variables
                    foreach (var item in calculatedVariables)
                    {
                        state = await state.ContinueWithAsync(String.Format("var {0} = {1};", item.Key, item.Value));
                    }

                    // Evaluate each condition
                    foreach (var rule in rules)
                    {
                        state = await state.ContinueWithAsync(rule);
                        Console.WriteLine(String.Format("Rule '{0}' was evaluated as {1}", rule, (bool)state.ReturnValue));
                    }
                }
                catch (CompilationErrorException e)
                {
                    Console.WriteLine(string.Join(Environment.NewLine, e.Diagnostics));
                }

                Console.WriteLine(Environment.NewLine);

                /// Code using replacing and evaluation of formulas
                Console.WriteLine("Using replacing and Roslyn evaluation of formulas:");
                try
                {
                    List<string> keys = new List<string>(calculatedVariables.Keys);
                    foreach (var key in keys)
                    {
                        foreach (var item in inputVariables)
                        {
                            calculatedVariables[key] = calculatedVariables[key].Replace(item.Key, item.Value);
                        };
                        calculatedVariables[key] = (await CSharpScript.EvaluateAsync(calculatedVariables[key])).ToString();
                    }


                    for (var i = 0; i < rules.Length; i++)
                    {
                        foreach (var item in inputVariables)
                        {
                            rules[i] = rules[i].Replace(item.Key, item.Value);
                        }
                        foreach (var item in calculatedVariables)
                        {
                            rules[i] = rules[i].Replace(item.Key, item.Value);
                        }
                        bool isRuleTrue = await CSharpScript.EvaluateAsync<bool>(rules[i]);
                        Console.WriteLine(String.Format("Rule '{0}' was evaluated as {1}", rules[i], isRuleTrue));
                    }

                }
                catch (CompilationErrorException e)
                {
                    Console.WriteLine(string.Join(Environment.NewLine, e.Diagnostics));
                }
            }).GetAwaiter().GetResult();
            Console.ReadKey();
        }
    }
}
