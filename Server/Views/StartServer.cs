using Server;

namespace Chat
{
    class StartServer
    {
        static void Main(string[] args)
        {
            ServerModel server = new ServerModel();
            server.Start();
        }
    }
}
