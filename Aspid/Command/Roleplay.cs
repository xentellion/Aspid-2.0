using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspid.Command
{
    public class Roleplay : ModuleBase<SocketCommandContext>
    {
        [Command("hero")]
        public async Task GetCharacter(string name)
        { 
            if (!System.IO.Directory.Exists(Config.configPath + Context.Guild.Id.ToString()))
                System.IO.Directory.CreateDirectory(Config.configPath + Context.Guild.Id.ToString());

            string path = Config.configPath + Context.Guild.Id.ToString() + "/" + CommandTools.Translit(name);

            if (!System.IO.File.Exists($"{path}.png"))
            {
                Console.WriteLine($"{path}.png");
                await Update(name, Context.Guild.Id);
            }
            await Context.Channel.SendFileAsync($"{path}.png");
        }

        [Command("skills")]
        public async Task GetStats(string name)
        {
            if (!System.IO.Directory.Exists(Config.configPath + Context.Guild.Id.ToString()))
                System.IO.Directory.CreateDirectory(Config.configPath + Context.Guild.Id.ToString());

            string path = Config.configPath + Context.Guild.Id.ToString() + "/" + CommandTools.Translit(name) + "_skills";

            if (!System.IO.File.Exists($"{path}.png"))
            {
                Console.WriteLine($"{path}.png");
                await Update(name, Context.Guild.Id);
            }
            await Context.Channel.SendFileAsync($"{path}.png");
        }

        [Command("hero")]
        public async Task GetCharacter(SocketGuildUser user)
        {
            List<Hero> characters = new List<Hero>();

            using SqliteCommand getHero = new SqliteCommand(Queries.GetCharacter(Context.Guild.Id, user.Id), Program.sqliteConnection);
            using SqliteDataReader reader = await getHero.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                while(await reader.ReadAsync())
                {
                    characters.Add(new Hero
                    {
                        Name = reader.GetString(0)
                    });
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(Text("no_user_hero"));
                return;
            }
            await reader.CloseAsync();

            if (characters.Count == 1)
            {
                await Context.Channel.SendFileAsync($"{Config.configPath + Context.Guild.Id.ToString() + "/" + CommandTools.Translit(characters[0].Name)}.png");
            }
            else
            {
                string list = "";
                characters.Sort((x, y) => string.Compare(x.Name, y.Name));
                foreach (Hero hero in characters)
                {
                    list += hero.Name;
                    list += "\n";
                }
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    .WithDescription(list + $"\n\n{user.Mention}")
                    .WithColor(Color.Blue)
                    .WithTitle(Text("user_heroes"))
                    .Build());
            }
        }

        async Task Update(string name, ulong id)
        {
            Hero character = new Hero();

            using SqliteCommand getHero = new SqliteCommand(Queries.GetCharacter(id, name), Program.sqliteConnection);
            using SqliteDataReader reader = await getHero.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                foreach (System.Data.Common.DbDataRecord record in reader)
                {
                    character = new Hero
                    {
                        Name = Convert.ToString(record["CHAR_NAME"]),
                        Owner = Convert.ToUInt64(record["CHAR_OWNER"]),
                        Image = Convert.ToString(record["CHAR_IMAGE"]),
                        Level = Convert.ToString(record["CHAR_LEVEL"]),
                        Bio = Convert.ToString(record["CHAR_BIO"]),
                        Inv = Convert.ToString(record["CHAR_INV"]),
                        Intellect = Convert.ToString(record["CHAR_INT"]),
                        Magic = Convert.ToString(record["CHAR_MAG"]),
                        Nature = Convert.ToString(record["CHAR_NAT"])
                    };
                }
            }
            else
            {
                await reader.CloseAsync();
                return;
            }
            await reader.CloseAsync();

            string[] words = character.Bio.Split('|');

            string a = Fields.part1 + character.Name +
                        Fields.part2 + character.Image +
                        Fields.part3 + Fields.SetLevels(character.Level) +
                        Fields.part5 + CommandTools.EnterRepacer(words[0]) +
                        Fields.part6 + CommandTools.EnterRepacer(words[1]) +
                        Fields.part7 + CommandTools.EnterRepacer(character.Inv) +
                        Fields.part8;

            string b = Fields.part1 + character.Name +
                        Fields.part2_bis + CommandTools.EnterRepacer(character.Intellect) +
                        Fields.part3_bis + CommandTools.EnterRepacer(character.Magic) +
                        Fields.part4_bis + CommandTools.EnterRepacer(character.Nature) +
                        Fields.part8;

            HtmlConverter converter = new HtmlConverter();
            byte[] bytes = converter.FromHtmlString(a, 770, ImageFormat.Png, 100);
            System.IO.File.WriteAllBytes($"{Config.configPath + Context.Guild.Id.ToString() + "/" + CommandTools.Translit(character.Name)}.png", bytes);

            bytes = converter.FromHtmlString(b, 770, ImageFormat.Png, 100);
            System.IO.File.WriteAllBytes($"{Config.configPath + Context.Guild.Id.ToString() + "/" + CommandTools.Translit(character.Name)}_skills.png", bytes);
        }

        [Command("add")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task AddCharacter(string name, SocketUser owner)
        {
            using SqliteCommand command = new SqliteCommand(Queries.AddChar(Context.Guild.Id, name, owner.Id), Program.sqliteConnection);
            await command.ExecuteNonQueryAsync();

            await Context.Channel.DeleteMessageAsync(Context.Message);

            string a = Fields.part1 + name + Fields.part2 + "https://media.discordapp.net/attachments/708001747842498623/708762818719252480/111.png?width=475&height=475" + Fields.part3 + Fields.SetLevels("0000000_0") + Fields.part5 + "Описание" + Fields.part6 + "Черты" + Fields.part7 + "Инвентарь" + Fields.part8;
            string b = Fields.part1 + name + Fields.part2_bis + Fields.part3_bis + Fields.part4_bis + Fields.part8;

            var converter = new HtmlConverter();
            var bytes = converter.FromHtmlString(a, 770, ImageFormat.Png, 100);
            System.IO.File.WriteAllBytes($"{Config.configPath + "/" + Context.Guild.Id.ToString() + "/" + CommandTools.Translit(name)}.png", bytes);

            bytes = converter.FromHtmlString(b, 770, ImageFormat.Png, 100);
            System.IO.File.WriteAllBytes($"{Config.configPath + "/" + Context.Guild.Id.ToString() + "/" + CommandTools.Translit(name)}_skills.png", bytes);

            await Context.Channel.SendMessageAsync(Text("char_is") + name + Text("added"));

            RestTextChannel restTextChannel = await Program._client.GetGuild(732632258489352262)
                .CreateTextChannelAsync(name, 
                x => x.CategoryId = 
                Program._client.GetGuild(732632258489352262)
                .CategoryChannels.Where(x => x.Name == Context.Guild.Id.ToString()).FirstOrDefault().Id);
            await restTextChannel.SendFileAsync($"{Config.configPath + "/" + Context.Guild.Id.ToString() + "/" + CommandTools.Translit(name)}.png", "");
        }

        [Command("updateInt")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task UpdateIntellect(string name, [Remainder] string info)
        {
            using SqliteCommand command = new SqliteCommand(Queries.UpdateInt(Context.Guild.Id, name, info), Program.sqliteConnection);
            await command.ExecuteNonQueryAsync();

            await Context.Channel.DeleteMessageAsync(Context.Message);
            await Update(name, Context.Guild.Id);
            await Context.Channel.SendMessageAsync(Text("char_is") + name + Text("updated"));
        }

        [Command("updateMag")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task UpdateMagic(string name, [Remainder] string info)
        {
            using SqliteCommand command = new SqliteCommand(Queries.UpdateMag(Context.Guild.Id, name, info), Program.sqliteConnection);
            await command.ExecuteNonQueryAsync();

            await Context.Channel.DeleteMessageAsync(Context.Message);
            await Update(name, Context.Guild.Id);
            await Context.Channel.SendMessageAsync(Text("char_is") + name + Text("updated"));
        }

        [Command("updateNat")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task UpdateNature(string name, [Remainder] string info)
        {
            using SqliteCommand command = new SqliteCommand(Queries.UpdateNat(Context.Guild.Id, name, info), Program.sqliteConnection);
            await command.ExecuteNonQueryAsync();

            await Context.Channel.DeleteMessageAsync(Context.Message);
            await Update(name, Context.Guild.Id);
            await Context.Channel.SendMessageAsync(Text("char_is") + name + Text("updated"));
        }

        [Command("updateBio")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task UpdateBio(string name, [Remainder] string info)
        {
            using SqliteCommand command = new SqliteCommand(Queries.UpdateBio(Context.Guild.Id, name, info), Program.sqliteConnection);
            await command.ExecuteNonQueryAsync();

            await Context.Channel.DeleteMessageAsync(Context.Message);
            await Update(name, Context.Guild.Id);
            await Context.Channel.SendMessageAsync(Text("char_is") + name + Text("updated"));
        }

        [Command("updateInv")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task UpdateInventory(string name, [Remainder] string info)
        {
            using SqliteCommand command = new SqliteCommand(Queries.UpdateInv(Context.Guild.Id, name, info), Program.sqliteConnection);
            await command.ExecuteNonQueryAsync();

            await Context.Channel.DeleteMessageAsync(Context.Message);
            await Update(name, Context.Guild.Id);
            await Context.Channel.SendMessageAsync(Text("char_is") + name + Text("updated"));
        }

        [Command("updateLevel")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task AddCharacterData(string name, [Remainder] string info)
        {
            using SqliteCommand command = new SqliteCommand(Queries.UpdateLevel(Context.Guild.Id, name, info), Program.sqliteConnection);
            await command.ExecuteNonQueryAsync();

            await Context.Channel.DeleteMessageAsync(Context.Message);
            await Update(name, Context.Guild.Id);
            await Context.Channel.SendMessageAsync(Text("char_is") + name + Text("updated"));
        }

        [Command("delete")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task DeleteCharacter(string name)
        {
            using SqliteCommand command = new SqliteCommand(Queries.DeleteChar(Context.Guild.Id, name), Program.sqliteConnection);
            await command.ExecuteNonQueryAsync();
            System.IO.File.Delete($"{Config.configPath + "/" + Context.Guild.Id.ToString() + "/" + name}.png");
            await Context.Channel.DeleteMessageAsync(Context.Message);
            await Context.Channel.SendMessageAsync(Text("char_is") + name + Text("deleted"));
        }

        [Command("icon")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task PicCharacter(string name, string info)
        {
            using SqliteCommand command = new SqliteCommand(Queries.ChangeImage(Context.Guild.Id, name, info), Program.sqliteConnection);
            await command.ExecuteNonQueryAsync();
            await Update(name, Context.Guild.Id);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            await Context.Channel.SendMessageAsync(Text("char_is") + name + Text("updated"));
        }

        [Command("gallery")]
        public async Task AddImage(string name)
        {
            using SqliteCommand getHero = new SqliteCommand(Queries.GetImage(Context.Guild.Id, name), Program.sqliteConnection);
            string a = "";
            using (SqliteDataReader reader = getHero.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    foreach (System.Data.Common.DbDataRecord record in reader)
                    {
                        a += Convert.ToString(record["CHAR_GALLERY"]);
                    }
                }
                else
                {
                    await reader.CloseAsync();
                    await Context.Channel.DeleteMessageAsync(Context.Message);
                    return;
                }
                await reader.CloseAsync();
            }

            await Context.Channel.DeleteMessageAsync(Context.Message);

            string[] b = a.Split('|');

            for (int i = 0; i < b.Count(); i++)
            {
                if (b[i].Length < 5)
                    continue;
                EmbedBuilder builder = new EmbedBuilder
                {
                    Title = name + " " + i.ToString(),
                    Color = Color.DarkBlue,
                    ImageUrl = b[i]
                };
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }

        [Command("tweet")]
        public async Task Tweet(string name, [Remainder]string text)
        {
            using SqliteCommand getHero = new SqliteCommand(Queries.GetCharacter(Context.Guild.Id, name), Program.sqliteConnection);
            string a = "";
            string add = "";
            string n = "";
            using (SqliteDataReader reader = getHero.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    foreach (System.Data.Common.DbDataRecord record in reader)
                    {
                        if (Convert.ToUInt64(record["CHAR_OWNER"]) != Context.User.Id)
                        {
                            await Context.Message.DeleteAsync();
                            await reader.CloseAsync();
                            return;
                        }
                        a = Convert.ToString(record["CHAR_IMAGE"]);
                        add += Convert.ToString(record["CHAR_LEVEL"])[0];
                        add += Convert.ToString(record["CHAR_LEVEL"])[1];
                        n += Convert.ToString(record["CHAR_NICKNAME"]);
                    }
                }
                else
                {
                    await reader.CloseAsync();
                    await Context.Channel.DeleteMessageAsync(Context.Message);
                    return;
                }
                await reader.CloseAsync();
            }

            await Context.Channel.DeleteMessageAsync(Context.Message);

            EmbedBuilder builder = new EmbedBuilder
            {
                Title = $"**{n}** *@{CommandTools.Translit(name).ToLower() + add }*",
                ThumbnailUrl = a,
                Description = text,
                Color = Color.DarkBlue,
            };
            builder.WithCurrentTimestamp();
            RestUserMessage messa = await Context.Channel.SendMessageAsync("", false, builder.Build());

            Emoji[] emojis1 = { new Emoji("💙") };
            await messa.AddReactionsAsync(emojis1);
        }

        [Command("nick")]
        public async Task ChanheNickname(string name, [Remainder]string text)
        {
            using SqliteCommand getHero = new SqliteCommand(Queries.GetCharacter(Context.Guild.Id, name), Program.sqliteConnection);
            using (SqliteDataReader reader = getHero.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    foreach (System.Data.Common.DbDataRecord record in reader)
                    {
                        if (Convert.ToUInt64(record["CHAR_OWNER"]) != Context.User.Id)
                        {
                            await Context.Message.DeleteAsync();
                            await reader.CloseAsync();
                            return;
                        }
                    }
                }
                else
                {
                    await reader.CloseAsync();
                    await Context.Channel.DeleteMessageAsync(Context.Message);
                    return;
                }
                await reader.CloseAsync();
            }

            using SqliteCommand command = new SqliteCommand(Queries.ChangeNickname(Context.Guild.Id, name, text), Program.sqliteConnection);
            await command.ExecuteNonQueryAsync();

            await Context.Channel.DeleteMessageAsync(Context.Message);
            await Context.Channel.SendMessageAsync(Text("char_is") + name + Text("updated"));
        }

        [Command("addImage")]
        public async Task AddImage(string name, string info)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);

            using SqliteCommand getHero = new SqliteCommand(Queries.GetImage(Context.Guild.Id, name), Program.sqliteConnection);
            using SqliteDataReader reader = await getHero.ExecuteReaderAsync();

            string imageBuffer = "";
            bool pass = false;
            
            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    if ((ulong)reader.GetInt64(0) == Context.User.Id)
                    {
                        if (!reader.IsDBNull(1))
                            imageBuffer = reader.GetString(1) + " | ";
                    }
                    else
                        pass = true;
                    break;
                }
                await reader.CloseAsync();
            }
            else
            {
                await reader.CloseAsync();
                return;
            }

            if (pass) return;

            using SqliteCommand command = new SqliteCommand(Queries.AddImage(Context.Guild.Id, name, imageBuffer + info), Program.sqliteConnection);
            await command.ExecuteNonQueryAsync();

            await Context.Channel.SendMessageAsync(Text("char_is") + name + Text("updated"));
        }

        [Command("deleteImage")]
        public async Task DeleteImage(string name, int info)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);

            using SqliteCommand getHero = new SqliteCommand(Queries.GetImage(Context.Guild.Id, name), Program.sqliteConnection);
            using SqliteDataReader reader = await getHero.ExecuteReaderAsync();

            string imageBuffer = "";
            bool pass = false;

            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    if ((ulong)reader.GetInt64(0) == Context.User.Id)
                    {
                        if (!reader.IsDBNull(1))
                            imageBuffer = reader.GetString(1) + " | ";
                    }
                    else
                        pass = true;
                    break;
                }
                await reader.CloseAsync();
            }
            else
            {
                await reader.CloseAsync();
                return;
            }

            if (pass) return;


            string[] b = imageBuffer.Split('|');

            string output = b[0];

            for (int i = 1; i < b.Count(); i++)
            {
                if (i == info-1 || b[i].Length < 5)
                    continue;
                else
                {
                    output += " | ";
                    output += b[i];
                }
            }

            using SqliteCommand command = new SqliteCommand(Queries.AddImage(Context.Guild.Id, name, output), Program.sqliteConnection);
            await command.ExecuteNonQueryAsync();

            await Context.Channel.SendMessageAsync(Text("char_is") + name + Text("updated"));
        }

        [Command("heroes")]
        public async Task AllBois()
        {
            List<Hero> characters = new List<Hero>();

            using SqliteCommand getHero = new SqliteCommand(Queries.GetAllCharacters(Context.Guild.Id), Program.sqliteConnection);

            using (SqliteDataReader reader = getHero.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    foreach (DbDataRecord record in reader)
                    {
                        characters.Add(new Hero
                        {
                            Name = Convert.ToString(record["CHAR_NAME"])
                        });
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync(Text("no_server_heroes"));
                    return;
                }
                await reader.CloseAsync();
            }

            characters.Sort((x, y) => string.Compare(x.Name, y.Name));

            EmbedBuilder builder = new EmbedBuilder();
            builder
                .WithColor(Color.Default)
                .WithTitle(Text("chars_list"));

            foreach (Hero hero in characters)
            {
                string name = hero.Name;
                name = name.Replace(@"\", "");

                int a = name.Length / 2;
                string b = "";

                if (a < 5)
                {
                    for (int i = 0; i < 5 - a; i++)
                        b += " ";
                }

                builder.AddField(x =>
                {
                    x.Name = "<:left:648177971185844235>-----------<:right:648177971504611338>";
                    x.Value = $"```{b + name}```";
                    x.IsInline = true;
                });
            }
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("stats")]
        public async Task Stats(string name)
        {
            using SqliteCommand getHero = new SqliteCommand(Queries.GetCharacter(Context.Guild.Id, name), Program.sqliteConnection);
            using SqliteDataReader reader = await getHero.ExecuteReaderAsync();

            string stats = "";

            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    stats = reader.GetString(3);
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(Text("no_user_hero"));
                return;
            }
            await reader.CloseAsync();

            int hp = Convert.ToInt32(stats[5].ToString());
            if (hp == 0) hp = 10;


            int str = Convert.ToInt32(stats[4].ToString());
            if (str == 0) str = 10;

            string block = "";
            string cdamage = "";

            if (str == 10) { cdamage = "Урон оружием ближнего боя +1\n"; block = "Блокирование +3\n"; str = 6; }
            else if (str > 6) { block = $"Блокирование +{str - 6}\n"; str = 6; }

            int dex = Convert.ToInt32(stats[2].ToString());
            if (dex == 0) dex = 10;

            int armor = Convert.ToInt32(stats[8].ToString());
            
            string parry = "";
            string damage = "";

            if (dex == 10) { damage = "Урон оружием дальнего боя +1\n"; parry = "Парирование +3\n"; dex = 6; }
            else if (dex > 6) { parry = $"Парирование +{dex - 6}\n"; dex = 6; }

            string dodge = $"Уклонение +{1 + (dex - 1) / 2 - armor}\n";
            string accuracy = $"Точность +{dex / 2}\n";

            int charisma = Convert.ToInt32(stats[1].ToString());
            if (charisma == 0) charisma = 10;

            string success = "";
            if(charisma > 6) { success = $"Бонус к НПС +{charisma - 6}\n";  charisma = 6; }
            string charm = $"Убеждение +{1 + (charisma - 1) / 2}\n";
            string folks = $"Размер отряда {charisma / 2}\n";


            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                .WithTitle(name)
                .WithDescription(
                $"Здоровье - {3 + 1 + (hp - 1) / 2}\n" +
                $"Броня - {armor}\n\n" +
                $"{block + cdamage}\n" +
                $"{dodge + accuracy + parry + damage}\n\n" +
                $"{charm + folks + success}")
                .WithColor(Color.DarkBlue)
                .WithFooter("Без учета навыков персонажа")
                .Build());
        }

        string Text(string text)
        {
            YamlDotNet.RepresentationModel.YamlMappingNode Node;

            if (Config.Countries.Contains(Context.Guild.VoiceRegionId))
                Program.mapping.TryGetValue(Context.Guild.VoiceRegionId, out Node);
            else
                Program.mapping.TryGetValue("english", out Node);

            Node.Children.TryGetValue(text, out YamlDotNet.RepresentationModel.YamlNode answer);
            return answer.ToString();
        }
    }
}
