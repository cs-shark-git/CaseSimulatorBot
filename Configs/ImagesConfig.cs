using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Configs
{
    internal class ImagesConfig : IConfig
    {

        [JsonProperty("profile_template")]
        public string? ProfileTemplate { get; set; }

        [JsonProperty("inventory_template")]
        public string? InventoryTemplate { get; set; }

        [JsonProperty("case_common_template")]
        public string? CaseCommonTemplate { get; set; }

        [JsonProperty("case_uncommon_template")]
        public string? CaseUncommonTemplate { get; set; }

        [JsonProperty("case_rare_template")]
        public string? CaseRareTemplate { get; set; }

        [JsonProperty("case_legendary_template")]
        public string? CaseLegendaryTemplate { get; set; }

        [JsonProperty("case_mythical_template")]
        public string? CaseMythicalTemplate { get; set; }

        [JsonProperty("not_opened_case")]
        public string? NotOpenedCase { get; set; }

        [JsonProperty("cases_slots_template")]
        public string? CasesSlotsTemplate { get; set; }

        [JsonProperty("many_cases_template")]
        public string? ManyCasesTemplate { get; set; }

        [JsonIgnore]
        public string FileName => "images.json";

        [JsonIgnore]
        public string Template => "{\r\n\t\"profile_template\": \"rsc/images/profile_template.png\",\r\n\t\"inventory_template\": \"rsc/images/inventory_template.png\",\r\n\t\"case_common_template\": \"rsc/images/case_templates/case_common_template.png\",\r\n\t\"case_uncommon_template\": \"rsc/images/case_templates/case_uncommon_template.png\",\r\n\t\"case_rare_template\": \"rsc/images/case_templates/case_rare_template.png\",\r\n\t\"case_legendary_template\": \"rsc/images/case_templates/case_legendary_template.png\",\r\n\t\"case_mythical_template\": \"rsc/images/case_templates/case_mythical_template.png\",\r\n\t\"not_opened_case\": \"rsc/images/not_opened_case.png\",\r\n\t\"cases_slots_template\": \"rsc/images/case_templates/cases_slots_template.png\",\r\n\t\"many_cases_template\": \"rsc/images/case_templates/many_cases_template.png\"\r\n}";

        [JsonIgnore]
        public bool IsFillRequired => false;
    }
}
