using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Aspid
{
    public class CommandTools : ModuleBase<SocketCommandContext>
    {
        [Command("reset")]
        public async Task ResetLanguage()
        {
            for (int i = 0; i < Config.Countries.Length; i++)
            {
                using var reader = new System.IO.StreamReader(Config.configPath + $"/localization/{Config.Countries[i]}.yml");
                var yaml = new YamlStream();
                yaml.Load(reader);
                Program.mapping.Add(Config.Countries[i], (YamlMappingNode)yaml.Documents[0].RootNode);
            }
            await Context.Channel.SendMessageAsync("Language packages had been reset");
        }
        
        //Add emotion to context message
        public async Task Emotion(string emotion)
        {
            await Context.Message.AddReactionAsync(Emote.Parse(emotion));
        }

        //Adapt input text for HTML use
        public static string EnterRepacer(string line)
        {
            string a = line.Replace((char)10, '|');
            a = a.Replace("|", "<br>");
            return a;
        }

        //russian-english transliteration for RP-twitter
        public static string Translit(string str)
        {
            string[] lat_up = { "A", "B", "V", "G", "D", "E", "Yo", "Zh", "Z", "I", "Y", "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "F", "Kh", "Ts", "Ch", "Sh", "Shch", "\"", "Y", "'", "E", "Yu", "Ya" };
            string[] lat_low = { "a", "b", "v", "g", "d", "e", "yo", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "kh", "ts", "ch", "sh", "shch", "\"", "y", "'", "e", "yu", "ya" };
            string[] rus_up = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я" };
            string[] rus_low = { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я" };
            for (int i = 0; i <= 32; i++)
            {
                str = str.Replace(rus_up[i], lat_up[i]);
                str = str.Replace(rus_low[i], lat_low[i]);
            }
            return str;
        }
    }
}
