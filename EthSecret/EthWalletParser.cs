using HtmlAgilityPack;
using Nethereum.Web3;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using Nethereum.Util;
using Newtonsoft.Json;
using System.Threading;

namespace EthSecret
{
    public enum EthGeth
    {
        Local = 0,
        Infura = 1
    }

    public class EthWalletParser
    {
        public static int AccountsTested = 0;
        public static int PositiveAccounts = 0;

        private static ILog _logger = LogManager.GetLogger(typeof(EthWalletParser));

        public static async Task Run(EthGeth api)
        {
            string _apiURL = "http://localhost:8545";

            switch (api)
            {
                case EthGeth.Infura:
                    _apiURL = "https://mainnet.infura.io/";
                    break;
            }

            var _web3 = new Web3(_apiURL);

            Console.WriteLine($"Starting on {_apiURL}.");

            string privateKey = null;
            string publicKey = null;

            try
            {
                using (var client = new HttpClient(new HttpClientHandler { UseProxy = false }))
                {
                    while (true)
                    {
                        privateKey = GeneratePrivateKey();
                        publicKey = Web3.GetAddressFromPrivateKey(privateKey);

                        try
                        {
                            var req = new
                            {
                                method = "eth_getBalance",
                                @params = new[] { publicKey, "pending" },
                                id = 1,
                                jsonrpc = "2.0"
                            };

                            using (var balanceResponse = await client.PostAsync(_apiURL, new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8, "application/json")))
                            {
                                var balanceContent = await balanceResponse.Content.ReadAsStringAsync();

                                var ethBalanceObject = JsonConvert.DeserializeObject<BalanceResult>(balanceContent);
                                var ethBalance = Web3.Convert.FromWei(BigInteger.Parse(ethBalanceObject.Result.Substring(2), NumberStyles.HexNumber));

                                Console.WriteLine($"{privateKey} ({publicKey}) ----> {ethBalance} ETH");

                                if (ethBalance > 0)
                                {
                                    Interlocked.Increment(ref PositiveAccounts);

                                    _logger.Info($"{privateKey} ({publicKey}) ----> {ethBalance} ETH");
                                }

                                Interlocked.Increment(ref AccountsTested);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e.Message);
                            _logger.Info($"Trouble with {privateKey} ({publicKey})...");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Info($"Trouble with {privateKey} ({publicKey})...");
                _logger.Info(e.Message);
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

        public static string ByteArrayToHexString(byte[] Bytes)
        {
            StringBuilder Result = new StringBuilder(Bytes.Length * 2);
            string HexAlphabet = "0123456789abcdef";

            foreach (byte B in Bytes)
            {
                Result.Append(HexAlphabet[(int)(B >> 4)]);
                Result.Append(HexAlphabet[(int)(B & 0xF)]);
            }

            return Result.ToString();
        }
    }
}
