using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlocEncryptCmdServer
{
    public enum ConfigKey
    {
        ConfigSecret,
        UsersData,
    }

    public class ServerConfig
    {
        const string cfgFileName = "./config.txt";
        const string alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

        readonly Dictionary<string, string> configs;

        public ServerConfig()
        {
            configs = [];
        }

        public async Task LoadConfig()
        {
            configs.Clear();

            if (!File.Exists(cfgFileName))
            {
                StreamWriter sw = new(cfgFileName);
                await sw.WriteAsync("");
                sw.Close();
            }

            StreamReader sr = new(cfgFileName);
            var content = await sr.ReadToEndAsync();
            sr.Close();

            var configStrings = content
                        .Replace("\r", "")
                        .Split("\n")
                        .ToList();
            configStrings.RemoveAll(conf => !conf.Contains('='));

            foreach (var confStr in configStrings)
            {
                var pair = confStr.Split('=');
                configs.Add(pair[0].Trim(), pair[1].Trim());
            }

            if (!configs.ContainsKey(ConfigKey.ConfigSecret.ToString()))
            {
                Console.WriteLine("Введите кодовое слово для шифрования конфигурации (кириллица, только заглавные буквы):");
                string? secret;
                do
                {
                    secret = Console.ReadLine();

                    if (secret != null)
                    {
                        var flag = false;
                        foreach (var ch in secret)
                        {
                            if (!alphabet.Contains(ch))
                            {
                                flag = true;
                                break;
                            }
                        }

                        if (flag) continue;
                        break;
                    }
                } while (true);

                configs.Add(ConfigKey.ConfigSecret.ToString(), secret);
            }

            await SaveConfig();
        }

        public async Task SaveConfig()
        {
            if (configs.Count == 0) return;

            var output = configs.Select(cfg => cfg.Key + '=' + cfg.Value).Aggregate((a, b) => a + '\n' + b);

            StreamWriter sw = new(cfgFileName);
            await sw.WriteAsync(output);
            sw.Close();
        }

        public async Task SetValue(ConfigKey key, string value)
        {
            configs.Add(key.ToString(), value);
            await SaveConfig();
        }

        public string? GetValue(ConfigKey key)
        {
            if (configs.ContainsKey(key.ToString()))
            {
                return configs[key.ToString()];
            }

            return null;
        }
    }
}
