using System;
using Discord;
using System.Linq;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace Aspid.Events
{
    internal class RoleHandler
    {
        internal static async Task AddRoles(SocketGuild arg)
        {
            try
            {
                using var command0 = new SqliteCommand(Queries.AddGuild(arg.Id), Program.sqliteConnection);
                await command0.ExecuteNonQueryAsync();
            }
            catch { Console.WriteLine("Guild already exists!"); }

            using SqliteCommand command1 = new SqliteCommand(Queries.GetRoles(arg.Id), Program.sqliteConnection);
            using SqliteDataReader reader = await command1.ExecuteReaderAsync();

            ulong[] roles = { 0, 0, 0 };
            string[] roleNames = { "Dead", "Punished", "Muted" };
            Color[] roleColors = { new Color(0, 0, 1), new Color(), new Color(129, 131, 134) };
            GuildPermissions[] permissions = { new GuildPermissions(), new GuildPermissions(), new GuildPermissions(1024) };

            while (await reader.ReadAsync())
            {
                for (int i = 0; i < 3; i++)
                {
                    if (reader.GetInt64(i + 1) == 0)
                    {
                        if (arg.Roles.FirstOrDefault(x => x.Name == roleNames[i]) == null)
                        {
                            var mutedRole = await arg.CreateRoleAsync(roleNames[i], permissions[i], roleColors[i], true, null);
                            roles[i] = mutedRole.Id;
                        }
                        else
                            roles[i] = arg.Roles.FirstOrDefault(x => x.Name == roleNames[i]).Id;
                    }
                    else
                    {
                        if (arg.Roles.FirstOrDefault(x => x.Id == (ulong)reader.GetInt64(i + 1)) == null)
                        {
                            var mutedRole = await arg.CreateRoleAsync(roleNames[i], permissions[i], roleColors[i], true, null);
                            roles[i] = mutedRole.Id;
                        }
                        else
                            roles[i] = (ulong)reader.GetInt64(i + 1);
                    }
                }
            }
            await reader.CloseAsync();

            using var command = new SqliteCommand(Queries.UpdateRole(arg.Id, roles[0], roles[1], roles[2]), Program.sqliteConnection);
            await command.ExecuteNonQueryAsync();

            foreach(var channel in arg.Channels)
            {
                if ((channel as IGuildChannel).GetPermissionOverwrite(arg.Roles.FirstOrDefault(x => x.Id == roles[2])) == null)
                    await (channel as IGuildChannel).AddPermissionOverwriteAsync(arg.Roles.FirstOrDefault(x => x.Id == roles[2]), new OverwritePermissions(PermValue.Inherit, PermValue.Inherit, PermValue.Deny, PermValue.Inherit, PermValue.Deny, PermValue.Deny));
            }
        }

        internal static async Task AddTables()
        {
            foreach (SocketGuild guild in Program._client.Guilds)
            {
                await guild.DownloadUsersAsync();
                try
                {
                    using SqliteCommand check = new SqliteCommand("SELECT * FROM guild_" + guild.Id, Program.sqliteConnection);
                    using SqliteDataReader reader = await check.ExecuteReaderAsync();
                    await reader.CloseAsync();
                }
                catch
                {
                    using SqliteCommand addTable = new SqliteCommand(Queries.CreateTable(guild.Id), Program.sqliteConnection);
                    await addTable.ExecuteNonQueryAsync();
                }

                using SqliteCommand command1 = new SqliteCommand(Queries.GetRoles(guild.Id), Program.sqliteConnection);
                using SqliteDataReader reader1 = await command1.ExecuteReaderAsync();

                IRole muted = null;
                IRole punished = null;

                while (await reader1.ReadAsync())
                {
                    muted = guild.Roles.FirstOrDefault(x => x.Id == (ulong)reader1.GetInt64(3));
                    punished = guild.Roles.FirstOrDefault(x => x.Id == (ulong)reader1.GetInt64(2));
                }
                await reader1.CloseAsync();

                foreach (SocketGuildUser user in guild.Users)
                {
                    SqliteCommand check = new SqliteCommand(Queries.GetUser(guild.Id, user.Id), Program.sqliteConnection);
                    if (Convert.ToInt64(await check.ExecuteScalarAsync()) == 0)
                    {
                        SqliteCommand addUser = new SqliteCommand(Queries.AddUser(guild.Id, user.Id), Program.sqliteConnection);
                        await addUser.ExecuteNonQueryAsync();
                    }

                    using SqliteDataReader reader = await check.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        if (reader.GetInt64(1) != 0)
                        {
                            await user.AddRoleAsync(muted);
                            new MuteTimer((int)reader.GetInt64(1), guild.Id, user.Id);
                        }
                        if (reader.GetInt64(2) != 0)
                        {
                            await user.AddRoleAsync(punished);
                            new PunishTimer(guild.Id, user.Id);
                        }
                    }
                    await reader.CloseAsync();
                }
            }
        }

        internal static async Task CheckRoles()
        {
            foreach(var arg in Program._client.Guilds)
            {
                try { await AddRoles(arg); }
                catch { }
                if(!Program._client.GetGuild(732632258489352262).CategoryChannels.Any(x => x.Name == arg.Id.ToString()))
                {
                    await Program._client.GetGuild(732632258489352262).CreateCategoryChannelAsync(arg.Id.ToString());
                }
            }
            
        }

        internal static async Task AddPermissions(SocketChannel arg)
        {
            try
            {
                using SqliteCommand command = new SqliteCommand(Queries.GetRoles((arg as SocketGuildChannel).Guild.Id), Program.sqliteConnection);
                using SqliteDataReader reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        IRole role = (arg as SocketGuildChannel).Guild.Roles.FirstOrDefault(x => x.Id == (ulong)reader.GetInt64(3));
                        if ((arg as IGuildChannel).GetPermissionOverwrite(role) == null)
                            await (arg as IGuildChannel).AddPermissionOverwriteAsync(role, new OverwritePermissions(PermValue.Inherit, PermValue.Inherit, PermValue.Deny, PermValue.Inherit, PermValue.Deny, PermValue.Deny));
                    }
                }
                await reader.CloseAsync();
            }
            catch { return; }
        }
    }
}
