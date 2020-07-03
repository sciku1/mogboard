using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MogboardDataExporter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaintCoinach.Xiv;

namespace MogboardDataExporter.Exporters
{
    public static class ItemExports
    {
        public static void GenerateItemJSON(IXivSheet<Item> items, IXivSheet<Item> itemsDe, IXivSheet<Item> itemsFr, IXivSheet<Item> itemsJp, IEnumerable<CsvItem> itemsChs, IXivSheet<ItemSearchCategory> categories, string outputPath)
        {
            var ieBaseTop = Console.CursorTop;
            foreach (var category in categories)
            {
                // We don't need those, not for sale
                if (category.Key == 0)
                    goto console_update;

                var output = new List<JObject>();

                Parallel.ForEach(items.Where(item => item.ItemSearchCategory.Key == category.Key), item =>
                {
                    dynamic outputItem = new JObject();

                    outputItem.ID = item.Key;

                    var iconId = (ushort) item.GetRaw("Icon");
                    outputItem.Icon = $"/i/{Util.GetIconFolder(iconId)}/{iconId}.png";

                    outputItem.Name_en = item.Name.ToString();
                    outputItem.Name_de = itemsDe.First(localItem => localItem.Key == item.Key).Name.ToString();
                    outputItem.Name_fr = itemsFr.First(localItem => localItem.Key == item.Key).Name.ToString();
                    outputItem.Name_jp = itemsJp.First(localItem => localItem.Key == item.Key).Name.ToString();

                    var nameChs = itemsChs.FirstOrDefault(localItem => localItem.Key == item.Key)?.Name.ToString();
                    outputItem.Name_chs = string.IsNullOrEmpty(nameChs) ? item.Name.ToString() : nameChs;

                    outputItem.LevelItem = item.ItemLevel.Key;
                    outputItem.Rarity = item.Rarity;

                    lock (output)
                    {
                        output.Add(outputItem);
                    }
                });
                output.Sort((item1, item2) => item1["ID"].ToObject<int>() - item2["ID"].ToObject<int>());

                if (output.Count == 0)
                    goto console_update;

                File.WriteAllText(Path.Combine(outputPath, $"ItemSearchCategory_{category.Key}.json"), JsonConvert.SerializeObject(output));

                console_update:
                Console.CursorLeft = 0;
                Console.CursorTop = ieBaseTop;
                Console.Write($"cat: [{category.Key}/{categories.Count - 1}]");
            }

            Console.WriteLine();
        }

        public static void GenerateMarketableItemJSON(IXivSheet<Item> items, IXivSheet<ItemSearchCategory> categories, string outputPath)
        {
            var mieBaseTop = Console.CursorTop;
            dynamic itemJSONOutput = new JObject();
            var itemID = new List<int>();
            foreach (var category in categories)
            {
                if (category.Key < 9)
                    goto console_update;
                var itemSet = items
                    .AsParallel()
                    .Where(item => item.ItemSearchCategory.Key == category.Key)
                    .Select(item => item.Key)
                    .ToList();
                if (!itemSet.Any())
                    goto console_update;
                itemID = itemID.Concat(itemSet).ToList();

                console_update:
                Console.CursorLeft = 0;
                Console.CursorTop = mieBaseTop;
                Console.Write($"cat: [{category.Key}/{categories.Count - 1}]");
            }
            itemID.Sort();
            itemJSONOutput.itemID = JToken.FromObject(itemID);
            File.WriteAllText(Path.Combine(outputPath, "item.json"), JsonConvert.SerializeObject(itemJSONOutput));
            Console.WriteLine();
        }
    }
}