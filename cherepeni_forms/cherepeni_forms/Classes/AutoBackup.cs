using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using cherepeni_forms.Classes;

namespace cherepeni_forms.Services
{
    public static class AutoBackup
    {
        private static readonly string[] tables =
        {
            "Users",
            "Roles",
            "Products",
            "Categories",
            "Customers",
            "Orders",
            "OrderItems",
            "Statuses"
        };

        public static void CreateBackup()
        {
            string backupRoot = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                "ShopDB_Backups");

            Directory.CreateDirectory(backupRoot);

            string backupFolder = Path.Combine(
                backupRoot,
                "Backup_" + DateTime.Now.ToString(
                    "yyyyMMdd_HHmmss"));

            Directory.CreateDirectory(backupFolder);

            string connectionString =
                DBconfig.BuildConnectionString();

            using (MySqlConnection conn =
                new MySqlConnection(connectionString))
            {
                conn.Open();

                foreach (string tableName in tables)
                {
                    string query =
                        $"SELECT * FROM `{tableName}`";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        string filePath = Path.Combine(
                            backupFolder,
                            tableName + ".csv");

                        using (StreamWriter writer =
                            new StreamWriter(
                                filePath,
                                false,
                                new UTF8Encoding(true)))
                        {
                            // Заголовки
                            writer.WriteLine(string.Join(";",
                                dt.Columns.Cast<DataColumn>()
                                .Select(c => CsvEscape(
                                    c.ColumnName))));

                            // Данные
                            foreach (DataRow row in dt.Rows)
                            {
                                string line = string.Join(";",
                                    row.ItemArray.Select(x =>
                                        CsvEscape(
                                            x == DBNull.Value
                                            ? ""
                                            : x.ToString())));

                                writer.WriteLine(line);
                            }
                        }
                    }
                }
            }
        }

        private static string CsvEscape(string value)
        {
            if (value.Contains(";") ||
                value.Contains("\"") ||
                value.Contains("\n") ||
                value.Contains("\r"))
            {
                value = "\"" +
                    value.Replace("\"", "\"\"") +
                    "\"";
            }

            return value;
        }
    }
}