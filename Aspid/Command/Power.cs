using System;
using Discord;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Aspid.Command
{
    public class Power : ModuleBase<SocketCommandContext>
    {
        [Command("stop")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task KillAspid()
        {
            SocketGuild myguild = Context.Guild;
            SocketRole role = myguild.Roles.FirstOrDefault(x => x.Name == "Muted");

            await Context.Channel.SendMessageAsync(Text("stop"));
            await (myguild.GetUser(581221797295554571) as IGuildUser).AddRoleAsync(role);
        }

        [Command("start")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task StartAspid()
        {
            SocketGuild myguild = Context.Guild;
            SocketRole role = myguild.Roles.FirstOrDefault(x => x.Name == "Muted");

            await (myguild.GetUser(581221797295554571) as IGuildUser).RemoveRoleAsync(role);
            await Context.Channel.SendMessageAsync(Text("start"));
        }

        [Command("break")]
        public async Task StopBot()
        {
            if (Context.User.Id != 264811248552640522)
            {
                await Context.Message.DeleteAsync();
                return;
            }

            Config.SaveDead();

            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                .WithTitle(Text("break"))
                .WithColor(Color.DarkGreen)
                .WithImageUrl("https://media.discordapp.net/attachments/614108079545647105/709052808124432497/AspidOnRepair.gif")
                .Build());

            try
            {
                //await Global.HelpHandler.Item1.DeleteAsync();
            }
            catch { Console.WriteLine("Cannot delete that message"); }

            Environment.Exit(1);
        }

        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("purge")]
        public async Task Purge(int amount = 0)
        {
            IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
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
