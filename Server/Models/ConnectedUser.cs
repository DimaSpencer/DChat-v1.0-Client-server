using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ConnectedUser
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public TcpClient TcpClient { get; set; }
        public ConnectedUser(User user, TcpClient tcpClient)//получаем обект с базы данных и связываем его с определенным TcpClient
        {
            Id = user.Id;
            Login = user.Login;
            TcpClient = tcpClient;
        }
    }
}
