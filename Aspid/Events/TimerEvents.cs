using System;
using Discord;
using System.Linq;
using System.Timers;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Aspid.Events
{
    internal static class TimerEvents
    {
        const int hour1 = 21;
        const int hour2 = 9;

        const int minute = 0;

        static bool canSend = true;

        internal static Task MainTimer()
        {
            Timer looper = new Timer
            {
                Interval = 60000,
                AutoReset = true,
                Enabled = true,
            };

            looper.Elapsed += async (object sender, ElapsedEventArgs e) => 
            {
                if ((DateTime.UtcNow.Hour == hour1 || DateTime.UtcNow.Hour == hour2) && DateTime.UtcNow.Minute == minute && canSend)
                {
                    canSend = false;
                   await Global.channel.SendMessageAsync("", false, new EmbedBuilder()
                        .WithTitle("Напоминание").WithColor(Color.Red)
                        .WithImageUrl("https://media.discordapp.net/attachments/614108079545647105/614108112730718249/primal_aspid.jpg?width=676&height=474")               
                        .Build());

                    IEnumerable<SocketGuild> guilds = Program._client.Guilds;
                    foreach (SocketGuild guild in guilds)
                    {
                        using SqliteCommand command1 = new SqliteCommand(Queries.GetRoles(guild.Id), Program.sqliteConnection);
                        using SqliteDataReader reader = await command1.ExecuteReaderAsync();

                        IRole role = null;

                        while (await reader.ReadAsync())
                        {
                            role = guild.Roles.FirstOrDefault(x => x.Id == (ulong)reader.GetInt64(1));
                        }
                        await reader.CloseAsync();

                        IEnumerable<SocketGuildUser> users = guild.Users.Where(x => x.Roles.Contains(role as SocketRole));

                        foreach (SocketGuildUser a in users) await (a as IGuildUser).RemoveRoleAsync(role);
                    }
                    Config.SaveDead();
                }
                if (!canSend && (DateTime.UtcNow.Hour == hour1 + 1 || DateTime.UtcNow.Hour == hour2 + 1)) 
                    canSend = true;
            };
            Config.random = new Random();
            return Task.CompletedTask;
        }
    }


    internal class MuteTimer
    {
        internal MuteTimer(int time, ulong guildId, ulong userId)
        {
            Console.WriteLine("Mute initiated");
            Timer timer = new Timer()
            {
                Interval = time * 60000,
                Enabled = true,
                AutoReset = false
            };
            timer.Elapsed += async (object sender, ElapsedEventArgs e) =>
            {
                var guild = Program._client.GetGuild(guildId);
                var user = guild.GetUser(userId);
                IRole role = null;

                using SqliteCommand command1 = new SqliteCommand(Queries.GetRoles(guildId), Program.sqliteConnection);
                using SqliteDataReader reader = await command1.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    role = guild.Roles.FirstOrDefault(x => x.Id == (ulong)reader.GetInt64(3));
                }
                await reader.CloseAsync();

                try { await (user as IGuildUser).RemoveRoleAsync(role); } 
                catch { Console.WriteLine("Cannot remove role in" + guild.Name); }

                using SqliteCommand command = new SqliteCommand(Queries.RemoveMute(guild.Id, user.Id), Program.sqliteConnection);
                await command.ExecuteNonQueryAsync();

                timer.Dispose();
            };
        }
    }

    internal class PunishTimer
    {
        public PunishTimer(ulong guildId, ulong userId)
        {
            Console.WriteLine("Mute initiated");
            Timer timer = new Timer()
            {
                Interval = 21600000,
                Enabled = true,
                AutoReset = false
            };
            timer.Elapsed += async (object sender, ElapsedEventArgs e) =>
            {
                var guild = Program._client.GetGuild(guildId);
                var user = guild.GetUser(userId);
                IRole role = null;

                using SqliteCommand command1 = new SqliteCommand(Queries.GetRoles(guildId), Program.sqliteConnection);
                using SqliteDataReader reader = await command1.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    role = guild.Roles.FirstOrDefault(x => x.Id == (ulong)reader.GetInt64(2));
                }
                await reader.CloseAsync();

                try { await (user as IGuildUser).RemoveRoleAsync(role); }
                catch { Console.WriteLine("Cannot remove role in" + guild.Name); }

                using SqliteCommand command = new SqliteCommand(Queries.RemovePunish(guild.Id, user.Id), Program.sqliteConnection);
                await command.ExecuteNonQueryAsync();

                timer.Dispose();
            };
        }
    }
}
