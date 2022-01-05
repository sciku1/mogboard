using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Lumina.Data;
using Lumina.Excel.GeneratedSheets;
using MogboardDataExporter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MogboardDataExporter.Exporters
{
    public static class ItemSearchCategoryExports
    {
        public static void GenerateJSON(
            IEnumerable<ItemSearchCategory> itemSearchCategories,
            IEnumerable<ItemSearchCategory> itemSearchCategoriesDe,
            IEnumerable<ItemSearchCategory> itemSearchCategoriesFr,
            IEnumerable<ItemSearchCategory> itemSearchCategoriesJa,
            string outputPath)
        {
            var mappings = new Dictionary<uint, ItemSearchCategoryExport>();
            var validIds = new List<uint>();

            var langIsc = new[] { itemSearchCategories, itemSearchCategoriesDe, itemSearchCategoriesFr, itemSearchCategoriesJa };
            foreach (var lang in langIsc)
            {
                foreach (var cat in lang)
                {
                    if (cat.Name == "") continue;

                    if (!mappings.TryGetValue(cat.RowId, out _))
                    {
                        mappings[cat.RowId] = new ItemSearchCategoryExport
                        {
                            Id = cat.RowId,
                            Category = cat.Category,
                            Order = cat.Order,
                            Icon = $"/i/{Util.GetIconFolder(cat.Icon)}/{cat.Icon:000000}.png",
                        };

                        validIds.Add(cat.RowId);
                    }

                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                    switch (cat.SheetLanguage)
                    {
                        case Language.Japanese:
                            mappings[cat.RowId].NameJa = cat.Name;
                            break;
                        case Language.English:
                            mappings[cat.RowId].NameEn = cat.Name;
                            break;
                        case Language.German:
                            mappings[cat.RowId].NameDe = cat.Name;
                            break;
                        case Language.French:
                            mappings[cat.RowId].NameFr = cat.Name;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            File.WriteAllText(Path.Combine(outputPath, "ItemSearchCategory_Mappings_Global.json"), JsonConvert.SerializeObject(mappings));
            File.WriteAllText(Path.Combine(outputPath, "ItemSearchCategory_Keys.json"), JsonConvert.SerializeObject(validIds));
        }

        public static void GenerateChineseMappingsJSON(HttpClient http, string outputPath)
        {
            var chsCategories = JObject.Parse(http.GetStringAsync(new Uri("https://cafemaker.wakingsands.com/ItemSearchCategory")).GetAwaiter()
                .GetResult())["Results"];
            var chsIscOutput = chsCategories.ToDictionary(category => category["ID"].ToObject<int>(), category => category["Name"].ToObject<string>());
            File.WriteAllText(Path.Combine(outputPath, "ItemSearchCategory_Mappings_Chs.json"), JsonConvert.SerializeObject(chsIscOutput));
        }
    }
}