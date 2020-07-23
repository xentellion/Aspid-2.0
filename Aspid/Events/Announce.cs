using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
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

            var channel = Program._client.GetChannel(567770314642030592) as SocketTextChannel;
            if (arg.Guild.Id == 567767402062807055)
            {
                EmbedBuilder builder = new EmbedBuilder();
                builder
                    .WithTitle($"Добро пожаловать!")
                    .WithDescription($"Должно быть, долгим был твой путь, {arg.Mention}, - и потому мы не будем томить тебя длинными речами. Все, что тебе нужно знать, от наших законов и обычаев, до путеводителя по нашему миру, ты можешь найти здесь - в разделе {arg.Guild.GetTextChannel(567768871654916112).Mention}. Если же тебе понадобится помощь или возникнут вопросы - то мы всегда будем ждать тебя у костра - {arg.Guild.GetTextChannel(567770314642030592).Mention}. А пока что, осматривайся, осваивайся, отдыхай после долгой дороги.\n" + "\n" +
                                    "И когда будешь готов - узри величайшую и единственную цивилизацию - Королевство Халлоунест!")
                    .WithColor(Color.DarkBlue);
                await channel.SendMessageAsync($"{arg.Mention}", false, builder.Build());
            }

            using SqliteCommand npgSqlCommand = new SqliteCommand(Queries.AddUser(arg.Guild.Id, arg.Id), Program.sqliteConnection);
            await npgSqlCommand.ExecuteNonQueryAsync();
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
        internal static async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> cashe, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!reaction.User.Value.IsBot)
            {
                if (Global.HelpHandler.Item1 != null && reaction.MessageId == Global.HelpHandler.Item1.Id)
                {
                    if (reaction.Emote.Name == "◀" || reaction.Emote.Name == "▶")
                    {
                        await Global.HelpHandler.Item1.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                        await Command.Help.Turn(reaction.Emote.Name);
                    }
                }
            }
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
