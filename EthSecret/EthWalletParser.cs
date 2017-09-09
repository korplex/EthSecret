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

namespace EthSecret
{
    public class EthWalletParser
    {
        private readonly Web3 _web3;

        public const string Url = "http://www.ethersecret.com/";
        private readonly ILog _logger = LogManager.GetLogger(typeof(EthWalletParser));

        public static int AccountsTested = 0;
        public static int PositiveAccounts = 0;

        public EthWalletParser()
        {
            _web3 = new Web3("http://localhost:8545");
        }

        public async Task Parse()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    while (true)
                    {
                        var privateKey = GeneratePrivateKey();

                        var publicKey = Web3.GetAddressFromPrivateKey(privateKey);

                        //var balance = await _web3.Eth.GetBalance.SendRequestAsync(publicKey);
                        //var ethBalance = Web3.Convert.FromWei(balance.Value, 18);

                        var req = new
                        {
                            method = "eth_getBalance",
                            @params = new[] { publicKey, "pending" },
                            id = 1,
                            jsonrpc = "2.0"
                        };

                        AccountsTested++;

                        var balanceResponse = client.PostAsync("http://localhost:8545",
                            new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8,
                                "application/json")).Result;

                        var balanceContent = await balanceResponse.Content.ReadAsStringAsync();

                        var ethBalanceObject = JsonConvert.DeserializeObject<BalanceResult>(balanceContent);

                        try
                        {
                            var ethBalance = Web3.Convert.FromWei(BigInteger.Parse(ethBalanceObject.Result.Substring(2), NumberStyles.HexNumber));

                            Console.WriteLine($"{privateKey} ({publicKey}) ----> {ethBalance} ETH");

                            if (ethBalance > 0)
                            {
                                PositiveAccounts++;
                                _logger.Info($"{privateKey} ({publicKey}) ----> {ethBalance} ETH");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            _logger.Info($"Trouble with {privateKey} ({publicKey})...");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
