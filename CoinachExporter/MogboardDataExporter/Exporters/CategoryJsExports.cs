using MogboardDataExporter.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lumina.Excel.GeneratedSheets;
using MogboardDataExporter.Models;
using Cyalume = Lumina.Lumina;

namespace MogboardDataExporter.Exporters
{
    public static class CategoryJsExports
    {
        public static void Generate(Cyalume luminaEn, Cyalume luminaDe, Cyalume luminaFr, Cyalume luminaJp, string outputPath)
        {
            string[] langs = { "en", "de", "fr", "ja" };
            Cyalume[] realms = { luminaEn, luminaDe, luminaFr, luminaJp };
            IEnumerable<Item>[] itemSheets = { luminaEn.GetExcelSheet<Item>(), luminaDe.GetExcelSheet<Item>(), luminaFr.GetExcelSheet<Item>(), luminaJp.GetExcelSheet<Item>() };

            var baseTop = Console.CursorTop;

            Parallel.For(0, 4, i =>
            {
                dynamic output = new JObject();
                var categories = realms[i].GetExcelSheet<ItemSearchCategory>();
                foreach (var category in categories)
                {
                    if (category.RowId < 9)
                        goto console_update;

                    var categoryItems = new List<string[]>();
                    var sortedItems = itemSheets[i].Where(item => item.ItemSearchCategory.Row == category.RowId).ToList();
                    sortedItems.Sort((item1, item2) => (int)(item2.LevelItem.Row - item1.LevelItem.Row));

                    foreach (var item in sortedItems)
                    {
                        var outputItem = new string[6];

                        string classJobAbbr = item.ItemSearchCategory.Value.ClassJob.Value.Abbreviation;
                        if (item.ItemSearchCategory.Value.ClassJob.Value.ClassJobParent.Value.Abbreviation != classJobAbbr)
                            classJobAbbr = item.ItemSearchCategory.Value.ClassJob.Value.ClassJobParent.Value.Abbreviation + " " + classJobAbbr;
                        else if (Resources.ClassJobMap.TryGetValue(classJobAbbr, out var jobAbbr))
                            classJobAbbr += " " + jobAbbr;
                        else if (classJobAbbr == "ADV")
                            classJobAbbr = "";

                        outputItem[0] = item.RowId.ToString();
                        outputItem[1] = item.Name;
                        outputItem[2] = $"/i/{item.Icon}.png";
                        outputItem[3] = item.LevelItem.Row.ToString();
                        outputItem[4] = item.Rarity.ToString();
                        outputItem[5] = classJobAbbr;

                        categoryItems.Add(outputItem);
                    }

                    if (categoryItems.Count == 0)
                        goto console_update;

                    output[category.RowId.ToString()] = JToken.FromObject(categoryItems);

                    console_update:
                    Console.CursorLeft = 0;
                    Console.CursorTop = baseTop + i;
                    Console.Write($"{langs[i]}: [{category.RowId}/{categories.Count() - 1}]");
                    Console.CursorLeft = 10 + category.RowId.ToString("000").Length;
                    Console.Write("                                                                              ");
                }

                File.WriteAllText(Path.Combine(outputPath, $"categories_{langs[i]}.js"), JsonConvert.SerializeObject(output));
            });

            Console.CursorLeft = 0;
            Console.CursorTop = baseTop + 4;
        }

        public static void GenerateChinese(Cyalume lumina, IEnumerable<CsvItem> itemsChs, string outputPath)
        {
            var output = new Dictionary<string, List<string[]>>();

            var counter = 0;
            var baseTop = Console.CursorTop;
            var localItems = lumina.GetExcelSheet<Item>();
            var categories = lumina.GetExcelSheet<ItemSearchCategory>();
            Parallel.ForEach(categories, category =>
            {
                if (category.RowId < 9)
                    goto console_update;

                var categoryItems = new List<string[]>();
                var filteredItems = itemsChs.Where(item => item?.ItemSearchCategory.RowId == category.RowId).ToList();

                Parallel.ForEach(filteredItems, item =>
                {
                    var outputItem = new string[6];

                    var classJobAbbr = item.ItemSearchCategory.ClassJob.Value.Abbreviation ?? "";
                    if (item.ItemSearchCategory.ClassJob.Value.ClassJobParent.Value.Abbreviation != classJobAbbr)
                        classJobAbbr = item.ItemSearchCategory.ClassJob.Value.ClassJobParent.Value.Abbreviation + " " +
                                       classJobAbbr;
                    else if (Resources.ClassJobMap.TryGetValue(classJobAbbr, out var jobAbbr))
                        classJobAbbr += " " + jobAbbr;
                    else if (classJobAbbr == "ADV")
                        classJobAbbr = "";

                    var iconId = (ushort) localItems.First(itm => itm.RowId == item.Key).Icon;
                    var icon = $"/i/{Util.GetIconFolder(iconId)}/{iconId:000000}.png";

                    outputItem[0] = item.Key.ToString();
                    outputItem[1] = item.Name;
                    outputItem[2] = icon;
                    outputItem[3] = item.LevelItem.ToString();
                    outputItem[4] = item.Rarity.ToString();
                    outputItem[5] = classJobAbbr;

                    lock (categoryItems)
                    {
                        categoryItems.Add(outputItem);
                    }
                });

                categoryItems.Sort((item1, item2) => int.Parse(item2[3]) - int.Parse(item1[3]));

                if (categoryItems.Count == 0)
                    goto console_update;

                output[category.RowId.ToString()] = categoryItems;

                console_update:
                Console.CursorLeft = 0;
                Console.CursorTop = baseTop;
                Console.Write($"ch: [{counter}/{categories.Count() - 1}]");
                Console.CursorLeft = 10 + counter.ToString("000").Length;
                Console.Write("                                                                              ");

                counter++;
            });

            File.WriteAllText(Path.Combine(outputPath, "categories_chs.js"), JsonConvert.SerializeObject(output));

            Console.WriteLine();
        }
    }
}
