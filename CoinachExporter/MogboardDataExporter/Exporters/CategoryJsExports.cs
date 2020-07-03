using MogboardDataExporter.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaintCoinach;
using SaintCoinach.Xiv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MogboardDataExporter.Models;

namespace MogboardDataExporter.Exporters
{
    public static class CategoryJsExports
    {
        public static void Generate(ARealmReversed realmEn, ARealmReversed realmDe, ARealmReversed realmFr, ARealmReversed realmJp, string outputPath)
        {
            string[] langs = { "en", "de", "fr", "ja" };
            ARealmReversed[] realms = { realmEn, realmDe, realmFr, realmJp };
            IXivSheet<Item>[] itemSheets = { realmEn.GameData.GetSheet<Item>(), realmDe.GameData.GetSheet<Item>(), realmFr.GameData.GetSheet<Item>(), realmJp.GameData.GetSheet<Item>() };

            var baseTop = Console.CursorTop;

            Parallel.For(0, 4, i =>
            {
                dynamic output = new JObject();
                var categories = realms[i].GameData.GetSheet<ItemSearchCategory>();
                foreach (var category in categories)
                {
                    if (category.Key < 9)
                        goto console_update;

                    var categoryItems = new List<string[]>();
                    var sortedItems = itemSheets[i].Where(item => item.ItemSearchCategory.Key == category.Key).ToList();
                    sortedItems.Sort((item1, item2) => item2.ItemLevel.Key - item1.ItemLevel.Key);

                    foreach (var item in sortedItems)
                    {
                        var outputItem = new string[6];

                        string classJobAbbr = item.ItemSearchCategory.ClassJob.Abbreviation;
                        if (item.ItemSearchCategory.ClassJob.ParentClassJob.Abbreviation != classJobAbbr)
                            classJobAbbr = item.ItemSearchCategory.ClassJob.ParentClassJob.Abbreviation + " " + classJobAbbr;
                        else if (Resources.ClassJobMap.TryGetValue(classJobAbbr, out var jobAbbr))
                            classJobAbbr += " " + jobAbbr;
                        else if (classJobAbbr == "ADV")
                            classJobAbbr = "";

                        outputItem[0] = item.Key.ToString();
                        outputItem[1] = item.Name.ToString();
                        outputItem[2] = $"/i/{item.Icon.Path.Substring(8, 13)}.png";
                        outputItem[3] = item.ItemLevel.ToString();
                        outputItem[4] = item.Rarity.ToString();
                        outputItem[5] = classJobAbbr;

                        categoryItems.Add(outputItem);
                    }

                    if (categoryItems.Count == 0)
                        goto console_update;

                    output[category.Key.ToString()] = JToken.FromObject(categoryItems);

                    console_update:
                    Console.CursorLeft = 0;
                    Console.CursorTop = baseTop + i;
                    Console.Write($"{langs[i]}: [{category.Key}/{categories.Count - 1}]");
                    Console.CursorLeft = 10 + category.Key.ToString("000").Length;
                    Console.Write("                                                                              ");
                }

                File.WriteAllText(Path.Combine(outputPath, $"categories_{langs[i]}.js"), JsonConvert.SerializeObject(output));
            });

            Console.CursorLeft = 0;
            Console.CursorTop = baseTop + 4;
        }

        public static void GenerateChinese(ARealmReversed realm, IEnumerable<CsvItem> itemsChs, string outputPath)
        {
            dynamic output = new JObject();

            var baseTop = Console.CursorTop;
            var localItems = realm.GameData.GetSheet<Item>();
            var categories = realm.GameData.GetSheet<ItemSearchCategory>();
            foreach (var category in categories)
            {
                if (category.Key < 9)
                    goto console_update;

                var categoryItems = new List<string[]>();
                var filteredItems = itemsChs.Where(item => item?.ItemSearchCategory.Key == category.Key).ToList();

                Parallel.ForEach(filteredItems, item =>
                {
                    var outputItem = new string[6];

                    var classJobAbbr = item.ItemSearchCategory.ClassJob.Abbreviation ?? "";
                    if (item.ItemSearchCategory.ClassJob.ParentClassJob.Abbreviation != classJobAbbr)
                        classJobAbbr = item.ItemSearchCategory.ClassJob.ParentClassJob.Abbreviation + " " +
                                       classJobAbbr;
                    else if (Resources.ClassJobMap.TryGetValue(classJobAbbr, out var jobAbbr))
                        classJobAbbr += " " + jobAbbr;
                    else if (classJobAbbr == "ADV")
                        classJobAbbr = "";

                    var iconId = (ushort) localItems.First(itm => itm.Key == item.Key).GetRaw("Icon");
                    var icon = $"/i/{Util.GetIconFolder(iconId)}/{iconId:000000}.png";

                    outputItem[0] = item.Key.ToString();
                    outputItem[1] = item.Name;
                    outputItem[2] = icon;
                    outputItem[3] = item.LevelItem.ToString();
                    outputItem[4] = item.Rarity.ToString();
                    outputItem[5] = classJobAbbr;

                    lock(categoryItems)
                    {
                        categoryItems.Add(outputItem);
                    }
                });

                categoryItems.Sort((item1, item2) => int.Parse(item2[3]) - int.Parse(item1[3]));

                if (categoryItems.Count == 0)
                    goto console_update;

                output[category.Key.ToString()] = JToken.FromObject(categoryItems);

                console_update:
                Console.CursorLeft = 0;
                Console.CursorTop = baseTop;
                Console.Write($"ch: [{category.Key}/{categories.Count - 1}]");
            }

            File.WriteAllText(Path.Combine(outputPath, "categories_chs.js"), JsonConvert.SerializeObject(output));

            Console.WriteLine();
        }
    }
}
