using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using static parsing.Program;
using CsvHelper;
using System.IO;
using System.Globalization;
using static Microsoft.FSharp.Core.ByRefKinds;
using System.Text.RegularExpressions;

namespace parsing
{
    internal class Program
    {
        public class ReceivedData
        {
            public string Currency { get; set; }
            public double Value { get; set; }

            public override string ToString()
            {
                return $"Currency: {Currency}, Value: {Value}";
            }

        }

        static void Main(string[] args)
        {

            var browser = new ScrapingBrowser();
            {
                browser.AllowAutoRedirect = true;
                browser.AllowMetaRedirect = true;
            }
    

            WebPage webpage = browser.NavigateToPage(new Uri("https://www.banki.ru/products/currency/cb/"));

            string htmlContent = webpage.Content;
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);


            var receivedDataList = new List<ReceivedData>();
           

            var curNodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='Text__sc-j452t5-0 fOLdnH currencyCbListItemstyled__StyledName-sc-12ajhcx-4 nLxpH']");
            var valNodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='Text__sc-j452t5-0 jxxlPG']");

            if (curNodes != null && valNodes != null) {
                for (int i = 0; i < curNodes.Count; i++)
                {
                    var currency = curNodes[i].InnerText.Trim();
                    var resultingValue = valNodes[i].InnerText;

                    string cleanedInput =""; 
                    foreach (char c in resultingValue)
                        if (char.IsDigit(c) || c=='.' || c==',')
                                {
                            cleanedInput += c;
                }


                    resultingValue = cleanedInput.Replace(',', '.');
                    if (double.TryParse(resultingValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                    {
                        var currencyAndValue = new ReceivedData() { Currency = currency, Value = value };
                        receivedDataList.Add(currencyAndValue);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to convert '{resultingValue}' to an double.");
                    }
                    
                }
                foreach (var i in receivedDataList)
                { 
                    Console.WriteLine(i); 
                }
                    
            }
            else
            {
                Console.WriteLine("No matching div elements found or mismatched counts.");
            }

            using (var writer = new StreamWriter("Currencies for today.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(receivedDataList);
            }

            Console.ReadLine();

        }
    
    }
}
