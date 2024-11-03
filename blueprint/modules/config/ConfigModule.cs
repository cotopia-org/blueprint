using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;
using srtool;
using blueprint.core;
using System.Collections;
namespace blueprint.modules.config
{
    public class ConfigModule : Module<ConfigModule>
    {

        private static readonly ConcurrentDictionary<string, string> configs = new ConcurrentDictionary<string, string>();
        public static void Init()
        {
            try
            {

                if (!File.Exists("config.conf"))
                    File.WriteAllText("config.conf",
                        @"
#CONFIG....
net:
   host: http://*:9339
mongodb:
   db-connection: mongodb://localhost
   db-name: automation
swagger:
    active: true
"
                        );
                var setupConfig = File.ReadAllText("config.conf");
                var className = "";
                foreach (string row in setupConfig.Split("\n"))
                {
                    if (row.Length > 0)
                    {
                        if (row.StartsWith('#'))
                        {
                            continue;
                        }
                        var items = row.Split(':');
                        if (items.Length > 1)
                        {
                            var key = items[0].Trim();
                            var value = string.Join(":", items.Skip(1)).Trim();

                            if (value.Trim() == "")
                            {
                                if (row.Trim().EndsWith(':'))
                                    className = row.Trim().Split(':')[0];
                            }
                            else
                            {
                                configs.TryAdd(className + "." + key, value);
                            }
                        }
                        else
                        {

                        }
                    }
                }

                foreach (DictionaryEntry envVar in Environment.GetEnvironmentVariables())
                {
                    var envKey = envVar.Key.ToString().Replace(">", ".").ToLower();
                    var envValue = envVar.Value.ToString();

                    // Add or override the configuration dictionary with environment variables
                    if (configs.ContainsKey(envKey))
                        configs[envKey] = envValue;
                    else
                        configs.TryAdd(envKey, envValue);
                }

            }
            catch (Exception ee)
            {
                Debug.Error(ee);
            }

        }
        public static string GetString(string key)
        {
            return GetString(key, "");
        }
        public static string GetString(string key, string alter = "")
        {
            if (configs.TryGetValue(key, out string _value))
                return _value;
            else
                return alter;
        }
        public static int GetInt(string key)
        {
            return GetInt(key, 0);
        }
        public static int GetInt(string key, int alter)
        {
            if (configs.TryGetValue(key, out string _value))
            {
                if (int.TryParse(_value, out int _int))
                {
                    return _int;
                }
                else
                    return alter;
            }
            else
                return alter;
        }
        public static bool GetBool(string key, bool alter)
        {
            if (configs.TryGetValue(key, out string _value))
            {
                if (bool.TryParse(_value, out bool _int))
                {
                    return _int;
                }
                else
                    return alter;
            }
            else
                return alter;
        }
    }
}