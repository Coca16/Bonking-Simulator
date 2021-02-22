using Newtonsoft.Json;

namespace Bonking
{
    public struct SecretJson
    {
        [JsonProperty("server")]
        public string Server { get; private set; }
        [JsonProperty("userid")]
        public string UserID { get; private set; }
        [JsonProperty("password")]
        public string Password { get; private set; }
        [JsonProperty("database")]
        public string Database { get; private set; }

        public SecretJson(string server, string userid, string password, string database)
        {
            Database = database;
            Server = server;
            UserID = userid;
            Password = password;
        }
    }
}