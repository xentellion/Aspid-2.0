using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aspid
{
    class Global
    {
        //Queue of input $shoot commands. which are to be cleaned up
        internal static Queue<(SocketMessage, RestUserMessage)> missCache = new Queue<(SocketMessage, RestUserMessage)>();

        internal static ISocketMessageChannel channel = (ISocketMessageChannel)Program._client.GetGuild(567767402062807055).GetChannel(567770314642030592);

        //Array of grub images and cache of them
        internal static IEnumerable<IMessage> Grubs { get; set; }
        internal static Queue<(SocketUserMessage, RestUserMessage)> GrubCashe = new Queue<(SocketUserMessage, RestUserMessage)>();

        internal static (RestUserMessage, int) HelpHandler { get; set; }
    }
}
