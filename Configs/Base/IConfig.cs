using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Configs
{
    internal interface IConfig
    {
        [JsonIgnore]
        public string FileName { get; }

        [JsonIgnore]
        public string Template { get; }

        [JsonIgnore]
        public bool IsFillRequired { get; }
    }
}
