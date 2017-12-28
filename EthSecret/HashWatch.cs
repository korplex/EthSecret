using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace EthSecret
{
    public class HashWatch
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HashWatch));

        public static void Activate()
        {
            Logger.Info("Starting Eth Parser...");

            while (true)
            {
                var prevNumb = EthWalletParser.AccountsTested;

                Thread.Sleep(5000);

                var num = (EthWalletParser.AccountsTested - prevNumb) / 5;

                Console.Title = $"EthSecret - {EthWalletParser.PositiveAccounts}/{EthWalletParser.AccountsTested} - {num} PK/s";
            }
        }
    }
}
