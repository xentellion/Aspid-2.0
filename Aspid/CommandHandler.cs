using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Aspid
{
    class CommandHandler
    {
        CommandService _service = new CommandService();

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            await _service.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly(), services: null);
            client.MessageReceived += async (SocketMessage) =>
            {
                var message = (SocketUserMessage)SocketMessage;
                var context = new SocketCommandContext(client, message);

                if (message == null) return;
                if (context.User.IsBot) return;

                //react on any aspid in chat
                if (message.Content.ToLower().Contains("аспид") || message.Content.ToLower().Contains("aspid"))
                    await context.Message.AddReactionAsync(Discord.Emote.Parse("<:Aspid:567801319197245448>"));

                //Send emote when there is F in chat
                if(message.Content.ToUpper().Contains("F") && message.Content.Length == 1)
                {
                    await message.DeleteAsync();
                    Discord.Rest.RestUserMessage rest = await context.Channel.SendMessageAsync("🇫", false);
                    await rest.AddReactionAsync(Discord.Emote.Parse("<:F_:575051447717330955>"));
                }

                int argPos = 0;

                //command executing
                if(message.HasStringPrefix(Config.bot.Prefix, ref argPos))
                {
                    //Checks author roles. If there is "Punished" role - it restricts bot using
                    if ((message.Author as SocketGuildUser).Roles.Contains((message.Channel as SocketGuildChannel).Guild.Roles.FirstOrDefault(x => x.Name == "Punished")))
                    {
                        await (message.Channel as ISocketMessageChannel).DeleteMessageAsync(message);
                        return;
                    }
                    var result = await _service.ExecuteAsync(context, argPos, services: null);
                    if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    {
                        System.Console.WriteLine(result.ErrorReason);
                    }
                }
            };
        } 
    }
}
