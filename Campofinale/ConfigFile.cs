namespace Campofinale
{
    public class ConfigFile
    {
        public MongoDatabaseSettings mongoDatabase = new();
        public DispatchServerSettings dispatchServer = new();
        public GameserverSettings gameServer = new();
        public ServerOptions serverOptions = new();
        public LogSettings logOptions = new();
    }
    public class ServerOptions
    {
        public int defaultSceneNumId = 87;
        public int maxPlayers = 20;
        public CharactersOptions defaultCharacters = new();
        public ServerOptions()
        {
        }

        public class CharactersOptions
        {
            public int defaultLevel = 1;
            public bool giveAllCharacters = true;
            public List<string> characters = new List<string>(); //used if giveAllCharacters is false

            public CharactersOptions() { }
        }
       /* public struct WelcomeMail
        {
        }*/
    }
    public class LogSettings
    {
        public bool packets = true;
        public bool packetWarnings = true;
        public bool packetBodies = false;
        public bool debugPrint = false;

        public LogSettings()
        {
        }
    }
    public class GameserverSettings
    {
        public string bindAddress = "127.0.0.1";
        public int bindPort = 30000;
        public string accessAddress = "127.0.0.1";
        public int accessPort = 30000;
        public GameserverSettings()
        {
        }
    }
    public class DispatchServerSettings
    {
        public string bindAddress = "127.0.0.1";
        public int bindPort = 5000;
        public string accessAddress = "127.0.0.1";
        public int accessPort = 5000;
        public string emailFormat = "@campofinale.ps";
        public DispatchServerSettings()
        {

        }
    }
    public class MongoDatabaseSettings
    {
        public string uri = "mongodb://localhost:27017";
        public string collection = "Campofinale";
        public MongoDatabaseSettings()
        {
        }
    }
}
