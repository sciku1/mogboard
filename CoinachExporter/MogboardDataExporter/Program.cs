using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using CsvHelper;
using MogboardDataExporter.Exporters;
using MogboardDataExporter.Models;
using SaintCoinach;
using SaintCoinach.Ex;
using SaintCoinach.Xiv;
using Directory = System.IO.Directory;
using Item = SaintCoinach.Xiv.Item;

namespace MogboardDataExporter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var outputPath = Path.Combine("..", "..", "..", "..", "DataExports");
            var categoryJsOutputPath = Path.Combine("..", "..", "..", "..", "public", "data");

            var http = new HttpClient();
            
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Directory.CreateDirectory(outputPath);

            var realm = new ARealmReversed(args[0], Language.English);
            var realmDe = new ARealmReversed(args[0], Language.German);
            var realmFr = new ARealmReversed(args[0], Language.French);
            var realmJp = new ARealmReversed(args[0], Language.Japanese);

            var items = realm.GameData.GetSheet<Item>();
            var itemsDe = realmDe.GameData.GetSheet<Item>();
            var itemsFr = realmFr.GameData.GetSheet<Item>();
            var itemsJp = realmJp.GameData.GetSheet<Item>();
            var itemsChs = GetChineseItems(realm, http).ToList();

            var categories = realm.GameData.GetSheet<ItemSearchCategory>();

            Console.WriteLine($"Global items: {items.Count}");
            Console.WriteLine($"CN items: {itemsChs.Count}");

            Console.WriteLine("Starting game data export...");
            
            categoryjs:
            Console.WriteLine("== Category JS Export ==");
            CategoryJsExports.Generate(realm, realmDe, realmFr, realmJp, categoryJsOutputPath);
            CategoryJsExports.GenerateChinese(realm, itemsChs, categoryJsOutputPath);

            items:
            Console.WriteLine("== Item Export ==");
            ItemExports.GenerateItemJSON(items, itemsDe, itemsFr, itemsJp, itemsChs, categories, outputPath);

            marketableitems:
            Console.WriteLine("== Marketable Item JSON Export ==");
            ItemExports.GenerateMarketableItemJSON(items, categories, outputPath);
            
            itemsearchcategories:
            Console.WriteLine("== Item Search Category Export ==");
            Console.Write("...");
            ItemSearchCategoryExports.GenerateJSON(realm, outputPath);
            Console.WriteLine("Done!");

            Console.WriteLine("== Chinese Item Search Category Mappings Export ==");
            Console.Write("...");
            ItemSearchCategoryExports.GenerateChineseMappingsJSON(http, outputPath);
            Console.WriteLine("Done!");
            
            town_export:
            Console.WriteLine("== Town Export ==");
            Console.Write("...");
            TownExports.GenerateJSON(realm, realmDe, realmFr, realmJp, http, outputPath);
            Console.WriteLine("Done!");

            world_export:
            Console.WriteLine("== World Export ==");
            Console.Write("...");
            WorldExports.GenerateJSON(realm, outputPath);
            Console.WriteLine("Done!");

            end:
            Console.WriteLine("All done!");
            Console.ReadKey();
        }

        private static IEnumerable<CsvItem> GetChineseItems(ARealmReversed realm, HttpClient http)
        {
            var rawData = http.GetStreamAsync(new Uri("https://raw.githubusercontent.com/thewakingsands/ffxiv-datamining-cn/master/Item.csv")).GetAwaiter().GetResult();
            using var sr = new StreamReader(rawData);
            using var csv = new CsvReader(sr, CultureInfo.InvariantCulture);
            var iscSheet = realm.GameData.GetSheet<ItemSearchCategory>();
            var items = new List<CsvItem>();
            for (var i = 0; i < 3; i++) csv.Read();
            while (csv.Read())
            {
                var itemSearchCategory = csv.GetField<int>(17);
                items.Add(new CsvItem
                {
                    Key = csv.GetField<int>(0),
                    Name = csv.GetField<string>(10),
                    LevelItem = csv.GetField<int>(12),
                    Rarity = csv.GetField<int>(13),
                    ItemSearchCategory = iscSheet.First(isc => isc.Key == itemSearchCategory),
                });
            }
            return items;
        }
    }
}
