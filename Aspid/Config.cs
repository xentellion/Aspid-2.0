using System;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using YamlDotNet.RepresentationModel;

namespace Aspid
{
    class Config
    {
        //Yep. This is where i keep files
        static string LinuxFolder { get; } = "/home/xentellion/Aspid/Data";
        static string WindowsFolder { get; } = $"{System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)}/Xentellion/Aspid";

        internal static string configPath;

        const string configFile = "Load.json";
        internal const string mydb = "AspidDataBase.db";

        static internal string connectionString;

        internal static BotConfig bot;

        internal static string[] Countries = { "english", "russia" };

        static Config()
        {
            //No MAC OS, sorry not sorry
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                configPath = LinuxFolder;
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                configPath = WindowsFolder;
            else
            {
                Console.WriteLine("This platform is not suppprted");
                return;
            }

            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);

            configPath += "/";
            bot = new BotConfig();

            //json with data
            if (!File.Exists(configPath + configFile))
            {
                using FileStream fs = File.Create(configPath + configFile);
                byte[] info = new System.Text.UTF8Encoding(true).GetBytes(JsonConvert.SerializeObject(bot, Formatting.Indented));
                fs.Write(info, 0, info.Length);
            }
            else
                bot = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText(configPath + configFile));

            //main database
            if (!File.Exists(configPath + mydb))
            {
                using FileStream fs = File.Create(configPath + mydb);
                Console.WriteLine("New database had been created");
            }

            connectionString = "Filename = " + configPath + mydb;

            for (int i = 0; i < Countries.Length; i++)
            {
                using var reader = new System.IO.StreamReader($"C:\\Users\\Xentellion\\Documents\\Xentellion\\Aspid\\localization\\{Countries[i]}.yml");
                var yaml = new YamlStream();
                yaml.Load(reader);
                Program.mapping.Add(Countries[i], (YamlMappingNode)yaml.Documents[0].RootNode);
            }
        }

        internal static void SaveDead()
        {
            File.WriteAllText(configPath + configFile, JsonConvert.SerializeObject(bot, Formatting.Indented));
        }
    }

    internal class BotConfig
    {
        public string Token { get; set; } = "";
        public string Prefix { get; set; } = "";
        public int DeadPeople { get; set; } = 0;
    }

    public class Hero
    {
        public string Name { get; set; }
        public ulong Owner { get; set; }
        public string Image { get; set; }
        public string Level { get; set; }
        public string Bio { get; set; }
        public string Inv { get; set; }
        public string Intellect { get; set; }
        public string Magic { get; set; }
        public string Nature { get; set; }
    }
}
