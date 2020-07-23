namespace Aspid
{
    class Queries
    {
        public static string CreateTable(ulong id)
        {
            return
                $"CREATE TABLE GUILD_{id} (U_ID BIGINT NOT NULL PRIMARY KEY, MUTETIME INT DEFAULT 0, ISPUNISHED BOOL DEFAULT FALSE); " +

                $"CREATE TABLE RP_{id} (CHAR_NAME TEXT NOT NULL UNIQUE, CHAR_OWNER BIGINT NOT NULL REFERENCES GUILD_{id} (U_ID) ON UPDATE CASCADE, CHAR_IMAGE TEXT, CHAR_LEVEL TEXT, CHAR_BIO TEXT, CHAR_INV TEXT, CHAR_INT TEXT, CHAR_MAG TEXT, CHAR_NAT TEXT, CHAR_GALLERY TEXT, CHAR_NICKNAME TEXT)";
        }

        public static string CreateTableList()
        {
            return $"CREATE TABLE GUILDS (ID BIGINT PRIMARY KEY NOT NULL UNIQUE, DEAD BIGINT, PUNISHED BIGINT, MUTED BIGINT)";
        }

        public static string AddGuild(ulong id, ulong dead = 0, ulong punished = 0, ulong muted = 0)
        {
            return $"INSERT INTO GUILDS (ID, DEAD, PUNISHED, MUTED) VALUES({id},{dead},{punished},{muted})";
        }

        public static string UpdateRole(ulong id, ulong dead = 0, ulong punished = 0, ulong muted = 0)
        {
            return $"UPDATE GUILDS SET DEAD =  {dead}, PUNISHED = {punished}, MUTED = {muted} WHERE ID = {id}";
        }
        public static string GetRoles(ulong guild)
        {
            return $"SELECT * FROM GUILDS WHERE ID={guild}";
        }

        #region Users

        public static string AddUser(ulong guild, ulong id)
        {
            return $"INSERT INTO GUILD_{guild} (U_ID) VALUES({id});";
        }

        public static string GetUser(ulong guild, ulong id)
        {
            return $"SELECT * FROM GUILD_{guild} WHERE U_ID={id};";
        }
        #endregion

        #region Mute

        public static string GetMuted(ulong guild)
        {
            return $"SELECT U_ID FROM GUILD_{guild} WHERE MUTETIME <= 0;";
        }

        public static string RemoveMute(ulong guild, ulong id)
        {
            return $"UPDATE GUILD_{guild} SET MUTETIME = 0 WHERE U_ID = {id};";
        }

        public static string AddMute(ulong guild, ulong id, ulong mutetime)
        {
            return $"UPDATE GUILD_{guild} SET MUTETIME = {mutetime} WHERE U_ID = {id};";
        }
        #endregion

        #region Punish

        public static string RemovePunish(ulong guild, ulong id)
        {
            return $"UPDATE GUILD_{guild} SET ISPUNISHED = FALSE WHERE U_ID = {id};";
        }

        public static string AddPunish(ulong guild, ulong id)
        {
            return $"UPDATE GUILD_{guild} SET ISPUNISHED = TRUE WHERE U_ID = {id};";
        }

        #endregion

        #region Roleplay

        public static string AddChar(ulong guild, string name, ulong dude)
        {
            return $"INSERT INTO RP_{guild} (CHAR_NAME, CHAR_OWNER, CHAR_IMAGE, CHAR_LEVEL, CHAR_BIO, CHAR_INV, CHAR_NICKNAME) VALUES('{name}', {dude}, '{"https://media.discordapp.net/attachments/708001747842498623/708762818719252480/111.png?width=475&height=475"}', '0000000_0', 'Описание | Черта', 'Инвентарь', '{name}');";
        }

        public static string ChangeNickname(ulong guild, string name, string description)
        {
            return $"UPDATE RP_{guild} SET CHAR_NICKNAME = '{description}' WHERE CHAR_NAME = '{name}'";
        }

        public static string ChangeImage(ulong guild, string name, string description)
        {
            return $"UPDATE RP_{guild} SET CHAR_IMAGE = '{description}' WHERE CHAR_NAME = '{name}'";
        }

        public static string DeleteChar(ulong guild, string name)
        {
            return $"DELETE FROM RP_{guild} WHERE CHAR_NAME = '{name}'";
        }

        public static string GetCharacter(ulong guild, string name)
        {
            return $"SELECT * FROM RP_{guild} WHERE CHAR_NAME = '{name}'";
        }

        public static string GetCharacter(ulong guild, ulong id)
        {
            return $"SELECT * FROM RP_{guild} WHERE CHAR_OWNER = {id}";
        }

        public static string GetAllCharacters(ulong guild)
        {
            return $"SELECT * FROM RP_{guild}";
        }
        #endregion

        internal static string UpdateInt(ulong id, string name, string info)
        {
            return $"UPDATE RP_{id} SET CHAR_INT = '{info}' WHERE CHAR_NAME = '{name}'";
        }

        internal static string UpdateMag(ulong id, string name, string info)
        {
            return $"UPDATE RP_{id} SET CHAR_MAG = '{info}' WHERE CHAR_NAME = '{name}'";
        }

        internal static string UpdateNat(ulong id, string name, string info)
        {
            return $"UPDATE RP_{id} SET CHAR_NAT = '{info}' WHERE CHAR_NAME = '{name}'";
        }

        internal static string UpdateBio(ulong id, string name, string info)
        {
            return $"UPDATE RP_{id} SET CHAR_BIO = '{info}' WHERE CHAR_NAME = '{name}'";
        }

        internal static string UpdateInv(ulong id, string name, string info)
        {
            return $"UPDATE RP_{id} SET CHAR_INV = '{info}' WHERE CHAR_NAME = '{name}'";
        }

        internal static string UpdateLevel(ulong id, string name, string info)
        {
            return $"UPDATE RP_{id} SET CHAR_LEVEL = '{info}' WHERE CHAR_NAME = '{name}'";
        }

        internal static string GetImage(ulong id, string name)
        {
            return $"SELECT CHAR_OWNER, CHAR_GALLERY FROM RP_{ id } WHERE CHAR_NAME = '{name}'";
        }

        internal static string AddImage(ulong id, string name, string info)
        {
            return $"UPDATE RP_{id} SET CHAR_GALLERY = '{info}' WHERE CHAR_NAME = '{name}'";
        }
    }
}
