using Server.Models;
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
        public string IP { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 6060;

        public DarkChatDbContext Database { get; set; }
        public static HashAlgorithm HashAlgorithm { get; } = SHA256.Create();
        public TcpListener ServerSocket { get; set; }
        public List<ConnectedUser> ConnectedUsers { get; set; }

        public ServerModel()
        {
            ServerSocket = new TcpListener(IPAddress.Parse(IP), Port);
            ConnectedUsers = new List<ConnectedUser>();
        }
        public void Start()
        {
            ServerSocket.Start();

            DisplayColoredText("Сервер запущен!", ConsoleColor.White);
            while (true)
            {
                TcpClient client = ServerSocket.AcceptTcpClient();
                GetAndParseUserData(client); //начинаем читать пользователя чтобы узнать что он хочет, авторизоватся или зарегистрироватся
            }
        }
        public void Stop()
        {
            ConnectedUsers.Clear();
            ServerSocket.Stop();
            DisplayColoredText("Сервер закрыт", ConsoleColor.Red);
        }

        private void GetAndParseUserData(TcpClient client) //парсим входные даные юзера
        {
            string data = GetDataFromClient(client);

            if (data == string.Empty)
                return;

            string[] inputData = ParseData(data);

            string login = inputData[0]; 
            string password = inputData[1];
            string purpose = inputData[2];

            if (purpose == Purpose.Register.ToString())
                Register(login, password, client);
            else if (purpose == Purpose.Login.ToString())
                Autorization(login, password, client);
            else
            {
                client.Close();
                throw new ArgumentException(purpose);
            }
        }
        private void Register(string inputLogin, string inputPassword, TcpClient client)
        {
            DisplayColoredText($"Новый пользователь под именем {inputLogin} регистрируется на сервере", ConsoleColor.Yellow);

            using var Database = new DarkChatDbContext();
            bool userExists = Database.Users.Any(u => u.Login == inputLogin.ToLower());
            if (userExists == false)
            {
                string hashedPassword = GetHash(inputPassword);
                User newUser = new User() { Login = inputLogin, Password = hashedPassword };
                Database.Users.Add(newUser);
                Database.SaveChanges();

                SendResponce("success", client);
                DisplayColoredText($"Пользователь под ником {newUser.Login} успешно зарегистрировался под id: {newUser.Id}", ConsoleColor.Green);
            }
            else
                SendResponce("Fail: Пользователь с таким ником уже зарегистрирован на сервере", client);
        }
        private void Autorization(string inputLogin, string inputPassword, TcpClient client)
        {
            DisplayColoredText($"Пользователь {inputLogin} пытается войти", ConsoleColor.Yellow);
            using var Database = new DarkChatDbContext();
            User user = Database.Users.FirstOrDefault(u => u.Login == inputLogin);
            if (user?.Login == inputLogin && VerifyHashedPassword(user.Password, inputPassword))//проверка данных пользователя
            {
                //TODO: Реализовать нормальную проверку есть ли уже этот пользователь на сервере или его нету
                bool userIsPresentOnTheServer = ConnectedUsers.Any(u => u.Login == inputLogin);
                if (userIsPresentOnTheServer == false)
                {
                    SendResponce("success", client);
                    DisplayColoredText($"Пользователь {user.Login} идентифицирован",ConsoleColor.Green);
                    SendMessageToAllUsersAsync(null, $"Пользователь {user.Login} вошел на сервер");

                    ConnectedUser connectedUser = new ConnectedUser(user, client);
                    ConnectedUsers.Add(connectedUser);
                    ListenMessageFromUserAsync(connectedUser);
                }
                else
                {
                    SendResponce("Fail: Такой аккаунт уже присутствует на сервере", client);
                    DisplayColoredText($"Warning! Произошла попытка входа на один аккаунт с двух разных клиентов, на аккаунт пользователя под именем {inputLogin}"
                        , ConsoleColor.Red);
                }
            }
            else
            {
                SendResponce("Fail: Неверные данные авторизации", client);
                DisplayColoredText($"Пользователь {inputLogin} не идентифицирован", ConsoleColor.Red);
            }
        }

        public void DisableClient(ConnectedUser user)
        {
            ConnectedUsers.Remove(user);
            user?.TcpClient?.Close();
            SendMessageToAllUsersAsync(null, $"{user.Login} отключился от сервера");
            DisplayColoredText($"Соеденение с пользователем {user.Login} разорвано", ConsoleColor.Red);
        } 
        async private void ListenMessageFromUserAsync(ConnectedUser user)
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    string message = GetDataFromClient(user.TcpClient); //метод GetDataFromClient в случай ошибки отключит пользователя от сервера
                    if (message == string.Empty)
                    {
                        ConnectedUsers.Remove(user);
                        return;
                    }

                    SendMessageToAllUsersAsync(sender: user, message);
                    Thread.Sleep(400);
                }
            });
        }
        async private void SendMessageToAllUsersAsync(ConnectedUser sender, string message)
        {
            await Task.Run(() =>
            {
                string resultMessage = $"[{DateTime.Now}] {sender?.Login}: {message}"; //TODO: Тут подумать про то что мы можем изменять изменять сообзения по хотению
                DisplayColoredText(resultMessage, ConsoleColor.White);

                foreach (var user in ConnectedUsers)//TODO:Тут была сомнительная проверка if (user.TcpClient.Connected)
                    SendResponce(resultMessage, user.TcpClient);
            });
        }
    }
}
