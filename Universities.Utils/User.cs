namespace Universities.Utils
{
    public static class User
    {
        public static int Id { get; set; } = 0;
        public static string Username { get; set; } = string.Empty;
        public static string Role { get; set; } = string.Empty;
        public static string LastMainOrg { get; set; } = string.Empty;
        public static int LastDocId { get; set; } = -1;

        public static bool CheckUser(string user)
        {
            if (user.StartsWith("Error")) return PromptBox.Error(user);
            Id = int.Parse(user.Split(";")[0]);
            Username = user.Split(";")[1];
            Role = user.Split(";")[3];
            LastMainOrg = user.Split(";")[4];
            LastDocId = int.Parse(user.Split(";")[5]);
            return true;
        }

        public static string[] ToArray()
        {
            return new string[]
            {
                $"{Id}",
                Username,
                Role,
                LastMainOrg,
                $"{LastDocId}"
            };
        }
    }
}