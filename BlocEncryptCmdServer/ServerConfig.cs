using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlocEncryptCmdServer
{
    public class ServerConfig
    {
        const string cfgFileName = "./config.txt";
        const string alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

        List<User> users;

        public ServerConfig()
        {
            users = [];
        }

        public async Task LoadConfig()
        {
            users.Clear();

            if (!File.Exists(cfgFileName))
            {
                using StreamWriter sw = new(cfgFileName);
                await sw.WriteAsync("");
                sw.Close();
            }

            using StreamReader sr = new(cfgFileName);
            var content = await sr.ReadToEndAsync();
            sr.Close();

            try
            {
                users = JsonSerializer.Deserialize<List<User>>(content)!;
            }
            catch
            {
                Console.WriteLine("Не найдены пользователи.");

                await AddUser();
            }
        }

        public async Task SaveConfig()
        {
            if (users.Count == 0) return;

            var output = JsonSerializer.Serialize(users);

            using StreamWriter sw = new(cfgFileName);
            await sw.WriteAsync(output);
            sw.Close();
        }

        public async Task AddUser()
        {
            string? username;
            Console.WriteLine("Введите имя пользователя");
            do
            {
                username = Console.ReadLine();
            } while (username == null);

            Console.WriteLine("Введите кодовое слово для шифрования пользователя (кириллица, только заглавные буквы):");
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

            var user = new User()
            {
                Name = username!,
                ChatSecret = secret!,
            };
            users.Add(user);

            await SaveConfig();
        }

        public bool CheckUser(string name, string secret)
        {
            return users.Any(user => user.Name == name && user.ChatSecret == secret);
        }
    }
}
