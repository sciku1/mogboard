using Newtonsoft.Json;

namespace MogboardDataExporter.Models
{
    public class ItemSearchCategoryExport
    {
        [JsonProperty("ID")]
        public uint Id { get; set; }
        
        public byte Category { get; set; }
        
        public byte Order { get; set; }
        
        public string Icon { get; set; }

        [JsonProperty("Name_en")]
        public string NameEn { get; set; }

        [JsonProperty("Name_de")]
        public string NameDe { get; set; }

        [JsonProperty("Name_fr")]
        public string NameFr { get; set; }

        [JsonProperty("Name_ja")]
        public string NameJa { get; set; }
    }
}