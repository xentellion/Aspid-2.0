namespace Aspid.Command
{
    internal class Fields
    {
        internal static string part1 = @"
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
                                <h1 class=TrajanPro>";
        //Name
        internal static string part2 = @"</h1>     
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
                            <img style= height:100% width:100% src =";


        //https://media.discordapp.net/attachments/567796677113676080/708007404830195814/20200507_202825.jpg 


        internal static string part3 = @" />
                        </div>
                        
                        <div id = imageicon>
                            <img style= height:100% width:100% src= https://cdn.discordapp.com/attachments/708001747842498623/708008561543741602/Jornal.png />
                        </div>

                        <div id= special> 
                            <table class= TrajanPro border= 2 width= 100%>";
        internal static string SetLevels(string input)
        {
            string[] a = new string[7];
            for (int i = 0; i < a.Length; i++)
            {
                if (input[i] != '0')
                    a[i] = input[i].ToString();
                else
                    a[i] = "10";
            }

            return $"<tr><td>Интеллект</td><td>{ a[0] }</td></tr>" +
                    $"<tr><td>Харизма</td><td>{ a[1] }</td></tr>" +
                    $"<tr><td>Ловкость</td><td>{ a[2] }</td></tr>" +
                    $"<tr><td>Магия</td><td>{ a[3] }</td></tr>" +
                    $"<tr><td>Сила</td><td>{ a[4] }</td></tr>" +
                    $"<tr><td>Выносливость</td><td>{ a[5] }</td></tr>" +
                    $"<tr><td>Естество</td><td>{ a[6] }</td></tr>";
        }

        internal static string part5 = @"</table>
                        </div> 
                    </div>

                    <div class= TrajanPro id= bio> 
                        <div><strong>Биография</strong></div>
                        <img src= https://vignette.wikia.nocookie.net/hollowknight/images/9/92/Spacer.png/revision/latest/scale-to-width-down/321?cb=20190126033524 />
                        <p style=text-align:left>";
        //Biography
        internal static string part6 = @"</p>
                    </div> 
                    
                    <div class= TrajanPro id= inv>
                        <div><strong>Черты</strong></div>                    
                            <img src= https://vignette.wikia.nocookie.net/hollowknight/images/9/92/Spacer.png/revision/latest/scale-to-width-down/321?cb=20190126033524 />               
                            <p style=text-align:left>";
        //Quirks
        internal static string part7 = @"</p>
                    </div>

                    <div class= TrajanPro id= inv>
                        <div><strong>Инвентарь</strong></div>
                        <img src= https://vignette.wikia.nocookie.net/hollowknight/images/9/92/Spacer.png/revision/latest/scale-to-width-down/321?cb=20190126033524 />
                        <p style=text-align:left>>";
        //Inventory
        internal static string part8 = @"
                </p>
                    </div> 
                </body>
            </html>";

        internal static string part2_bis = @"</h1>     
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
                        <p style=text-align:left>";

        internal static string part3_bis = @"</p>
                    </div>

                    <div class= TrajanPro id= skills> 
                        <div><strong>Заклинания</strong></div>
                        <img src= https://vignette.wikia.nocookie.net/hollowknight/images/9/92/Spacer.png/revision/latest/scale-to-width-down/321?cb=20190126033524 />
                        <p style=text-align:left>";

        internal static string part4_bis = @"</p>
                    </div>

                    <div class= TrajanPro id= skills> 
                        <div><strong>Навыки Естества</strong></div>
                        <img src= https://vignette.wikia.nocookie.net/hollowknight/images/9/92/Spacer.png/revision/latest/scale-to-width-down/321?cb=20190126033524 />
                        <p style=text-align:left>";
    }
}