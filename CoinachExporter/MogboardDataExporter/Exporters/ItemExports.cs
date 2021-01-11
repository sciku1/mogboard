using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lumina.Excel.GeneratedSheets;
using MogboardDataExporter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MogboardDataExporter.Exporters
{
    public static class ItemExports
    {
        public static void GenerateItemJSON(IEnumerable<Item> items, IEnumerable<Item> itemsDe, IEnumerable<Item> itemsFr, IEnumerable<Item> itemsJp, IEnumerable<CsvItem> itemsChs, IEnumerable<ItemSearchCategory> categories, string outputPath)
        {
            var ieBaseTop = Console.CursorTop;
            var counter = 0;
            Parallel.ForEach(categories, category =>
            {
                // We don't need those, not for sale
                if (category.RowId == 0)
                    goto console_update;

                var output = new List<JObject>();

                Parallel.ForEach(items.Where(item => item.ItemSearchCategory.Value.RowId == category.RowId), item =>
                {
                    dynamic outputItem = new JObject();

                    outputItem.ID = item.RowId;

                    var iconId = item.Icon;
                    outputItem.Icon = $"/i/{Util.GetIconFolder(iconId)}/{iconId:000000}.png";

                    outputItem.Name_en = (string)item.Name;
                    outputItem.Name_de = (string)itemsDe.First(localItem => localItem.RowId == item.RowId).Name;
                    outputItem.Name_fr = (string)itemsFr.First(localItem => localItem.RowId == item.RowId).Name;
                    outputItem.Name_jp = (string)itemsJp.First(localItem => localItem.RowId == item.RowId).Name;

                    var nameChs = itemsChs.FirstOrDefault(localItem => localItem.Key == item.RowId)?.Name;
                    outputItem.Name_chs = string.IsNullOrEmpty(nameChs) ? item.Name : nameChs;

                    outputItem.LevelItem = item.LevelItem.Value.RowId;
                    outputItem.Rarity = item.Rarity;

                    lock (output)
                    {
                        output.Add(outputItem);
                    }
                });
                output.Sort((item1, item2) => item1["ID"].ToObject<int>() - item2["ID"].ToObject<int>());

                if (output.Count == 0)
                    goto console_update;

                File.WriteAllText(Path.Combine(outputPath, $"ItemSearchCategory_{category.RowId}.json"),
                    JsonConvert.SerializeObject(output));

                console_update:
                Console.CursorLeft = 0;
                Console.CursorTop = ieBaseTop;
                Console.Write($"cat: [{counter}/{categories.Count() - 1}]");
                Console.CursorLeft = 10 + counter.ToString("000").Length;
                Console.Write("                                                                              ");

                counter++;
            });

            Console.WriteLine();
        }

        public static void GenerateMarketableItemJSON(IList<Item> items, IList<ItemSearchCategory> categories, string outputPath)
        {
            var mieBaseTop = Console.CursorTop;
            dynamic itemJSONOutput = new JObject();
            var itemID = new List<int>();
            foreach (var category in categories)
            {
                if (category.RowId < 9)
                    goto console_update;
                var itemSet = items
                    .AsParallel()
                    .Where(item => item.ItemSearchCategory.Value.RowId == category.RowId)
                    .Select(item => item.RowId)
                    .Select(Convert.ToInt32)
                    .ToList();
                if (!itemSet.Any())
                    goto console_update;
                itemID = itemID.Concat(itemSet).ToList();

                console_update:
                Console.CursorLeft = 0;
                Console.CursorTop = mieBaseTop;
                Console.Write($"cat: [{category.RowId}/{categories.Count - 1}]");
            }
            itemID.Sort();
            itemJSONOutput.itemID = JToken.FromObject(itemID);
            File.WriteAllText(Path.Combine(outputPath, "item.json"), JsonConvert.SerializeObject(itemJSONOutput));
            Console.WriteLine();
        }
    }
}