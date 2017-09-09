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
        private static ILog _logger = LogManager.GetLogger(typeof(HashWatch));

        public static void Activate()
        {
            _logger.Info("Starting Eth Parser...");

            while (true)
            {
                var prevNumb = EthWalletParser.AccountsTested;

                Thread.Sleep(500);

                var num = (EthWalletParser.AccountsTested - prevNumb) / 0.5f;

                Console.Title = $"EthSecret - {EthWalletParser.PositiveAccounts}/{EthWalletParser.AccountsTested} - {num} PK/s";
            }
        }
    }
}
