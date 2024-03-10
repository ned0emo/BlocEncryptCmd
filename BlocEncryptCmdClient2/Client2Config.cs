using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlocEncryptCmdClient2
{
    public enum ConfigKey
    {
        ChatSecret,
        Username,
    }

    public class Client2Config
    {
        const string cfgFileName = "./config.txt";
        const string alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

        readonly Dictionary<string, string> configs;

        public Client2Config()
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

            Console.WriteLine("Введите кодовое слово для шифрования сообщений (кириллица, только заглавные буквы)");
            Console.WriteLine("Если слово уже введено и менять его не нужно, нажмите Enter");
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

                    if (flag || secret.Length < 1)
                    {
                        if (!configs.ContainsKey(ConfigKey.ChatSecret.ToString())) continue;
                        break;
                    }

                    if (configs.ContainsKey(ConfigKey.ChatSecret.ToString()))
                    {
                        configs[ConfigKey.ChatSecret.ToString()] = secret;
                    }
                    else
                    {
                        configs.Add(ConfigKey.ChatSecret.ToString(), secret);
                    }
                    break;
                }
            } while (true);

            Console.WriteLine("Введите имя пользователя");
            Console.WriteLine("Если имя уже введено и менять его не нужно, нажмите Enter");
            string? username;
            do
            {
                username = Console.ReadLine();

                if (username != null)
                {
                    if (username.Length < 1)
                    {
                        if (!configs.ContainsKey(ConfigKey.Username.ToString())) continue;
                        break;
                    }
                    if (configs.ContainsKey(ConfigKey.Username.ToString()))
                    {
                        configs[ConfigKey.Username.ToString()] = username;
                    }
                    else
                    {
                        configs.Add(ConfigKey.Username.ToString(), username);
                    }
                    break;
                }
            } while (true);

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
