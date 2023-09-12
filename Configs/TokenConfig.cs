using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Configs
{
    internal class TokenConfig : IConfig
    {
        [JsonProperty("token")]
        public string? Token { get; set; }

        [JsonIgnore]
        public string FileName => "token.json";

        [JsonIgnore]
        public string Template => "{\r\n\t\"token\": \"\"\r\n}";

        [JsonIgnore]
        public bool IsFillRequired => true;

    }
}
