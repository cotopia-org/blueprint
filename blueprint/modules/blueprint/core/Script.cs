using blueprint.modules.blueprint.runtime;
using blueprint.modules.blueprint.runtime.tools;
using System.Text.RegularExpressions;
using Jint;
namespace blueprint.modules.blueprint.core
{
    public class Script
    {
        const string expressionPattern = @"{{(.*?)}}";
        public string code { get; set; }

        public Script(string code)
        {
            this.code = code;
        }
        public object AsExpression(ScriptInput scriptInput)
        {
            return ParseExpressions(code, scriptInput);
        }
        public void Invoke(string function, ScriptInput scriptInput)
        {
            run_script(code, scriptInput, false, function);
        }
        private object ParseExpressions(string input, ScriptInput scriptInput)
        {
            if (input == null)
                return null;

            var regex = new Regex(expressionPattern);
            var items = regex.Matches(input).Select(i => i.Groups[1].Value).ToList();
            var count = 0;
            var c = regex.Replace(input, match =>
            {
                count++;
                return "";
            });

            if (items.Count > 0)
            {
                var output = regex.Replace(input, match =>
                {
                    string expressionCode = match.Groups[1].Value;
                    return run_script(expressionCode, scriptInput, true)?.ToString();
                });

                return output;
            }
            else
            {
                return input;
            }
        }

        private object run_script(string code, ScriptInput scriptInput, bool expression, string functionName = null)
        {

            var engine = new Engine(options =>
            {
                options.LimitMemory(4_000_000);
                options.TimeoutInterval(TimeSpan.FromSeconds(1));
                options.MaxStatements(1000);
            });
            //{
            try
            {
                foreach (var hostObject in scriptInput.hostObjects)
                    engine.SetValue(hostObject.Key, hostObject.Value);

                engine.SetValue("httprequest", typeof(httprequest));
                // Execute the JavaScript code
                if (expression)
                {
                    var result = engine.Evaluate($"var result = ({code}); result;");
                    return result;
                }
                else
                {
                    if (functionName != null)
                    {
                        engine.Execute(code);
                        engine.Invoke(functionName);
                        return null;
                    }
                    else
                    {
                        var result = engine.Evaluate(code);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any JavaScript execution errors
                Console.WriteLine("JavaScript Error: " + ex.Message);
            }
            //}
            return null;
        }
    }
    public class ScriptInput
    {
        public List<KeyValuePair<string, object>> hostObjects = new List<KeyValuePair<string, object>>();
        public void AddHostObject(string varName, object value)
        {
            hostObjects.Add(new KeyValuePair<string, object>(varName, value));
        }
    }
}
