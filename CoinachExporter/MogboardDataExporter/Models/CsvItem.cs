using SaintCoinach.Xiv;

namespace MogboardDataExporter.Models
{
    public class CsvItem
    {
        public int Key { get; set; }

        public string Name { get; set; }

        public int LevelItem { get; set; }

        public int Rarity { get; set; }

        public ItemSearchCategory ItemSearchCategory { get; set; }
    }
}