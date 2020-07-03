using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaintCoinach;

namespace MogboardDataExporter.Exporters
{
    public static class TownExports
    {
        public static void GenerateJSON(ARealmReversed realm, ARealmReversed realmDe, ARealmReversed realmFr, ARealmReversed realmJp, HttpClient http, string outputPath)
        {
            var towns = realm.GameData.GetSheet("Town");
            var townsDe = realmDe.GameData.GetSheet("Town");
            var townsFr = realmFr.GameData.GetSheet("Town");
            var townsJp = realmJp.GameData.GetSheet("Town");

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

                outputTown.ID = town.Key;

                var iconObj = town.GetRaw("Icon");
                outputTown.Icon = (int) iconObj != 0 ? $"/i/{Util.GetIconFolder((int) iconObj)}/{(int) iconObj}.png" : $"/i/{Util.GetIconFolder(060880)}/060880.png";

                outputTown.Name_en = town.AsString("Name").ToString();
                outputTown.Name_de = townsDe.First(localItem => localItem.Key == town.Key).AsString("Name").ToString();
                outputTown.Name_fr = townsFr.First(localItem => localItem.Key == town.Key).AsString("Name").ToString();
                outputTown.Name_jp = townsJp.First(localItem => localItem.Key == town.Key).AsString("Name").ToString();
                outputTown.Name_chs = townsChs.First(localItem => localItem.ID == town.Key).Name;

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
