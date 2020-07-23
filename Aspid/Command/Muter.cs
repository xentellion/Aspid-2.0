using System;
using Discord;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace Aspid.Command
{
    public class Muter : ModuleBase<SocketCommandContext>
    {
        [Command("mute")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Mute(SocketGuildUser user, string timer, [Remainder]string reason = null)
        {
            if (user.IsBot)
            {
                await Context.Message.DeleteAsync();
                return;
            }

            char time = timer.Last();
            string length = timer.Substring(0, timer.Length - 1);
            int muteTime = Convert.ToInt32(length);
            string longitud = "";

            switch (time)
            {
                case 'm': longitud = Text("minutes"); break;
                case 'h': muteTime *= 60; longitud = Text("hours"); break;
                case 'd': muteTime *= 1440; longitud = Text("days"); break;
                case 'w': muteTime *= 10080; longitud = Text("weeks"); break;
                case 'y': muteTime *= 3679200; longitud = Text("years"); break;
                default: return;
            }

            await Context.Channel.SendMessageAsync("🔇 " + user.Mention + Text("mute_in_channel") + muteTime + " " + longitud + "**");

            using SqliteCommand command = new SqliteCommand(Queries.AddMute(Context.Guild.Id, user.Id, (ulong)muteTime), Program.sqliteConnection);
            await command.ExecuteNonQueryAsync();

            IRole role = null;
            using SqliteCommand command1 = new SqliteCommand(Queries.GetRoles(Context.Guild.Id), Program.sqliteConnection);
            using SqliteDataReader reader = await command1.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                role = Context.Guild.Roles.FirstOrDefault(x => x.Id == (ulong)reader.GetInt64(3));
            }
            await reader.CloseAsync();

            await user.AddRoleAsync(role);

            var ls = await user.GetOrCreateDMChannelAsync();

            if (reason == null) reason = Text("mute_no_reason");

            new Events.MuteTimer(muteTime, Context.Guild.Id, user.Id);

            await ls.SendMessageAsync("", false, new EmbedBuilder()
                .WithTitle(Text("mute_head"))
                .WithDescription(Text("mute_1") + Context.Guild.Name + Text("mute_2") + muteTime + " " + longitud + Text("mute_3") + reason)
                .WithColor(Color.Red)
                .WithThumbnailUrl("https://media.discordapp.net/attachments/603600328117583874/615150515709411357/ezgif.com-gif-maker_31.gif")
                .WithCurrentTimestamp()
                .Build());
        }

        [Command("punish")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Punish(SocketGuildUser user)
        {
            IRole role = null;
            using SqliteCommand command1 = new SqliteCommand(Queries.GetRoles(Context.Guild.Id), Program.sqliteConnection);
            using SqliteDataReader reader = await command1.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                role = Context.Guild.Roles.FirstOrDefault(x => x.Id == (ulong)reader.GetInt64(2));
            }
            await reader.CloseAsync();


            using SqliteCommand command = new SqliteCommand(Queries.AddPunish(Context.Guild.Id, user.Id), Program.sqliteConnection);
            await command.ExecuteNonQueryAsync();

            try { await (user as IGuildUser).AddRoleAsync(role); } catch { Console.WriteLine("Cannot add role"); }

            new Events.PunishTimer(Context.Guild.Id, user.Id);
            await Context.Channel.SendMessageAsync(Text("punish_1") + user.Mention + Text("punish_2"), false);
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
