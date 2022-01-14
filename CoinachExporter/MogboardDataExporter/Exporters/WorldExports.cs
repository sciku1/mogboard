using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cyalume = Lumina.GameData;

namespace MogboardDataExporter.Exporters
{
    public static class WorldExports
    {
        public static void GenerateJSON(Cyalume lumina, string outputPath)
        {
            var worlds = lumina.GetExcelSheet<World>()!;

            var outputWorlds = new List<JObject>();
            
            var douDouChai = new[] { "ShuiJingTa", "YinLeiHu", "TaiYangHaiAn", "YiXiuJiaDe", "HongChaChuan", "XueSongYuan" };
            foreach (var world in worlds)
            {
                if (douDouChai.Contains(world.Name)) continue;

                dynamic outputWorld = new JObject();
                outputWorld.ID = world.RowId;
                
                outputWorld.Name = (string)world.Name;
                if (((string)world.Name).EndsWith("2"))
                {
                    outputWorld.Name = ((string)world.Name)[..^1];
                }

                outputWorld.DataCenter = (byte)world.DataCenter.Row;
                outputWorld.IsPublic = world.IsPublic;
                outputWorlds.Add(outputWorld);
            }

            File.WriteAllText(Path.Combine(outputPath, "World.json"), JsonConvert.SerializeObject(outputWorlds));
        }

        private class CustomWorld
        {
            public uint Id { get; set; }

            public string Name { get; set; }

            public byte DataCenter { get; set; }

            public bool IsPublic { get; set; }
        }
    }
}