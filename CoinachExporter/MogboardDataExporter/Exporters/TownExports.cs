using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cyalume = Lumina.GameData;

namespace MogboardDataExporter.Exporters
{
    public static class TownExports
    {
        public static void GenerateJSON(Cyalume luminaEn, Cyalume luminaDe, Cyalume luminaFr, Cyalume luminaJp, HttpClient http, string outputPath)
        {
            var towns = luminaEn.GetExcelSheet<Town>();
            var townsDe = luminaDe.GetExcelSheet<Town>();
            var townsFr = luminaFr.GetExcelSheet<Town>();
            var townsJp = luminaJp.GetExcelSheet<Town>();

            var townsChs = JObject.Parse(http.GetStringAsync(new Uri("https://cafemaker.wakingsands.com/Town"))
                    .GetAwaiter().GetResult())["Results"]
                .Children()
                .Select(town => town.ToObject<XIVAPITown>())
                .ToList();
            townsChs.Add(new XIVAPITown
            {
                ID = 0,
                Name = "不知何处",
            });

            var outputTowns = new List<JObject>();

            foreach (var town in towns)
            {
                dynamic outputTown = new JObject();

                outputTown.ID = town.RowId;

                var iconObj = town.Icon;
                outputTown.Icon = iconObj != 0 ? $"/i/{Util.GetIconFolder(iconObj)}/{iconObj:000000}.png" : $"/i/{Util.GetIconFolder(060880)}/060880.png";

                outputTown.Name_en = (string)town.Name;
                outputTown.Name_de = (string)townsDe.First(localItem => localItem.RowId == town.RowId).Name;
                outputTown.Name_fr = (string)townsFr.First(localItem => localItem.RowId == town.RowId).Name;
                outputTown.Name_jp = (string)townsJp.First(localItem => localItem.RowId == town.RowId).Name;

                // This may be null when Global has a new expansion that CN does not yet have
                outputTown.Name_chs = townsChs.FirstOrDefault(localItem => localItem.ID == town.RowId)?.Name;

                outputTowns.Add(outputTown);
            }

            File.WriteAllText(Path.Combine(outputPath, "Town.json"), JsonConvert.SerializeObject(outputTowns));
        }

        private class XIVAPITown
        {
            public int ID { get; set; }
            public string Icon { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
        }
    }
}
