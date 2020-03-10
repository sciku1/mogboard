using MogboardDataExporter.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaintCoinach;
using SaintCoinach.Xiv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MogboardDataExporter
{
    public static class CategoryJs
    {
        public static void Generate(ARealmReversed realmEn, ARealmReversed realmDe, ARealmReversed realmFr, ARealmReversed realmJp, string outputPath)
        {
            string[] langs = { "en", "de", "fr", "ja" };
            ARealmReversed[] realms = { realmEn, realmDe, realmFr, realmJp };
            IXivSheet<Item>[] itemSheets = { realmEn.GameData.GetSheet<Item>(), realmDe.GameData.GetSheet<Item>(), realmFr.GameData.GetSheet<Item>(), realmJp.GameData.GetSheet<Item>() };

            dynamic output = new JObject();

            for (int i = 0; i < 4; i++)
            {
                foreach (var category in realms[i].GameData.GetSheet<ItemSearchCategory>())
                {
                    if (category.Key < 9)
                        continue;

                    var categoryItems = new List<string[]>();
                    List<Item> sortedItems = itemSheets[i].Where(item => item.ItemSearchCategory.Key == category.Key).ToList();
                    sortedItems.Sort((item1, item2) => item2.ItemLevel.Key - item1.ItemLevel.Key);

                    foreach (var item in sortedItems)
                    {
                        var outputItem = new string[6];

                        string classJobAbbr = item.ItemSearchCategory.ClassJob.Abbreviation;
                        if (item.ItemSearchCategory.ClassJob.ParentClassJob.Abbreviation != classJobAbbr)
                            classJobAbbr = item.ItemSearchCategory.ClassJob.ParentClassJob.Abbreviation + " " + classJobAbbr;
                        else if (Resources.ClassJobMap.TryGetValue(classJobAbbr, out string jobAbbr))
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
                        continue;

                    output[category.Key.ToString()] = JToken.FromObject(categoryItems);

                    Console.WriteLine($"Cat {category.Key}: {categoryItems.Count}");
                }

                File.WriteAllText(Path.Combine(outputPath, $"categories_50_{langs[i]}.js"), JsonConvert.SerializeObject(output));
            }
        }
    }
}
