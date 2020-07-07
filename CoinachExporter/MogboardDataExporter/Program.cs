using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using CsvHelper;
using Lumina;
using Lumina.Data;
using Lumina.Excel.GeneratedSheets;
using MogboardDataExporter.Exporters;
using MogboardDataExporter.Models;
using Cyalume = Lumina.Lumina;

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

            var lumina = new Cyalume(args[0]);
            var luminaDe = new Cyalume(args[0], new LuminaOptions { DefaultExcelLanguage = Language.German });
            var luminaFr = new Cyalume(args[0], new LuminaOptions { DefaultExcelLanguage = Language.French });
            var luminaJp = new Cyalume(args[0], new LuminaOptions { DefaultExcelLanguage = Language.Japanese });

            var items = lumina.GetExcelSheet<Item>().ToList();
            var itemsDe = luminaDe.GetExcelSheet<Item>().ToList();
            var itemsFr = luminaFr.GetExcelSheet<Item>().ToList();
            var itemsJp = luminaJp.GetExcelSheet<Item>().ToList();
            var itemsChs = GetChineseItems(lumina, http).ToList();

            var categories = lumina.GetExcelSheet<ItemSearchCategory>().ToList();

            Console.WriteLine($"Global items: {items.Count}");
            Console.WriteLine($"CN items: {itemsChs.Count}");

            Console.WriteLine("Starting game data export...");
            
            categoryjs:
            Console.WriteLine("== Category JS Export ==");
            CategoryJsExports.Generate(lumina, luminaDe, luminaFr, luminaJp, categoryJsOutputPath);
            CategoryJsExports.GenerateChinese(lumina, itemsChs, categoryJsOutputPath);

            items:
            Console.WriteLine("== Item Export ==");
            ItemExports.GenerateItemJSON(items, itemsDe, itemsFr, itemsJp, itemsChs, categories, outputPath);

            marketableitems:
            Console.WriteLine("== Marketable Item JSON Export ==");
            ItemExports.GenerateMarketableItemJSON(items, categories, outputPath);
            
            itemsearchcategories:
            Console.WriteLine("== Item Search Category Export ==");
            Console.Write("...");
            ItemSearchCategoryExports.GenerateJSON(lumina, outputPath);
            Console.WriteLine("Done!");

            Console.WriteLine("== Chinese Item Search Category Mappings Export ==");
            Console.Write("...");
            ItemSearchCategoryExports.GenerateChineseMappingsJSON(http, outputPath);
            Console.WriteLine("Done!");
            
            town_export:
            Console.WriteLine("== Town Export ==");
            Console.Write("...");
            TownExports.GenerateJSON(lumina, luminaDe, luminaFr, luminaJp, http, outputPath);
            Console.WriteLine("Done!");

            world_export:
            Console.WriteLine("== World Export ==");
            Console.Write("...");
            WorldExports.GenerateJSON(lumina, outputPath);
            Console.WriteLine("Done!");

            end:
            Console.WriteLine("All done!");
            Console.ReadKey();
        }

        private static IEnumerable<CsvItem> GetChineseItems(Cyalume lumina, HttpClient http)
        {
            var rawData = http.GetStreamAsync(new Uri("https://raw.githubusercontent.com/thewakingsands/ffxiv-datamining-cn/master/Item.csv")).GetAwaiter().GetResult();
            using var sr = new StreamReader(rawData);
            using var csv = new CsvReader(sr, CultureInfo.InvariantCulture);
            var iscSheet = lumina.GetExcelSheet<ItemSearchCategory>();
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
                    ItemSearchCategory = iscSheet.First(isc => isc.RowId == itemSearchCategory),
                });
            }
            return items;
        }
    }
}
