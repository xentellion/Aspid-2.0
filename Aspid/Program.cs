﻿using System;
using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace Aspid
{
    class Program
    {
        internal static DiscordSocketClient _client;
        internal static SqliteConnection sqliteConnection;
        internal static Dictionary<string, YamlMappingNode> mapping = new Dictionary<string, YamlMappingNode>();

        static void Main() => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            //Check if there is bot token
            if (Config.bot.Token == "" || Config.bot.Token == null)
            {
                Console.WriteLine("Configuration string is empty or does not exist"); return;
            }

            //connect to database
            sqliteConnection = new SqliteConnection(Config.connectionString);
            sqliteConnection.Open();
            Console.WriteLine("Connected to SQLite database " + sqliteConnection.Database);

            //set up console logs
            _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });
            _client.Log += (LogMessage) => { Console.WriteLine(LogMessage); return Task.CompletedTask; };

            //a lot of events 
            _client.UserJoined += Events.Announce.AnnounceUserJoined;
            _client.UserLeft += Events.Announce.AnnounceUserLeft;

            //log in
            await _client.LoginAsync(TokenType.Bot, Config.bot.Token);
            await _client.SetGameAsync("ваши крики", null, ActivityType.Listening);
            await _client.StartAsync();

            //initiate 
            await new CommandHandler().InitializeAsync(_client);
            await Task.Delay(-1);
        }
    }
}
