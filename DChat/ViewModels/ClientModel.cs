using System.Linq;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DevExpress.Mvvm;
using System.Windows.Input;

namespace DarkChat.wpf.ViewModel
{
    public enum Purpose
    {
        Login,
        Register
    }
    public partial class ClientModel : ViewModelBase
    {
        private const string _defaultIp = "127.0.0.1";
        private const int _defaultPort = 6060;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; RaisePropertyChanged("UserName"); }
        }
        private string _userName;
        public string Password
        {
            get { return _password; }
            set { _password = value; RaisePropertyChanged("Password"); }
        }
        public string _password;
        public string MessageBox
        {
            get { return _messageBox; }
            set { _messageBox = value; RaisePropertyChanged("MessageBox"); }
        }
        private string _messageBox;
        public string ChatBox
        {
            get { return _chatBox; }
            set { _chatBox = value; RaisePropertyChanged("ChatBox"); }
        }
        private string _chatBox; 
        public bool Connected
        {
            get { return _connected; }
            set { _connected = value; RaisePropertyChanged("Connected"); }
        }
        private bool _connected;
        public TcpClient TcpClient { get; private set; }
        public NetworkStream ClientStream { get; private set; }

        public AsyncCommand LoginCommand
        {
            get
            {
                return new AsyncCommand(() =>
                {
                    return Task.Factory.StartNew(Login);
                });
            }
        }
        public AsyncCommand RegisterCommand
        {
            get
            {
                return new AsyncCommand(() =>
                {
                    return Task.Factory.StartNew(Register);
                });
            }
        }
        public AsyncCommand SendMessageCommand
        {
            get
            {
                return new AsyncCommand(() =>
                {
                    return Task.Factory.StartNew(SendMessage);
                }, string.IsNullOrWhiteSpace(MessageBox) == false);
            }
        }

        private bool TryConnectToServer()
        {
            try
            {
                TcpClient = new TcpClient(_defaultIp, _defaultPort);
                ClientStream = TcpClient.GetStream();
                Connected = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void Disconnect()
        {
            Connected = false;
            TcpClient.Close();
        }

        private void Login()
        {
            if (TryConnectToServer() == false)
            {
                ChatBox += "Сервер не отвечает, повторите попытку позже\n";
                return;
            }

            SendRequest(UserName, Password, Purpose.Login);
            string responce = GetResponceFromServer();
            if (responce == "success")
            {
                ReceiveMessagesAsync();
                ChatBox += $"Вас успешно подключено к севрверу под ником {UserName}\n";
            }
            else
            {
                ChatBox += "Ошибка подключения: " + responce + "\n";
                Disconnect();
            }
        }
        private void Register()
        {
            if (TryConnectToServer() == false)
            {
                ChatBox += "Сервер не отвечает, повторите попытку позже\n";
                return;
            }
            SendRequest(UserName, Password, Purpose.Register);
            ChatBox += GetResponceFromServer();
            Disconnect();
        }

        private void SendRequest(string login, string password, Purpose purpose)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes($"{login} {password} {purpose}");
                ClientStream.Write(buffer);
            }
            catch
            {
                throw new Exception();
            }
        }
        private string GetResponceFromServer()
        {
            byte[] buffer = new byte[128];
            ClientStream.Read(buffer);
            buffer = buffer.Where(b => b != 0).ToArray();
            string responce = Encoding.UTF8.GetString(buffer);
            return responce;
        }

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageBox))
                return;
            ClientStream.Write(Encoding.UTF8.GetBytes(MessageBox.Trim()));
            ClientStream.Flush();
            MessageBox = string.Empty;
        }
        async private void ReceiveMessagesAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    while (TcpClient.Connected)
                    {
                        byte[] buffer = new byte[128];
                        ClientStream.Read(buffer);
                        buffer = buffer.Where(b => b != 0).ToArray();
                        ChatBox += Encoding.UTF8.GetString(buffer) +"\n";
                        Task.Delay(10).Wait();
                    }
                }
                catch (Exception)
                {
                    Disconnect();
                }
            });
        }
    }
}
