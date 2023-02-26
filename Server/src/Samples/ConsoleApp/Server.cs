using ServerDemo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Fleck.Samples.ConsoleApp
{
    class Server
    {
        static void Main()
        {
            InitService();
            //FleckLog.Level = LogLevel.Debug;
            //NetManager.Instance.Connect();
            var input = Console.ReadLine();
            while (input != "exit")
            {
                NetManager.Instance.BroacastStringMsg(input);
                input = Console.ReadLine();
            }
        }

        static void InitService()
        {
            FleckLog.Level = LogLevel.Debug;
            NetManager.Instance.Connect();
            //GameManager.Instance.RegisterMessageListener();
        }
    }
}
