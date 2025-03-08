using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campofinale
{
    public class ConfigFile
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public MongoDatabaseSettings mongoDatabase = new();
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public DispatchServerSettings dispatchServer = new();
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public GameserverSettings gameServer = new();
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public ServerOptions serverOptions = new();
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public LogSettings logOptions = new();
    }
    public class ServerOptions
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int defaultSceneNumId = 98;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int maxPlayers = 20;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public CharactersOptions defaultCharacters = new();
        public ServerOptions()
        {
        }

        public class CharactersOptions
        {
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
            public int defaultLevel = 1;
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
            public bool giveAllCharacters = true;
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
            public List<string> characters = new List<string>()
            {
                "chr_0002_endminm",
                "chr_0003_endminf",
                "chr_0015_lifeng"
            }; //used if giveAllCharacters is false

            public CharactersOptions() { }
        }
       /* public struct WelcomeMail
        {
        }*/
    }
    public class LogSettings
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool packets;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool debugPrint=false;

        public LogSettings()
        {
        }
    }
    public class GameserverSettings
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string bindAddress = "127.0.0.1";
        public int bindPort = 30000;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string accessAddress = "127.0.0.1";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int accessPort = 30000;
        public GameserverSettings()
        {
        }
    }
    public class DispatchServerSettings
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string bindAddress = "127.0.0.1";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int bindPort = 5000;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string accessAddress = "127.0.0.1";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int accessPort = 5000;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string emailFormat = "@campofinale.ps";
        public DispatchServerSettings()
        {

        }
    }
    public class MongoDatabaseSettings
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string uri = "mongodb://localhost:27017";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string collection = "Campofinale";
        public MongoDatabaseSettings()
        {
        }
    }
}
