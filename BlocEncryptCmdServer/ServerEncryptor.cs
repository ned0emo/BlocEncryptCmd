using System;
using System.Collections.Generic;
namespace BlocEncryptCmdServer
{
    public class ServerEncryptor
    {
        //индексы для перестановки
        private readonly List<int> keys;
        private readonly string key;

        //в конструктор передаем слово-ключ
        public ServerEncryptor(string key)
        {
            keys = [];
            this.key = key;
            //сортируем элементы слова-ключа
            var sortedKey = key.OrderBy(x => x).ToList();

            foreach (char ch in key)
            {
                var index = sortedKey.IndexOf(ch);
                sortedKey[index] = '_';
                keys.Add(index);
            }
        }

        public string Encrypt(string message)
        {
            var blockLength = keys.Count;

            string spaces = "";
            var spaceCount = message.Length % blockLength == 0 ? 0 : blockLength - message.Length % blockLength;
            for (int i = 0; i < spaceCount; i++)
            {
                spaces += " ";
            }
            var fullMessage = message + spaces;

            var encryptedMessage = new List<char>();

            var blocks = fullMessage.Chunk(blockLength);
            foreach (var block in blocks)
            {
                for (int i = 0; i < blockLength; i++)
                {
                    encryptedMessage.Add(block[keys.IndexOf(i)]);
                }
            }

            return new string(encryptedMessage.ToArray());
        }

        public string Decrypt(string message)
        {
            var blockLength = keys.Count;

            var decryptedMessage = new List<char>();

            var blocks = message.Chunk(blockLength);
            foreach (var block in blocks)
            {
                for (int i = 0; i < blockLength; i++)
                {
                    decryptedMessage.Add(block[keys[i]]);
                }
            }

            return new string(decryptedMessage.ToArray()).Trim();
        }
    }
}
