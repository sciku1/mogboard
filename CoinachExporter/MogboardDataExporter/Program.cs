using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaintCoinach;
using SaintCoinach.Ex;
using SaintCoinach.Xiv;
using Directory = System.IO.Directory;
using Item = SaintCoinach.Xiv.Item;

namespace MogboardDataExporter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var outputPath = Path.Combine("..", "..", "..", "..", "DataExports");
            var categoryJsOutputPath = Path.Combine("..", "..", "..", "..", "public", "data");

            var http = new HttpClient();
            
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Directory.CreateDirectory(outputPath);

            var realm = new ARealmReversed(args[0], Language.English);
            var realmDe = new ARealmReversed(args[0], Language.German);
            var realmFr = new ARealmReversed(args[0], Language.French);
            var realmJp = new ARealmReversed(args[0], Language.Japanese);

            var items = realm.GameData.GetSheet<Item>();
            var itemsDe = realmDe.GameData.GetSheet<Item>();
            var itemsFr = realmFr.GameData.GetSheet<Item>();
            var itemsJp = realmJp.GameData.GetSheet<Item>();
            var itemsChs = GetChineseItems(http).GetAwaiter().GetResult();

            Console.WriteLine("Starting game data export...");
            
            #region Category JS Export
            categoryjs:
            CategoryJs.Generate(realm, realmDe, realmFr, realmJp, categoryJsOutputPath);
            CategoryJs.GenerateChinese(itemsChs, categoryJsOutputPath, http);
            #endregion
            goto end;
            #region Item Export
            items:
            foreach (var category in realm.GameData.GetSheet<ItemSearchCategory>())
            {
                // We don't need those, not for sale
                if (category.Key == 0)
                    continue;

                var output = new List<JObject>();

                foreach (var item in items.Where(item => item.ItemSearchCategory.Key == category.Key))
                {
                    dynamic outputItem = new JObject();

                    outputItem.ID = item.Key;

                    var iconId = (ushort) item.GetRaw("Icon");
                    outputItem.Icon = $"/i/{GetIconFolder(iconId)}/{iconId}.png";

                    outputItem.Name_en = item.Name.ToString();
                    outputItem.Name_de = itemsDe.First(localItem => localItem.Key == item.Key).Name.ToString();
                    outputItem.Name_fr = itemsFr.First(localItem => localItem.Key == item.Key).Name.ToString();
                    outputItem.Name_jp = itemsJp.First(localItem => localItem.Key == item.Key).Name.ToString();
                    
                    var nameChs = itemsChs.FirstOrDefault(localItem => localItem.ID == item.Key)?.Name.ToString();
                    outputItem.Name_chs = string.IsNullOrEmpty(nameChs) ? item.Name.ToString() : nameChs;

                    outputItem.LevelItem = item.ItemLevel.Key;
                    outputItem.Rarity = item.Rarity;

                    output.Add(outputItem);
                }

                if (output.Count == 0)
                    continue;

                Console.WriteLine($"Cat {category.Key}: {output.Count}");

                System.IO.File.WriteAllText(Path.Combine(outputPath, $"ItemSearchCategory_{category.Key}.json"), JsonConvert.SerializeObject(output));
            }
            #endregion

            #region Marketable Item JSON Export
            marketableitems:
            dynamic itemJSONOutput = new JObject();
            var itemID = new List<int>();
            foreach (var category in realm.GameData.GetSheet<ItemSearchCategory>())
            {
                if (category.Key < 9)
                    continue;
                var itemSet = items
                    .Where(item => item.ItemSearchCategory.Key == category.Key)
                    .Select(item => item.Key);
                if (!itemSet.Any())
                    continue;
                itemID = itemID.Concat(itemSet).ToList();

                Console.WriteLine($"Cat {category.Key}: {itemSet.Count()}");
            }
            itemID.Sort();
            itemJSONOutput.itemID = JToken.FromObject(itemID);
            File.WriteAllText(Path.Combine(outputPath, "item.json"), JsonConvert.SerializeObject(itemJSONOutput));
            #endregion
            
            #region ItemSearchCategory Export
            File.WriteAllText(Path.Combine(outputPath, "ItemSearchCategory_Keys.json"), JsonConvert.SerializeObject(realm.GameData.GetSheet("ItemSearchCategory").Keys.ToList()));
            #endregion
            
            #region Town Export
            town_export:
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
                outputTown.Icon = (int) iconObj != 0 ? $"/i/{GetIconFolder((int) iconObj)}/{(int) iconObj}.png" : $"/i/{GetIconFolder(060880)}/060880.png";

                outputTown.Name_en = town.AsString("Name").ToString();
                outputTown.Name_de = townsDe.First(localItem => localItem.Key == town.Key).AsString("Name").ToString();
                outputTown.Name_fr = townsFr.First(localItem => localItem.Key == town.Key).AsString("Name").ToString();
                outputTown.Name_jp = townsJp.First(localItem => localItem.Key == town.Key).AsString("Name").ToString();
                outputTown.Name_chs = townsChs.First(localItem => localItem.ID == town.Key).Name;

                outputTowns.Add(outputTown);
            }

            System.IO.File.WriteAllText(Path.Combine(outputPath, "Town.json"), JsonConvert.SerializeObject(outputTowns));
            #endregion

            #region World Export
            world_export:
            var worlds = realm.GameData.GetSheet("World");

            var outputWorlds = new List<JObject>();

            foreach (var world in worlds)
            {
                dynamic outputWorld = new JObject();

                outputWorld.ID = world.Key;

                outputWorld.Name = world.AsString("Name").ToString();
                outputWorld.DataCenter = (byte) world.GetRaw("DataCenter");
                outputWorld.IsPublic = world.AsBoolean("IsPublic");

                outputWorlds.Add(outputWorld);
            }

            System.IO.File.WriteAllText(Path.Combine(outputPath, "World.json"), JsonConvert.SerializeObject(outputWorlds));
            #endregion

            end:
            Console.WriteLine("Done!");
            Console.ReadKey();
        }

        private static async Task<IEnumerable<XIVAPIItem>> GetChineseItems(HttpClient http)
        {
            var items = new List<XIVAPIItem>();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var counter = 1;
            var pageTotal = JObject.Parse(
                await http.GetStringAsync(new Uri($"https://cafemaker.wakingsands.com/Item")))["Pagination"]["PageTotal"].ToObject<int>();
            Console.Write($"Downloading Chinese game data from FFCAFE (1/{pageTotal})...");
            Parallel.For(1, pageTotal, i =>
            {
                var res = JObject.Parse(http.GetStringAsync(new Uri($"https://cafemaker.wakingsands.com/Item?Columns=ID,Icon,Name,LevelItem,Rarity,ItemSearchCategory.ID,ItemSearchCategory.ClassJob.Abbreviation&Page={i}")).GetAwaiter().GetResult());
                Console.CursorLeft = "Downloading Chinese game data from FFCAFE ".Length;
                Console.Write($"({++counter}/{pageTotal})...");
                items.AddRange(res["Results"].Children().Select(item => new XIVAPIItem
                {
                    ID = item["ID"].ToObject<int>(),
                    Icon = item["Icon"].ToObject<string>(),
                    ItemSearchCategory = new XIVAPIMicroItemSearchCategory
                    {
                        Category = item["ItemSearchCategory"]["ID"].ToObject<int?>() ?? 0,
                        ClassJob = new XIVAPIMicroClassJob
                        {
                            Abbreviation = item["ItemSearchCategory"]["ClassJob"]["Abbreviation"].ToObject<string>()
                        },
                    },
                    LevelItem = item["LevelItem"].ToObject<int>(),
                    Name = item["Name"].ToObject<string>(),
                    Rarity = item["Rarity"].ToObject<int>(),
                }));
            });

            stopwatch.Stop();
            Console.WriteLine($" Done ({stopwatch.ElapsedMilliseconds / 1000.0f}s)!");

            return items;
        }

        private static string GetIconFolder(int iconId) => (Math.Floor(iconId / 1000d) * 1000).ToString("000000");

        private class XIVAPITown
        {
            public int ID { get; set; }
            public string Icon { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
        }
    }
}
