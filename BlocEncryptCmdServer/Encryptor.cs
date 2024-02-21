using System;
using System.Collections.Generic;
namespace BlocEncryptCmdServer
{
    public class Encryptor
    {
        //индексы для перестановки
        private readonly List<int> keys;

        //в конструктор передаем слово-ключ
        public Encryptor(string key)
        {
            keys = [];
            //сортируем элементы слова-ключа
            var sortedKey = key.OrderBy(x => x).ToList();

            foreach (char ch in key)
            {
                //проходим пооригиналу слова и получаем индекс букв в отсортированном ключе
                var index = sortedKey.IndexOf(ch);
                //убираем полученную букву, чтоб второй раз не получать ее индекс
                sortedKey[index] = '_';
                //добавляем индекс в массив для перестановки
                keys.Add(index);
            }
        }

        //шифрование
        //в иетод передаем сообщение
        public string Encrypt(string message)
        {
            //размер блока равен размеру слова-ключа
            var blockLength = keys.Count;

            //добавляем пробелы в конце, чтоб в последнем блоке сообщения было количество символов равным blockLength
            string spaces = "";
            var spaceCount = message.Length % blockLength == 0 ? 0 : blockLength - message.Length % blockLength;
            for (int i = 0; i < spaceCount; i++)
            {
                spaces += " ";
            }
            var fullMessage = message + spaces;

            //создаем массив для добавления зашифрованного сообщения
            var encryptedMessage = new List<char>();

            //делим оригинальное сообщение на блоки размером blockLength
            var blocks = fullMessage.Chunk(blockLength);
            //проходим по каждому блоку
            foreach (var block in blocks)
            {
                //добавляем в зашифрованный массив элементы блока по индексам в массиве keys
                for (int i = 0; i < blockLength; i++)
                {
                    encryptedMessage.Add(block[keys.IndexOf(i)]);
                }
            }

            //преобразуем массив в строку и возвращаем
            return new string(encryptedMessage.ToArray());
        }
    }
}
