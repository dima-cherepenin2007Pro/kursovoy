using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cherepeni_forms.Properties;
using MySql.Data.MySqlClient;

namespace cherepeni_forms.Classes
{
    public static class DBconfig
    {
        /// <summary>
        /// Формирует строку подключения из сохранённых настроек.
        /// </summary>
        public static string BuildConnectionString()
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = Settings.Default.DbHost,
                Port = uint.TryParse(Settings.Default.DbPort, out var p) ? p : 3306,
                UserID = Settings.Default.DbUser,
                Password = Settings.Default.DbPassword,
                Database = Settings.Default.DbName,
                CharacterSet = "utf8mb4"
            };
            return builder.ConnectionString;
        }

        /// <summary>
        /// Пытается открыть соединение. Возвращает null при успехе,
        /// иначе — текст ошибки.
        /// </summary>
        public static string TestConnection(out string connectionString)
        {
            connectionString = BuildConnectionString();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                }
                return null; // ошибки нет
            }
            catch (MySqlException ex)
            {
                return ex.Message;
            }
        }
    }
}
