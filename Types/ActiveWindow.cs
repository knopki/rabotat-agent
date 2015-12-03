using Newtonsoft.Json;
using System;

namespace RabotatAgent.Types
{
    [JsonObject]
    class ActiveWindow
    {
        [JsonProperty(PropertyName = "from", Required = Required.Always)]
        public DateTime From;

        [JsonProperty(PropertyName = "to", Required = Required.Always)]
        public DateTime To;

        [JsonProperty(PropertyName = "processName", Required = Required.Default)]
        public string ProcessName;

        [JsonProperty(PropertyName = "moduleName", Required = Required.Default)]
        public string ModuleName;

        [JsonProperty(PropertyName = "windowTitle", Required = Required.Always)]
        public string WindowTitle;

        [JsonProperty(PropertyName = "companyName", Required = Required.Default)]
        public string CompanyName;

        [JsonProperty(PropertyName = "description", Required = Required.Default)]
        public string Description;

        [JsonProperty(PropertyName = "fileName", Required = Required.Default)]
        public string FileName;

        [JsonProperty(PropertyName = "productName", Required = Required.Default)]
        public string ProductName;

        [JsonProperty(PropertyName = "url", Required = Required.Default)]
        public string Url;
    }
}
