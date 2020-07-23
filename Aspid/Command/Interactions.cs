using Discord.Commands;
using Discord;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Discord.WebSocket;
using YamlDotNet.RepresentationModel;
using Discord.Rest;
using Microsoft.Data.Sqlite;
using Discord.Webhook;

namespace Aspid.Command
{
    public class Interactions : ModuleBase<SocketCommandContext>
    {
        [Command("pet")]
        public async Task Pet(SocketGuildUser user = null)
        {
            if (user == null)
            {
                await Context.Message.AddReactionAsync(Emote.Parse("<:Aspid:567801319197245448>"));

                IRole dead = null;
                using SqliteCommand command1 = new SqliteCommand(Queries.GetRoles(Context.Guild.Id), Program.sqliteConnection);
                using SqliteDataReader reader1 = await command1.ExecuteReaderAsync();
                while (await reader1.ReadAsync())
                {
                    dead = Context.Guild.Roles.FirstOrDefault(x => x.Id == (ulong)reader1.GetInt64(1));
                }
                await reader1.CloseAsync();

                if ((Context.User as SocketGuildUser).Roles.Contains(dead))
                {
                    await Context.Channel.SendMessageAsync(Text("pet_dead").ToString()); return;
                }

                var answers = Text("pet_answers");

                int i = new Random().Next(0, answers.AllNodes.Count() - 1);
                if (i == 0)
                {
                    Config.bot.DeadPeople++;
                    await (Context.User as IGuildUser).AddRoleAsync(dead);
                }
                await Context.Channel.SendMessageAsync(answers[i].ToString());
            }
            //if message has ping than pinged person is petted
            else
            {
                IRole dead = null;
                IRole punished = null;
                using SqliteCommand command1 = new SqliteCommand(Queries.GetRoles(Context.Guild.Id), Program.sqliteConnection);
                using SqliteDataReader reader1 = await command1.ExecuteReaderAsync();
                while (await reader1.ReadAsync())
                {
                    dead = Context.Guild.Roles.FirstOrDefault(x => x.Id == (ulong)reader1.GetInt64(1));
                    punished = Context.Guild.Roles.FirstOrDefault(x => x.Id == (ulong)reader1.GetInt64(2));
                }
                await reader1.CloseAsync();

                if (user.IsBot)
                {
                    await Context.Channel.SendMessageAsync(Text("pet_aspid").ToString()); return;
                }
                else if (user == Context.User)
                {
                    await Context.Channel.SendMessageAsync(Text("pet_self").ToString()); return;
                }
                else if ((Context.User as SocketGuildUser).Roles.Contains(dead))
                {
                    await Context.Channel.SendMessageAsync(Text("dead_pet").ToString()); return;
                }
                else if (user.Roles.Contains(dead))
                {
                    await Context.Channel.SendMessageAsync(Text("pet_to_dead").ToString()); return;
                }
                else if (user.Roles.Contains(punished))
                {
                    await Context.Channel.SendMessageAsync(Text("pet_punished").ToString()); return;
                }

                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    .WithColor(Color.Magenta)
                    .WithDescription(user.Mention + Text("is_pet").ToString() + Context.User.Mention)
                    .Build());
            }
        }

        //ask aspid
        //string question is used to prevent answering empty queries 
        [Command("ask")]
        public async Task Ask([Remainder]string question = "")
        {
            if(question == "")
            {
                await Context.Channel.SendMessageAsync(Text("ask_no").ToString());
                return;
            }
            var answers = Text("ask_answers");
            await Context.Channel.SendMessageAsync(answers[new Random().Next(0, answers.AllNodes.Count() - 1)].ToString());
        }

        //just send image of fancy grub
        [Command("grub")]
        public async Task Grub()
        {
            await Context.Message.AddReactionAsync(Emote.Parse("<:cosplay:603601170346541058>"));

            int count = new Random().Next(0, Global.Grubs.Count());
            string grub = Global.Grubs.ElementAt(count).Attachments.First().Url;

            RestUserMessage newGrub = await Context.Channel.SendMessageAsync("", false, 
                new EmbedBuilder()
                    .WithImageUrl(grub)
                    .WithColor(Color.Green)
                    .Build());

            Global.GrubCashe.Enqueue((Context.Message, newGrub));
            if(Global.GrubCashe.Count > 2)
            {
                await Global.GrubCashe.Peek().Item1.DeleteAsync();
                await Global.GrubCashe.Dequeue().Item2.DeleteAsync();
            }
        }

        //add bot on your server
        //it just sends link in DM
        [Command("join")]
        public async Task AddBot()
        {
            IDMChannel ls = await Context.User.GetOrCreateDMChannelAsync();

            await ls.SendMessageAsync("", false, new EmbedBuilder()
                .WithTitle(Text("join").ToString())
                .WithColor(Color.DarkGreen)
                .WithDescription("https://discordapp.com/oauth2/authorize?&client_id=581221797295554571&scope=bot&permissions=8")
                .WithThumbnailUrl("https://media.discordapp.net/attachments/603600328117583874/615150515210420249/primal_aspid_king.gif")
                .Build());
        }


        //create yes/no poll
        [Command("vote")]
        public async Task CreateVote([Remainder] string input)
        {
            await Context.Message.DeleteAsync();

            RestUserMessage message = await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                .WithTitle(Text("vote_head").ToString())
                .WithDescription(
                    input + "\n\n" +
                    "<:THK_Good:575051447599628311>" + Text("vote_good").ToString() +
                    "<:THK_Bad:575796719078473738>" + Text("vote_bad").ToString()   )
                .WithFooter(Text("vote_by").ToString() + (Context.User as SocketGuildUser).Nickname)
                .WithColor(Color.Red)
                .Build());

            await message.AddReactionAsync(Emote.Parse("<:THK_Good:575051447599628311>"));
            await message.AddReactionAsync(Emote.Parse("<:THK_Bad:575796719078473738>"));
        }

        //gives link on github repo for this bot
        [Command("code")]
        public async Task Code()
        {
            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                .WithTitle(Text("code_head").ToString())
                .WithDescription(Text("code_body").ToString())
                .WithColor(Color.Red)
                .WithImageUrl("https://media.discordapp.net/attachments/614108079545647105/629782304738377738/3032408cf9e547dc.png")
                .WithCurrentTimestamp()
                .Build());
        }

        //server info
        [Command("server")]
        public async Task ServerInfo()
        {
            SocketGuild guild = Context.Guild;
            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                .WithTitle(Text("server_head").ToString())
                .WithDescription(
                    Text("server_1").ToString() + guild.Name + 
                    Text("server_2").ToString() + guild.CreatedAt.ToLocalTime() + "\n\n" +
                    Text("server_3").ToString() + guild.Users.Count + 
                    Text("server_4").ToString() + guild.Roles.Count + 
                    Text("server_5").ToString() + guild.Channels.Count + 
                    Text("server_6").ToString())
                .WithCurrentTimestamp()
                .WithThumbnailUrl(guild.IconUrl)
                .WithColor(Color.DarkPurple)
                .Build());
        }

        //info about all aspid servers
        [Command("servers")]
        public async Task Servers()
        {
            if (Context.User.Id != 264811248552640522)
            {
                await Context.Message.DeleteAsync();
                return;
            }
            System.Collections.Generic.IEnumerable<SocketGuild> guilds = Program._client.Guilds;
            Console.WriteLine(guilds.First());
            Thread thread = new Thread(x =>
            {
                foreach (SocketGuild guild in guilds)
                {
                    Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                        .WithTitle(Text("server_head").ToString())
                        .WithDescription(
                            Text("server_1").ToString() + guild.Name +
                            Text("server_2").ToString() + guild.CreatedAt.ToLocalTime() + "\n\n" +
                            Text("server_3").ToString() + guild.Users.Count +
                            Text("server_4").ToString() + guild.Roles.Count +
                            Text("server_5").ToString() + guild.Channels.Count +
                            Text("server_6").ToString())
                        .WithCurrentTimestamp()
                        .WithThumbnailUrl(guild.IconUrl)
                        .WithColor(Color.DarkPurple)
                        .Build());
                }
            });
            thread.Start();
        }

        //send message to any aspid server
        [Command("global")]
        public async Task GlobalAnnounce([Remainder] string text)
        {
            if (Context.User.Id != 264811248552640522)
            {
                await Context.Message.DeleteAsync();
                return;
            }
            System.Collections.Generic.IEnumerable<SocketGuild> guilds = Program._client.Guilds;
            Thread thread = new Thread(x =>
            {
                foreach (SocketGuild guild in guilds)
                {
                    try
                    {
                        guild.DefaultChannel.SendMessageAsync("", false, new EmbedBuilder()
                            .WithTitle("ATTENTION")
                            .WithColor(Color.Default)
                            .WithDescription(text)
                            .Build());
                    }
                    catch
                    {
                        Console.WriteLine("Cannot send to " + guild.Name);
                    }
                }
            });
            thread.Start();
            await Context.Message.DeleteAsync();
        }

        //select 1 from several (or 1 from 1 lol)
        [Command("pick")]
        public async Task Pick([Remainder]string text)
        {
            string[] words = text.Split('|');
            await Context.Channel.SendMessageAsync(words[new Random().Next(0, words.Count())]);
        }

        //make word from given alphabet
        [Command("blend")]
        public async Task Blendname(string alphabet, int count = 6)
        {
            if(alphabet.Count() < count)
            {
                await Context.Channel.SendMessageAsync(Text("blender").ToString()); return;
            }
            char[] letters = alphabet.ToCharArray();
            string name = "";
            for(int i = 0; i < count; i++)
            {
                for (; ; )
                {
                    int a = new Random().Next(0, letters.Count());
                    if (letters[a] != '∰')
                    {
                        name += letters[a];
                        letters[a] = '∰';
                        break;
                    }
                    else
                        continue;
                }
            }
            await Context.Channel.SendMessageAsync(name.ToLower());
        }

        [Command("roll")]
        public async Task RollDice([Remainder] string input = "20+0")
        {
            input = input.Replace(" ", "");
            int modifier = 0;
            int counter = 1;
            int mod = 0;

            for (int i = input.Length - 1; i >= 0; i--)
            {                
                try
                {
                    mod = Convert.ToInt32(input[i].ToString()) * counter;
                    counter *= 10;
                }
                catch
                {
                    counter = 1;
                    switch (input[i].ToString())
                    {
                        case "+": modifier += mod; break;
                        case "-": modifier -= mod; break;
                        default: await Context.Channel.SendMessageAsync("Incorrect sign"); return;
                    }
                    mod = 0;
                }
            }

            if (mod == 0) mod = 20;

            if (mod > 100 || modifier > 100)
            {
                await Context.Channel.SendMessageAsync(Text("big_roll").ToString()); return;
            }

            if (mod < 2 || modifier < -100)
            {
                await Context.Channel.SendMessageAsync(Text("small_roll").ToString()); return;
            }

            int random = 0;

            if(modifier >= 0)
            {
                if (modifier >= mod-1)
                    random = mod;
                else
                {
                    random += Config.random.Next(1 + modifier, mod / 2 + 1);
                    random += Config.random.Next(0, mod / 2 + 1);
                }
            }
            else
            {
                if (mod + modifier <= 1)
                    random = 1;
                else
                {
                    random = Config.random.Next(1, mod / 2 + modifier + 1);
                    random += Config.random.Next(0, mod / 2 + 1);
                }
            }

            EmbedBuilder builder = new EmbedBuilder();
            Color color;
            int z = mod / 4;
            if (random == 1) color = Color.Default;
            else if (random < z + z) color = Color.Red;
            else if (random <= z + z + z) color = Color.Green;
            else if (random < mod) color = Color.DarkGreen;
            else color = Color.Gold;

            builder
                .WithColor(color)
                .WithDescription(Context.User.Mention + Text("roll").ToString() + random.ToString() + "`")
                .WithThumbnailUrl("https://cdn.discordapp.com/attachments/603600328117583874/627468862153293824/422823435530403850.png")
                .WithCurrentTimestamp();

            if (random == 13) builder.WithThumbnailUrl("https://media.discordapp.net/attachments/614108079545647105/633423003161591808/IMG_20190831_190131.jpg?width=355&height=474");

            await Context.Channel.SendMessageAsync("", false, builder.Build());
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

        readonly string testhtml = @"
            <!DOCTYPE html>
            <html>
              <head>
                  <meta charset=utf-8>
                    <style>
                        @font-face{
                            font-family: TrajanPro;
                            src: url(https://cdn.discordapp.com/attachments/708001747842498623/708008151148003448/ofont.ru_Trajan.ttf) format(truetype);
                        }
                        h1.TrajanPro{
                            font-family: TrajanPro;
                            font-size:24pt
                        }
                        .TrajanPro{
                            font-family: TrajanPro;
                            font-size:10pt
                        }
                        p.TrajanPro{
                            font-family: TrajanPro;
                            font-size:10pt
                        }
                        img {
                                max-width: 100%;
                                max-height: 100%;
                        }
                        table{
                            border-collapse: collapse;
                            border: 1px solid grey;
                            background-color: black;
                        }
                        #imgstats { 
                                width:150px; 
                                height:350px; 
                                float:left; 
                                margin: 7px; 
                                padding: 10px;
                        }
                            #image { 
                                    width:150px; 
                                    height:150px; 
                                    padding: 10px;
                            }
                            #imageicon { 
                                    width:150px; 
                                    height:150px; 
                                    margin: -170px 0 0; 
                                    padding: 10px;
                            }
                            #special { 
                                    width:150px; 
                                    height:180px; 
                                    text-align:center;
                                    margin: 0 0 15px 0;
                                    padding: 10px;
                            }
                        #bio { 
                                width:500px;  
                                border: 30px;
                                -webkit-border-image:url(https://media.discordapp.net/attachments/708001747842498623/733980760020877352/master_ramka.png) 30 stretch stretch;
                                float:left; 
                                margin: 7px; 
                                text-align:center;
                        }
                        #inv { 
                                width:700px; 
                                border: 30px;
                                -webkit-border-image:url(https://media.discordapp.net/attachments/708001747842498623/733980760020877352/master_ramka.png) 30 stretch stretch;
                                float:left; 
                                margin: 7px;
                                text-align:center;
                        }
                        #head_block {
                            width:730;
                            height:60px; 
                        }
                            #nameblock {
                                display: inline-block;
                            }
                            #larrow {
                                margin:auto;
                                width:60px;
                                height:60px;
                                float:left;
                            }
                            #charname {
                                margin:auto;
                                height:40px;
                                display: inline-block;
                                line-height: 20px;
                                float:left;
                            }
                            #rarrow {
                                margin: auto;
                                width:60px;
                                height:60px;
                                float:left;
                            }
                    </style>
                </head>
                    
                <body bgcolor=#36393E><font color=#FFFFFF>  
                    <div id= head_block align=center>
                        <div id= nameblock>
                            <div id= larrow>
                                <img style= height:100% width:100% src= https://images.wikia.nocookie.net/hollowknight/ru/images/1/1c/%D0%A1%D1%82%D1%80%D0%B5%D0%BB%D0%BE%D1%87%D0%BA%D0%B02%D0%BB%D0%B5%D0%B2%D0%B0%D1%8F.png />
                            </div>
                            <div id= charname>                        
                                <h1 class=TrajanPro>
                                    Имя
                                </h1>     
                            </div>
                            <div id= rarrow>
                                <img style= height:100% width:100% src= https://images.wikia.nocookie.net/hollowknight/ru/images/8/82/%D0%A1%D1%82%D1%80%D0%B5%D0%BB%D0%BE%D1%87%D0%BA%D0%B02.png />
                            </div>
                        </div>
                    </div>

                    <div style= height:39px>
                        <img style= height:100% width:100% src= https://media.discordapp.net/attachments/708001747842498623/708659901383311427/Hr.png />
                    </div>                   

                    <div id = imgstats>
                        <div id = image align=center>
                            <img style= height:100% width:100% src= https://media.discordapp.net/attachments/567796677113676080/708007404830195814/20200507_202825.jpg  />
                        </div>
                        
                        <div id = imageicon>
                            <img style= height:100% width:100% src= https://cdn.discordapp.com/attachments/708001747842498623/708008561543741602/Jornal.png />
                        </div>

                        <div id= special> 
                            <table class= TrajanPro border= 2 width= 100%>
                                <tr><td>Интеллект</td><td>1</td></tr>
                                <tr><td>Харизма</td><td>2</td></tr>
                                <tr><td>Ловкость</td><td>3</td></tr>
                                <tr><td>Магия</td><td>4</td></tr>
                                <tr><td>Сила</td><td>5</td></tr>
                                <tr><td>Выносливость</td><td>6</td></tr>
                                <tr><td>Естество</td><td>7</td></tr>
                             </table>
                         </div> 
                    </div>

                    <div class= TrajanPro id= bio> 
                        <div><strong>Биография</strong></div>
                        <img src= https://vignette.wikia.nocookie.net/hollowknight/images/9/92/Spacer.png/revision/latest/scale-to-width-down/321?cb=20190126033524 />
                        <p style=text-align:left>
            
                        </p>
                    </div> 
                    
                    <div class= TrajanPro id= inv>
                        <div><strong>Черты</strong></div>                    
                            <img src= https://vignette.wikia.nocookie.net/hollowknight/images/9/92/Spacer.png/revision/latest/scale-to-width-down/321?cb=20190126033524 />               
                            <p style=text-align:left>                    
                    
                        </p>
                    </div>

                    <div class= TrajanPro id= inv>
                        <div><strong>Инвентарь</strong></div>
                        <img src= https://vignette.wikia.nocookie.net/hollowknight/images/9/92/Spacer.png/revision/latest/scale-to-width-down/321?cb=20190126033524 />
                        <p style=text-align:left>

                        </p>
                    </div>
                </body>
            </html>";


        readonly string testhtml1 = @"
            <!DOCTYPE html>
            <html>
              <head>
                  <meta charset=utf-8>
                    <style>
                        @font-face{
                            font-family: TrajanPro;
                            src: url(https://cdn.discordapp.com/attachments/708001747842498623/708008151148003448/ofont.ru_Trajan.ttf) format(truetype);
                        }
                        h1.TrajanPro{
                            font-family: TrajanPro;
                            font-size:24pt
                        }
                        .TrajanPro{
                            font-family: TrajanPro;
                            font-size:10pt
                        }
                        p.TrajanPro{
                            font-family: TrajanPro;
                            font-size:10pt
                        }
                        img {
                                max-width: 100%;
                                max-height: 100%;
                        }
                        #skills { 
                                width:740px; 
                                border: 30px;
                                -webkit-border-image:url(https://media.discordapp.net/attachments/708001747842498623/733980760020877352/master_ramka.png) 30 stretch stretch;
                                float:left; 
                                margin: 7px;
                                text-align:center;
                        }
                        #head_block {
                            width:730;
                            height:60px; 
                        }
                            #nameblock {
                                display: inline-block;
                            }
                            #larrow {
                                margin:auto;
                                width:60px;
                                height:60px;
                                float:left;
                            }
                            #charname {
                                margin:auto;
                                height:40px;
                                display: inline-block;
                                line-height: 20px;
                                float:left;
                            }
                            #rarrow {
                                margin: auto;
                                width:60px;
                                height:60px;
                                float:left;
                            }
                    </style>
                </head>
                    
                <body bgcolor=#36393E><font color=#FFFFFF>  
                    <div id= head_block align=center>
                        <div id= nameblock>
                            <div id= larrow>
                                <img style= height:100% width:100% src= https://images.wikia.nocookie.net/hollowknight/ru/images/1/1c/%D0%A1%D1%82%D1%80%D0%B5%D0%BB%D0%BE%D1%87%D0%BA%D0%B02%D0%BB%D0%B5%D0%B2%D0%B0%D1%8F.png />
                            </div>
                            <div id= charname>                        
                                <h1 class=TrajanPro>
                                    Имя
                                </h1>     
                            </div>
                            <div id= rarrow>
                                <img style= height:100% width:100% src= https://images.wikia.nocookie.net/hollowknight/ru/images/8/82/%D0%A1%D1%82%D1%80%D0%B5%D0%BB%D0%BE%D1%87%D0%BA%D0%B02.png />
                            </div>
                        </div>
                    </div>

                    <div style= height:39px>
                        <img style= height:100% width:100% src= https://media.discordapp.net/attachments/708001747842498623/708659901383311427/Hr.png />
                    </div>

                    <div class= TrajanPro id= skills> 
                        <div><strong>Навыки Интеллекта</strong></div>
                        <img src= https://vignette.wikia.nocookie.net/hollowknight/images/9/92/Spacer.png/revision/latest/scale-to-width-down/321?cb=20190126033524 />
                        <p style=text-align:left>

                        </p>
                    </div>

                    <div class= TrajanPro id= skills> 
                        <div><strong>Заклинания</strong></div>
                        <img src= https://vignette.wikia.nocookie.net/hollowknight/images/9/92/Spacer.png/revision/latest/scale-to-width-down/321?cb=20190126033524 />
                        <p style=text-align:left>

                        </p>
                    </div> 

                    <div class= TrajanPro id= skills> 
                        <div><strong>Навыки Естества</strong></div>
                        <img src= https://vignette.wikia.nocookie.net/hollowknight/images/9/92/Spacer.png/revision/latest/scale-to-width-down/321?cb=20190126033524 />
                        <p style=text-align:left>

                        </p>
                    </div>                     
                </body>
            </html>";

        [Command("test")]
        public async Task Test()
        {
            var converter = new HtmlConverter();
            var bytes = converter.FromHtmlString(testhtml1, 770, ImageFormat.Png, 100);
            System.IO.File.WriteAllBytes($"{Config.configPath + "/" + Context.Guild.Id.ToString() + "/" + CommandTools.Translit("Тест")}.png", bytes);

            await Context.Channel.SendFileAsync($"{Config.configPath + "/" + Context.Guild.Id.ToString() + "/" + CommandTools.Translit("Тест")}.png");
        }

        [Command("batya")]
        public async Task SendBatya()
        {
            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                .WithColor(Color.DarkGreen)
                .WithImageUrl("https://cdn.discordapp.com/attachments/557592875735580702/581237030663618570/IMG_20190502_170310.jpg").Build());
        }

        [Command("local")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Hook([Remainder]string Text)
        {
            await Context.Message.DeleteAsync();
            var webhook = await (Context.Channel as SocketTextChannel).CreateWebhookAsync(Context.Guild.Name);
            using var webhookClient = new DiscordWebhookClient(webhook);
            await webhookClient.SendMessageAsync(Text, false, null, webhook.Name, Context.Guild.IconUrl);
            await webhookClient.DeleteWebhookAsync();
        }
    }
}
