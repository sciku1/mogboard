using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cyalume = Lumina.GameData;

namespace MogboardDataExporter.Exporters
{
    public static class ItemSearchCategoryExports
    {
        public static void GenerateJSON(Cyalume lumina, string outputPath)
        {
            File.WriteAllText(Path.Combine(outputPath, "ItemSearchCategory_Keys.json"), JsonConvert.SerializeObject(lumina.GetExcelSheet<ItemSearchCategory>().Select(isc => isc.RowId)));
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