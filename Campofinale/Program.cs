using Campofinale;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        StartServer(args);
    }

    private static void StartServer(string[] args)
    {
        Console.Title = "Initializing...";
        ConfigFile config = new ConfigFile();
        if (File.Exists("server_config.json"))
        {
            config = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText("server_config.json"))!;
        }
        File.WriteAllText("server_config.json", JsonConvert.SerializeObject(config, Formatting.Indented));

        new Thread(() =>
        {
            new Server().Start(config);
        }).Start();
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            Logger.Print("Shutting down...");
            
            Server.Shutdown();
        };

        while (Server.Initialized == false)
        {

        }
        Console.Title = $"Campofinale Server v{Server.ServerVersion}";
    }
}