using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Configs
{
    internal class FontsConfig : IConfig
    {
        [JsonProperty("roboto")]
        public string? RobotoPath { get; set; }

        [JsonIgnore]
        public string FileName => "fonts.json";

        [JsonIgnore]
        public string Template => "{\r\n\t\"roboto\": \"rsc/fonts/roboto.ttf\"\r\n}";

        [JsonIgnore]
        public bool IsFillRequired => false;
    }
}
