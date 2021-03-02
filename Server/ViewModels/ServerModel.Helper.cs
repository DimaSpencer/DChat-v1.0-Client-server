using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public partial class ServerModel
    {
        private void DisplayColoredText(string text, ConsoleColor foregroundColor)
        {
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private void SendResponce(string message, TcpClient client)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            client.GetStream().Write(buffer);
        }
        private string GetDataFromClient(TcpClient client)
        {
            try
            {
                byte[] buffer = new byte[64];
                client.GetStream().Read(buffer);
                buffer = buffer.Where(b => b != 0).ToArray();

                if (buffer.Length <= 0)
                {
                    DisplayColoredText("Тут пришла строка в которой нету значения какбы",ConsoleColor.Red);
                    client.Close();
                    Console.ReadKey();
                    throw new Exception("Тут пришла строка в которой нету значения какбы");
                }
                return Encoding.UTF8.GetString(buffer);
            }
            catch (Exception)
            {
                DisplayColoredText("Ошибка получения данных с потока, отключаю пользователя", ConsoleColor.Red);
                client.Close();
                return string.Empty;
            }
        }
        private string[] ParseData(string data)
        {
            //извлекаем из данных пароль и логин
            string[] result = data.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return result;
        }

        private static bool VerifyHashedPassword(string hash, string inputData)
        {
            string hashOfInput = GetHash(inputData);
            return hashOfInput.Equals(hash);
        }
        private static string GetHash(string data)
        {
            byte[] buffer = HashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(data));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                builder.Append(buffer[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
