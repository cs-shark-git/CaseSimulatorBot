using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Configs
{
    internal class SettingsConfig : IConfig
    {
        [JsonProperty("fruits_directory")]
        public string? FruitsDirectory { get; set; }

        [JsonIgnore]
        public string FileName => "settings.json";

        [JsonIgnore]
        public string Template => "{\r\n\t\"fruits_directory\": \"rsc/images/fruits\"\r\n}";

        [JsonIgnore]
        public bool IsFillRequired => false;
    }
}
