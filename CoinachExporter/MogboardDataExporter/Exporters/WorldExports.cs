using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaintCoinach;

namespace MogboardDataExporter.Exporters
{
    public static class WorldExports
    {
        public static void GenerateJSON(ARealmReversed realm, string outputPath)
        {
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

            File.WriteAllText(Path.Combine(outputPath, "World.json"), JsonConvert.SerializeObject(outputWorlds));
        }
    }
}