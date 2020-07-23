using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using System.Linq;
using Discord.WebSocket;

namespace Aspid.Command
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task HelpList()
        {
            await Context.Message.AddReactionAsync(Emote.Parse("<:ThinkRadiance:567800282797309957>"));
            if (Global.HelpHandler.Item1 != null)
            {
                try
                {
                    await Global.HelpHandler.Item1.DeleteAsync();
                }
                catch { }
            }

            EmbedBuilder builder = new EmbedBuilder();
            builder
                .WithTitle(Text("help_header").ToString())
                .WithColor(Color.DarkBlue)
                .AddField(Text("help_titles")[0].ToString(), Text("help")[0].ToString())
                .WithFooter(Text("help_page") + "1/" + (Text("help").AllNodes.Count()-1).ToString());

            Discord.Rest.RestUserMessage a = await Context.Channel.SendMessageAsync("", false, builder.Build());

            await a.AddReactionAsync(new Emoji("◀"));
            await a.AddReactionAsync(new Emoji("▶"));
            Global.HelpHandler = (a, 0);
        }

        public static async Task Turn(string side)
        {
            bool changed = false;

            if (side == null) return;
            else if (side == "◀" && Global.HelpHandler.Item2 > 0)
            {
                int a = Global.HelpHandler.Item2;
                a--;
                Global.HelpHandler = (Global.HelpHandler.Item1, a);
                changed = true;
            }
            else if (side == "▶" && Global.HelpHandler.Item2 < 3)
            {
                int a = Global.HelpHandler.Item2;
                a++;
                Global.HelpHandler = (Global.HelpHandler.Item1, a);
                changed = true;
            }

            if (changed)
            {
                await Global.HelpHandler.Item1.ModifyAsync(a =>
                {
                    EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle(Text("help_header", (Global.HelpHandler.Item1.Channel as SocketGuildChannel).Guild.Id).ToString())
                    .WithColor(Color.DarkBlue)
                    .AddField(Text("help_titles", (Global.HelpHandler.Item1.Channel as SocketGuildChannel).Guild.Id)[Global.HelpHandler.Item2].ToString(), Text("help", (Global.HelpHandler.Item1.Channel as SocketGuildChannel).Guild.Id)[Global.HelpHandler.Item2].ToString())
                    .WithFooter(Text("help_page", (Global.HelpHandler.Item1.Channel as SocketGuildChannel).Guild.Id) + $"{Global.HelpHandler.Item2+1}/4");

                    a.Embed = builder.Build();
                });
                await Global.HelpHandler.Item1.UpdateAsync();
            }
        }

        YamlNode Text(string text)
        {
            YamlMappingNode Node;

            if (Config.Countries.Contains(Context.Guild.VoiceRegionId))
                Program.mapping.TryGetValue(Context.Guild.VoiceRegionId, out Node);
            else
                Program.mapping.TryGetValue("english", out Node);

            Node.Children.TryGetValue(text, out YamlNode answer);
            return answer;
        }

        static YamlNode Text(string text, ulong guild)
        {
            YamlMappingNode Node;

            if (Config.Countries.Contains(Program._client.GetGuild(guild).VoiceRegionId))
                Program.mapping.TryGetValue(Program._client.GetGuild(guild).VoiceRegionId, out Node);
            else
                Program.mapping.TryGetValue("english", out Node);

            Node.Children.TryGetValue(text, out YamlNode answer);
            return answer;
        }
    }
}
