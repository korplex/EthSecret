using log4net;
using Nethereum.Web3;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EthSecret
{
    public enum EthGeth
    {
        Local = 0,
        Infura = 1
    }

    public class EthWalletParser
    {
        public static int AccountsTested;
        public static int PositiveAccounts;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(EthWalletParser));

        public static async Task Run()
        {
            var apiUrl = "http://localhost:8545";

            var web3 = new Web3(apiUrl);

            Console.WriteLine($"Starting on {apiUrl}.");

            string privateKey = null;
            string publicKey = null;

            try
            {
                while (true)
                {
                    privateKey = GeneratePrivateKey();
                    publicKey = Web3.GetAddressFromPrivateKey(privateKey);

                    var balanceContent = await web3.Eth.GetBalance.SendRequestAsync(publicKey);

                    var ethBalance = Web3.Convert.FromWei(balanceContent);

                    Console.WriteLine($"{privateKey} ({publicKey}) ----> {ethBalance} ETH");

                    if (ethBalance > 0)
                    {
                        Interlocked.Increment(ref PositiveAccounts);

                        Logger.Info($"{privateKey} ({publicKey}) ----> {ethBalance} ETH (unverified)");
                    }

                    Interlocked.Increment(ref AccountsTested);
                }
            }
            catch (Exception e)
            {
                Logger.Info($"Trouble with {privateKey} ({publicKey})...");
                Logger.Info(e.Message);
            }
        }

        public class BalanceResult
        {
            public string JsonRpc { get; set; }
            public int Id { get; set; }
            public string Result { get; set; }
        }

        public static string GeneratePrivateKey()
        {
            using (var generator = RandomNumberGenerator.Create())
            {
                var seed = new byte[32];

                generator.GetBytes(seed, 0, 32);

                return ByteArrayToHexString(seed);
            }
        }

        private const string HexAlphabet = "0123456789abcdef";

        public static string ByteArrayToHexString(byte[] bytes)
        {
            var result = new StringBuilder(bytes.Length * 2);

            foreach (var b in bytes)
            {
                result.Append(HexAlphabet[b >> 4]);
                result.Append(HexAlphabet[b & 0xF]);
            }

            return result.ToString();
        }
    }
}
