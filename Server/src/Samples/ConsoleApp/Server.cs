using ServerDemo;
using System;

namespace Fleck.Samples.ConsoleApp
{
    class Server
    {
        static void Main()
        {
            InitService();
            var input = Console.ReadLine();
            while (input != "exit")
            {
                NetManager.Instance.BroadCastMsg(input);
                input = Console.ReadLine();
            }
        }

        static void InitService()
        {
            FleckLog.Level = LogLevel.Debug;
            NetManager.Instance.Connect();
        }
    }
}
