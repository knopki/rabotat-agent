using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RabotatAgent.Types
{
    [JsonObject]
    class DataPacket
    {
        [JsonProperty(PropertyName = "userName", Required = Required.Always)]
        public string UserName;

        [JsonProperty(PropertyName = "domainName", Required = Required.Always)]
        public string DomainName;

        [JsonProperty(PropertyName = "hostName", Required = Required.Always)]
        public string HostName;

        [JsonProperty(PropertyName = "timestamp", Required = Required.Always)]
        public DateTime Timestamp;

        [JsonProperty(PropertyName = "hash", Required = Required.Always)]
        public string Hash;

        [JsonProperty(PropertyName = "windows", Required = Required.Always)]
        public List<ActiveWindow> Windows;
    }
}
