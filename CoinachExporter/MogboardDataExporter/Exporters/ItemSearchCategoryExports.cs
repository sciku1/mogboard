using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaintCoinach;

namespace MogboardDataExporter.Exporters
{
    public static class ItemSearchCategoryExports
    {
        public static void GenerateJSON(ARealmReversed realm, string outputPath)
        {
            File.WriteAllText(Path.Combine(outputPath, "ItemSearchCategory_Keys.json"), JsonConvert.SerializeObject(realm.GameData.GetSheet("ItemSearchCategory").Keys.ToList()));
        }

        public static void GenerateChineseMappingsJSON(HttpClient http, string outputPath)
        {
            var chsCategories = JObject.Parse(http.GetStringAsync(new Uri($"https://cafemaker.wakingsands.com/Item")).GetAwaiter()
                .GetResult())["Results"];
            var chsIscOutput = chsCategories.ToDictionary(category => category["ID"].ToObject<int>(), category => category["Name"].ToObject<string>());
            File.WriteAllText(Path.Combine(outputPath, "ItemSearchCategory_Mappings_Chs.json"), JsonConvert.SerializeObject(chsIscOutput));
        }
    }
}