using blueprint.modules.blueprint.runtime;
using Microsoft.ClearScript.V8;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace blueprint.modules.blueprint.core
{
    public class Script
    {
        public string code { get; set; }

        public Script(string code)
        {
            this.code = code;
        }
        public object[] Expression(string objVarName, object fromObject)
        {
            return new object[] { ParseExpressions(objVarName, fromObject, code) };
        }
        public void Invoke(string objVarName, object fromObject, string function)
        {
            run_as_java_script(code, objVarName, fromObject, false, function);
        }
        const string expressionPattern = @"{{(.*?)}}";
        private object ParseExpressions(string objVarName, object fromObject, string input)
        {
            var regex = new Regex(expressionPattern);
            var items = regex.Matches(input).Select(i => i.Groups[1].Value).ToList();
            var count = 0;
            var c = regex.Replace(input, match =>
            {
                count++;
                return "";
            });

            if (count == 1)
            {
                return run_as_java_script(items[0], objVarName, fromObject, true)?.FirstOrDefault();
            }
            else
            if (items.Count > 0)
            {
                var output = regex.Replace(input, match =>
                {
                    // Extract the JavaScript code between {{ and }}
                    string expressionCode = match.Groups[1].Value;

                    return run_as_java_script(expressionCode, objVarName, fromObject, true)?.FirstOrDefault()?.ToString();

                });

                return output;
            }
            else
            {
                return run_as_java_script(items[0], objVarName, fromObject, true)?.FirstOrDefault();
            }
        }
        private object[] run_as_java_script(string code, string objectName, object fromObject, bool expression, string functionName = null)
        {
            // V8ScriptEngine.Current.Execute
          //  var stopwatch = new Stopwatch();
          //  stopwatch.Start();
            using (var engine = new V8ScriptEngine())
            {
                try
                {
                    engine.AddHostObject(objectName, fromObject);
                    engine.AddHostType("devtools", typeof(devtools));
                    // Execute the JavaScript code
                    if (expression)
                    {
                        var result = engine.Evaluate($"var result = ({code}); result;");
                      //  Console.WriteLine(stopwatch.ElapsedMilliseconds.ToString());
                        return new object[] { result };
                    }
                    else
                    {
                        if (functionName != null)
                        {
                            engine.Execute(code);
                            engine.Invoke(functionName);
                          //  Console.WriteLine(stopwatch.ElapsedMilliseconds.ToString());

                            return null;
                        }
                        else
                        {
                            var result = engine.Evaluate(code);
                          //  Console.WriteLine(stopwatch.ElapsedMilliseconds.ToString());
                            return new object[] { result };
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any JavaScript execution errors
                    Console.WriteLine("JavaScript Error: " + ex.Message);
                }
                //}
                //Console.WriteLine(stopwatch.ElapsedMilliseconds.ToString());

                return null;
            }
        }
    }
}
