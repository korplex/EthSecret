using System;
using System.IO;
using System.Reflection;
using System.Threading;
using log4net;
using log4net.Config;

namespace EthSecret
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            new Thread(() =>
            {
                HashWatch.Activate();
            }).Start();

            for (var i = 0; i < 32; i++)
            {
                new Thread(() =>
                {
                    Console.WriteLine("Starting new thread !");
                    var ethParser = new EthWalletParser();
                    ethParser.Parse();
                }).Start();
            }
        }
    }
}
