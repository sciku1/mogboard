namespace MogboardDataExporter
{
    public class XIVAPIItem
    {
        public int ID { get; set; }
        public string Icon { get; set; }
        public XIVAPIMicroItemSearchCategory ItemSearchCategory { get; set; }
        public int LevelItem { get; set; }
        public string Name { get; set; }
        public int Rarity { get; set; }
    }

    public class XIVAPIMicroItemSearchCategory
    {
        public int Category { get; set; }
        public XIVAPIMicroClassJob ClassJob { get; set; }
    }

    public class XIVAPIMicroClassJob
    {
        public string Abbreviation { get; set; }
    }
}
