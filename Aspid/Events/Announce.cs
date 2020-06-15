using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace Aspid.Events
{
    class Announce
    {
        internal static async Task AnnounceUserJoined(SocketGuildUser arg)
        {
            await arg.Guild.DefaultChannel.SendMessageAsync("", false,
                new EmbedBuilder()
                .WithColor(Color.DarkGreen)
                .WithCurrentTimestamp()
                .WithThumbnailUrl(arg.GetAvatarUrl())
                .WithDescription("**" + (arg as IUser).Username + Text("arrive", arg.Guild.Id))
                .WithTitle(Text("arrive_head", arg.Guild.Id))
                .Build());
        }
        internal static async Task AnnounceUserLeft(SocketGuildUser arg)
        {
            await arg.Guild.DefaultChannel.SendMessageAsync("", false,
                new EmbedBuilder()
                .WithColor(Color.Red)
                .WithCurrentTimestamp()
                .WithThumbnailUrl(arg.GetAvatarUrl())
                .WithDescription("**" + (arg as IUser).Username + Text("gone", arg.Guild.Id))
                .WithTitle(Text("gone_head", arg.Guild.Id))
                .Build());
        }

        static string Text(string text, ulong guild)
        {
            YamlDotNet.RepresentationModel.YamlMappingNode Node;

            if (Config.Countries.Contains(Program._client.GetGuild(guild).VoiceRegionId))
                Program.mapping.TryGetValue(Program._client.GetGuild(guild).VoiceRegionId, out Node);
            else
                Program.mapping.TryGetValue("english", out Node);

            Node.Children.TryGetValue(text, out YamlDotNet.RepresentationModel.YamlNode answer);
            return answer.ToString();
        }
    }
}
