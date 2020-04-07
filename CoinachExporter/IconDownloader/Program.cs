using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using SaintCoinach;
using SaintCoinach.Ex;
using SaintCoinach.Xiv;

namespace IconDownloader
{
    class Program
    {
        private static Dictionary<string, int> marketableDict = new Dictionary<string, int>();
        private static Dictionary<int, string> eorzeaDbDict = new Dictionary<int, string>();

        static void Main(string[] args)
        {
            var mode = args[1];

            var outputPath = Path.Combine("..", "..", "..", "..", "public", "i", "icon2x");

            Directory.CreateDirectory(outputPath);

            var realmJp = new ARealmReversed(args[0], Language.Japanese);

            switch (mode)
            {
                case "alllist": 
                    MakeListAll(realmJp, outputPath);
                    return;
                case "allicon":
                    DownloadIconAll(realmJp, outputPath);
                    return;
            }
        }

        private static void MakeListAll(ARealmReversed realm, string outputPath)
        {
            Console.WriteLine("Starting game data export...");

            var itemsJp = realm.GameData.GetSheet<Item>();

            foreach (var category in realm.GameData.GetSheet<ItemSearchCategory>())
            {
                // We don't need those, not for sale
                if (category.Key == 0)
                    continue;

                foreach (var item in itemsJp.Where(item => item.ItemSearchCategory.Key == category.Key))
                {
                    marketableDict.Add(item.Name, item.Key);
                }
            }

            Console.WriteLine($"{marketableDict.Count} marketable items.");

            var pages = GetPageCount();

            for (var i = 1; i <= pages; i++)
            {
                // Can't access i in there cause it gets modified
                var thisPage = i;
                var searchPage = Retry.Do(() => Get(GetSearchUrl(thisPage)), TimeSpan.FromSeconds(20), 100);

                var tableNode =
                    searchPage.DocumentNode.SelectSingleNode(
                        "/html/body/div[3]/div[2]/div[1]/div/div[2]/div[2]/div[5]/div/table/tbody");

                var tableEntries = tableNode.SelectNodes("tr");

                Console.WriteLine($"=> Page {i}");

                foreach (var tableEntry in tableEntries)
                {
                    var itemRow = tableEntry.ChildNodes[1];
                    var itemDivs = itemRow.ChildNodes.Where(x => x.Name == "div");
                    var item1 = itemDivs.ElementAt(1);
                    var item2 = item1.ChildNodes[3];
                    var itemUrl = item2.GetAttributeValue("href", string.Empty);
                    var itemName = item2.InnerHtml;

                    Console.WriteLine($"    => {itemName}: {itemUrl}");
                    if (marketableDict.TryGetValue(itemName, out var key))
                    {
                        eorzeaDbDict.Add(key, itemUrl);
                        Console.WriteLine($"         => MARKETABLE");
                    }
                    else
                    {
                        Console.WriteLine($"         => NOT MARKETABLE");
                    }
                }

                File.WriteAllText(Path.Combine(outputPath, "dbMapping.json"), JsonConvert.SerializeObject(eorzeaDbDict, Formatting.Indented));
            }
        }

        private static void DownloadIconAll(ARealmReversed realm, string outputPath)
        {
            var dbEntries =
                JsonConvert.DeserializeObject<Dictionary<int, string>>(
                    File.ReadAllText(Path.Combine(outputPath, "dbMapping.json")));

            Parallel.For(0, dbEntries.Count, new ParallelOptions
            {
                MaxDegreeOfParallelism = 4
            }, (index) => {
                var dbEntry = dbEntries.ElementAt(index);
                if (DownloadIcon(dbEntry.Key, dbEntry.Value, outputPath))
                    Console.WriteLine($"         => DOWNLOADED: {dbEntry.Key}, {index}/{dbEntries.Count}");
            });
        }

        private static bool DownloadIcon(int key, string url, string outputPath)
        {
            if (File.Exists(Path.Combine(outputPath, $"{key}.png")))
            {
                Console.WriteLine($"         => ALREADY EXIST");
            }
            else
            {
                try
                {
                    var itemPage = Retry.Do<HtmlDocument>(
                        () => Get("https://jp.finalfantasyxiv.com" + url), TimeSpan.FromSeconds(5),
                        100);
                    var imageUrl = itemPage.DocumentNode
                        .SelectSingleNode(
                            "/html/body/div[4]/div[2]/div[1]/div/div[2]/div[2]/div[1]/div[1]/div[1]/img[2]")
                        .GetAttributeValue("src", string.Empty);

                    try
                    {
                        Retry.Do(() => SaveImage(imageUrl, key, outputPath), TimeSpan.FromSeconds(5), 100);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] COULD NOT FETCH ICON PNG: {imageUrl} \n{ex}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] COULD NOT FETCH ITEM PAGE: {url} \n{ex}");
                }
            }

            return false;
        }

        private static string GetSearchUrl(int page)
        {
            return $"https://jp.finalfantasyxiv.com/lodestone/playguide/db/item/?page={page}";
        }

        private static int GetPageCount()   
        {
            var url = GetSearchUrl(1);

            var doc = Get(url);
            
            var node = doc.DocumentNode.SelectSingleNode("/html/body/div[3]/div[2]/div[1]/div/div[2]/div[2]/div[6]/div/div/ul/li[9]/a");
            var sendTo = node.GetAttributeValue("href", "1");

            var pageNum = sendTo.Substring(sendTo.IndexOf('=') + 1).Substring(0, 3);

            return int.Parse(pageNum);
        }

        private static HtmlDocument Get(string url)
        {
            using (var client = new WebClient())
            {
                client.Headers = new WebHeaderCollection
                {
                    { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36"}
                };

                var data = client.DownloadData(url);
                var html = new HtmlDocument();
                html.LoadHtml(Encoding.UTF8.GetString(data));

                return html;
            }
        }

        private static void SaveImage(string url, int itemKey, string outputBase)
        {
            var outputPath = Path.Combine(outputBase, $"{itemKey}.png");

            using (var client = new WebClient())
            {
                client.Headers = new WebHeaderCollection
                {
                    { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36"}
                };

                client.DownloadFile(url, outputPath);
            }
        }
    }
}
