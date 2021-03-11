using Newtonsoft.Json;

namespace Discord
{
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }
        [JsonProperty("prefix")]
        public static string[] Prefix { get; private set; }
        [JsonProperty("blacklist")]
        public static ulong[] Blacklist { get; private set; }
    }
}