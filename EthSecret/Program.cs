using System;
using System.IO;
using System.Reflection;
using System.Threading;
using log4net;
using log4net.Config;
using System.Net;
using System.Threading.Tasks;
using Nethereum.Web3;

namespace EthSecret
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            Console.WriteLine("Number of threads ?");

            var maxThreads = int.Parse(Console.ReadLine());

            new Thread(HashWatch.Activate).Start();

            //ServicePointManager.DefaultConnectionLimit = maxThreads + 16;

            for (var i = 0; i < maxThreads; i++)
            {
                Task.Factory.StartNew(EthWalletParser.Run, TaskCreationOptions.LongRunning);
            }
        }
    }
}
