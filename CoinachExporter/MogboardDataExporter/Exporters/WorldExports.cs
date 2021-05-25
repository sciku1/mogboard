using System.Collections.Generic;
using System.IO;
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
            var worlds = lumina.GetExcelSheet<World>();

            var outputWorlds = new List<JObject>();

            foreach (var world in worlds)
            {
                dynamic outputWorld = new JObject();

                outputWorld.ID = world.RowId;

                outputWorld.Name = (string)world.Name;
                outputWorld.DataCenter = (byte) world.DataCenter.Row;
                outputWorld.IsPublic = world.IsPublic;

                outputWorlds.Add(outputWorld);
            }

            File.WriteAllText(Path.Combine(outputPath, "World.json"), JsonConvert.SerializeObject(outputWorlds));
        }
    }
}