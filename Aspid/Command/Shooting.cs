using System;
using Discord;
using System.Linq;
using Discord.Commands;
using System.Threading;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Aspid.Command
{
    public class Shooting : ModuleBase<SocketCommandContext>
    {
        //kys and get free role with 1/6 chance
        [Command("gun")]
        public async Task RussianRoulette()
        {
            await Context.Message.AddReactionAsync(Emote.Parse("<:revolver:603601152885522465>"));
            
            var user = Context.User as SocketGuildUser;

            IRole dead = null;
            using SqliteCommand command1 = new SqliteCommand(Queries.GetRoles(Context.Guild.Id), Program.sqliteConnection);
            using SqliteDataReader reader1 = await command1.ExecuteReaderAsync();
            while (await reader1.ReadAsync())
            {
                dead = Context.Guild.Roles.FirstOrDefault(x => x.Id == (ulong)reader1.GetInt64(1));
            }
            await reader1.CloseAsync();

            if (user.Roles.Contains(dead))
            {
                await Context.Channel.SendMessageAsync(Text("gun_dead"));
                return;
            }

            Thread thread = new Thread(x =>
            {
                Thread.Sleep(1000); Context.Channel.SendMessageAsync(Text("gun_1"), false);
                Thread.Sleep(2000); Context.Channel.SendMessageAsync(Text("gun_2"), false);
                Thread.Sleep(2000); Context.Channel.SendMessageAsync(Text("gun_3"), false);
                Thread.Sleep(4000);

                if (new Random().Next(0, 6) == 0)
                {
                    Config.bot.DeadPeople++;

                    Context.Channel.SendMessageAsync(Text("gun_4.a.1") + Context.User.Mention + Text("gun_4.a.2"), false);
                    (Context.User as IGuildUser).AddRoleAsync(dead);
                }
                else 
                    Context.Channel.SendMessageAsync(Text("gun_4.b"), false);
            });
            thread.Start();
        }

        //shoot eople and grant them role
        [Command("shoot")]
        public async Task Shoot(SocketGuildUser user)
        {
            await Context.Message.AddReactionAsync(Emote.Parse("<:revolver:603601152885522465>"));

            IRole dead = null;
            using SqliteCommand command1 = new SqliteCommand(Queries.GetRoles(Context.Guild.Id), Program.sqliteConnection);
            using SqliteDataReader reader1 = await command1.ExecuteReaderAsync();
            while (await reader1.ReadAsync())
            {
                dead = Context.Guild.Roles.FirstOrDefault(x => x.Id == (ulong)reader1.GetInt64(1));
            }
            await reader1.CloseAsync();

            if (user.IsBot)
            {
                await MissMessage(Text("shoot_aspid")); return;
            }
            else if(user == Context.User)
            {
                await MissMessage(Text("shoot_self")); return;
            }
            else if((Context.User as SocketGuildUser).Roles.Contains(dead))
            {
                await MissMessage(Text("shoot_dead")); return;
            }
            else if (user.Roles.Contains(dead))
            {
                await MissMessage(Text("shoot_at_dead")); return;
            }

            if(new Random().Next(0, 2) == 0)
            {
                Config.bot.DeadPeople++;

                await Context.Channel.SendMessageAsync(Text("shoot.a") + user.Mention, false);
                await (user as IGuildUser).AddRoleAsync(dead);
            }
            else
                await MissMessage(Text("shoot.b") + "<:PKHeh:575051447906074634>");
        }

        //Just help to add and delete messages
        async Task MissMessage(string message)
        {
            Global.missCache.Enqueue((Context.Message, await Context.Channel.SendMessageAsync(message, false)));
            if (Global.missCache.Count > 3)
            {
                await Global.missCache.Peek().Item1.DeleteAsync();
                await Global.missCache.Dequeue().Item2.DeleteAsync();
            }
        }

        //"Resuurect" all dead people
        [Command("save")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Resurrect()
        {
            IRole dead = null;
            using SqliteCommand command1 = new SqliteCommand(Queries.GetRoles(Context.Guild.Id), Program.sqliteConnection);
            using SqliteDataReader reader1 = await command1.ExecuteReaderAsync();
            while (await reader1.ReadAsync())
            {
                dead = Context.Guild.Roles.FirstOrDefault(x => x.Id == (ulong)reader1.GetInt64(1));
            }
            await reader1.CloseAsync();
            var deadUsers = Context.Guild.Users.Where(x => x.Roles.Contains(dead));
            foreach(var user in deadUsers)
            {
                await (user as IGuildUser).RemoveRoleAsync(dead);
            }
            Config.SaveDead();
            await Context.Channel.SendMessageAsync(">>> " + deadUsers.Count() + Text("save_1") + Config.bot.DeadPeople + Text("save_2"), false);
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
