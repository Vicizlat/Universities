using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;

namespace Universities.Utils
{
    public static class SqlCommands
    {
        public static Tuple<string, bool> CurrentUser { get; set; } = new Tuple<string, bool>(GetCurrentUser(out bool isAdmin), isAdmin);

        public static string GetConnectionString()
        {
            return $"Server={Settings.Instance.Server};Port={Settings.Instance.Port};User={Settings.Instance.Username};Password={Settings.Instance.Password};";
        }

        public static string GetCurrentUser(out bool isAdmin)
        {
            isAdmin = false;
            try
            {
                string currentUser = string.Empty;
                using (MySqlConnection Connection = new MySqlConnection(GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand("SHOW GRANTS;", Connection))
                {
                    Connection.Open();
                    MySqlDataReader reader = Command.ExecuteReader();
                    while (reader.Read())
                    {
                        string value = reader.GetString(0);
                        currentUser = value.Substring(value.IndexOf("TO '") + 4, value.IndexOf("'@") - value.IndexOf("TO '") - 4);
                        if (value.StartsWith("GRANT ALL"))
                        {
                            isAdmin = true;
                            break;
                        }
                    }
                }
                return currentUser;
            }
            catch
            {
                return "Not connected to DB";
            }
        }

        public static void AddUser(string username, string password, bool isAdmin)
        {
            if (CurrentUser.Item2)
            {
                string command = $"CREATE USER '{username}' IDENTIFIED BY '{password}';";
                if (isAdmin) command += $"GRANT ALL ON *.* TO '{username}';";
                else command += $"GRANT SELECT, INSERT, UPDATE ON *.* TO '{username}';";
                using (MySqlConnection Connection = new MySqlConnection(GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand(command, Connection))
                {
                    Connection.Open();
                    Command.ExecuteNonQuery();
                }
                PromptBox.Information($"User '{username}' added successfully!", CurrentUser.Item1);
                ChangeUserPrivileges(username, isAdmin);
            }
            else PromptBox.Information("You don't have permission to add users!");
        }

        public static void ChangeUserPrivileges(string username, bool isAdmin)
        {
            if (CurrentUser.Item1 != username && CurrentUser.Item2)
            {
                string command = $"REVOKE ALL ON *.* FROM '{username}'@'%';FLUSH PRIVILEGES;";
                command += isAdmin ? $"GRANT ALL ON *.* TO '{username}'@'%';" : $"GRANT SELECT, INSERT, UPDATE ON *.* TO '{username}'@'%';";
                using (MySqlConnection Connection = new MySqlConnection(GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand(command, Connection))
                {
                    Connection.Open();
                    Command.ExecuteNonQuery();
                }
                PromptBox.Information($"User '{username}' priviliges changed successfully!", CurrentUser.Item1);
            }
            else PromptBox.Error("You don't have permission to change user privileges or you are trying to change your own priviliges!");
        }

        public static void ChangeUserPassword(string username, string password)
        {
            try
            {
                if (CurrentUser.Item2 || CurrentUser.Item1 == username)
                {
                    string command = $"SET PASSWORD FOR '{username}'@'%' = PASSWORD('{password}');";
                    using (MySqlConnection Connection = new MySqlConnection(GetConnectionString()))
                    using (MySqlCommand Command = new MySqlCommand(command, Connection))
                    {
                        Connection.Open();
                        Command.ExecuteNonQuery();
                    }
                    PromptBox.Information($"Password for User '{username}' changed successfully!", CurrentUser.Item1);
                }
                else PromptBox.Error($"Unable to change password for User '{username}'!");
            }
            catch
            {
                PromptBox.Error("Can't change password when you are not logged in.");
            }
        }

        public static void RemoveUser(string username)
        {
            if (CurrentUser.Item2)
            {
                string command = $"DROP USER '{username}'@'%';FLUSH PRIVILEGES;";
                using (MySqlConnection Connection = new MySqlConnection(GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand(command, Connection))
                {
                    Connection.Open();
                    Command.ExecuteNonQuery();
                }
                PromptBox.Information($"User '{username}' removed successfully!", CurrentUser.Item1);
            }
            else PromptBox.Error("You don't have permission to remove user!");
        }

        public static void AddDatabase(string database)
        {
            if (CurrentUser.Item2)
            {
                string command = $"CREATE DATABASE `{database}` COLLATE 'utf8_general_ci';";
                using (MySqlConnection Connection = new MySqlConnection(GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand(command, Connection))
                {
                    Connection.Open();
                    Command.ExecuteNonQuery();
                }
                PromptBox.Information($"Database '{database}' added successfully!", CurrentUser.Item1);
            }
            else PromptBox.Information("You don't have permission to add databases!");
        }

        public static List<Tuple<string, bool>> GetUsers()
        {
            try
            {
                List<Tuple<string, bool>> users = new List<Tuple<string, bool>>();
                using (MySqlConnection Connection = new MySqlConnection(GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand("SELECT * FROM mysql.user;", Connection))
                {
                    Connection.Open();
                    MySqlDataReader reader = Command.ExecuteReader();
                    while (reader.Read())
                    {
                        string user = (string)reader.GetValue("User");
                        bool isAdmin = (string)reader.GetValue("Grant_priv") == "Y";
                        if (user == "root") continue;
                        users.Add(new Tuple<string, bool>(user, isAdmin));
                    }
                }
                return users;
            }
            catch
            {
                return new List<Tuple<string, bool>>() { new Tuple<string, bool>("Unable to get Users", false) };
            }
        }

        public static List<string> GetDatabases()
        {
            try
            {
                List<string> databases = new List<string>();
                using (MySqlConnection Connection = new MySqlConnection(GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand("SHOW SCHEMAS", Connection))
                {
                    Connection.Open();
                    MySqlDataReader reader = Command.ExecuteReader();
                    while (reader.Read())
                    {
                        string database = ((string)reader.GetValue("Database")).ToLower();
                        if (database.Contains("mysql") || database.Contains("schema")) continue;
                        databases.Add(database);
                    }
                }
                return databases;
            }
            catch
            {
                return new List<string>() { "Unable to get Databases" };
            }
        }
    }
}