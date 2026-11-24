using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using cherepeni_forms.Classes;
using MySql.Data.MySqlClient;

namespace cherepeni_forms.Forms.Sis_admin
{
    public partial class Special : Form
    {

        // Список разрешённых таблиц
        private readonly string[] tables =
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
        public Special()
        {
            InitializeComponent();

            // Подключение обработчиков кнопок
            // Если они уже назначены в Designer, эти строки уберите
            button2.Click += button2_Click;
            button3.Click += button3_Click;
            button4.Click += button4_Click;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }
        // =========================================
        // CSV ЭКСПОРТ
        // =========================================

        private void ExportTable(string tableName, string folder)
        {
            string connectionString =
                DBconfig.BuildConnectionString();

            using (MySqlConnection conn =
                new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = $"SELECT * FROM `{tableName}`";

                using (MySqlDataAdapter adapter =
                    new MySqlDataAdapter(query, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    string filePath = Path.Combine(
                        folder, tableName + ".csv");

                    using (StreamWriter writer =
                        new StreamWriter(
                            filePath,
                            false,
                            new UTF8Encoding(true)))
                    {
                        // Запись заголовков
                        writer.WriteLine(string.Join(";",
                            dt.Columns.Cast<DataColumn>()
                            .Select(c => CsvEscape(c.ColumnName))));

                        // Запись данных
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

        // Экранирование CSV
        private string CsvEscape(string value)
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

        // Кнопка button4 - Экспорт
        private void button4_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog =
                new FolderBrowserDialog())
            {
                dialog.Description =
                    "Выберите папку для экспорта";

                if (dialog.ShowDialog() ==
                    DialogResult.OK)
                {
                    try
                    {
                        foreach (string table in tables)
                        {
                            ExportTable(
                                table,
                                dialog.SelectedPath);
                        }

                        MessageBox.Show(
                            "Экспорт всех таблиц выполнен успешно!",
                            "Экспорт",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show(
                            "Ошибка БД при экспорте:\n\n" +
                            ex.Message,
                            "Ошибка БД",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "Ошибка при экспорте:\n\n" +
                            ex.Message,
                            "Ошибка",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        // =========================================
        // CSV ИМПОРТ
        // =========================================

        // Чтение CSV-файла с определением кодировки и разделителя
        private List<string[]> ReadCsv(string filePath)
        {
            List<string[]> result = new List<string[]>();

            byte[] bytes = File.ReadAllBytes(filePath);
            string content;

            // Определение кодировки
            if (bytes.Length >= 3 &&
                bytes[0] == 0xEF &&
                bytes[1] == 0xBB &&
                bytes[2] == 0xBF)
            {
                // UTF-8 с BOM
                content = Encoding.UTF8.GetString(bytes, 3,
                    bytes.Length - 3);
            }
            else if (bytes.Length >= 2 &&
                bytes[0] == 0xFF &&
                bytes[1] == 0xFE)
            {
                // UTF-16 LE
                content = Encoding.Unicode.GetString(bytes, 2,
                    bytes.Length - 2);
            }
            else if (bytes.Length >= 2 &&
                bytes[0] == 0xFE &&
                bytes[1] == 0xFF)
            {
                // UTF-16 BE
                content = Encoding.BigEndianUnicode.GetString(bytes, 2,
                    bytes.Length - 2);
            }
            else
            {
                // UTF-8 без BOM или Windows-1251
                try
                {
                    // Строгое чтение UTF-8 без замены ошибочных символов
                    content = new UTF8Encoding(false, true)
                        .GetString(bytes);
                }
                catch (DecoderFallbackException)
                {
                    // Если UTF-8 не подходит, читаем Windows-1251
                    content = Encoding.GetEncoding(1251)
                        .GetString(bytes);
                }
            }

            if (string.IsNullOrWhiteSpace(content))
                return result;

            // Определение разделителя
            string firstLine = content.Split(
                new char[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries)[0];

            char delimiter =
                firstLine.Count(c => c == ';') >=
                firstLine.Count(c => c == ',')
                ? ';'
                : ',';

            List<string> row = new List<string>();
            StringBuilder field = new StringBuilder();

            bool inQuotes = false;

            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];

                if (c == '"')
                {
                    if (inQuotes &&
                        i + 1 < content.Length &&
                        content[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    row.Add(field.ToString());
                    field.Clear();
                }
                else if ((c == '\r' || c == '\n') && !inQuotes)
                {
                    row.Add(field.ToString());
                    field.Clear();

                    if (row.Any(x => x.Length > 0))
                        result.Add(row.ToArray());

                    row.Clear();

                    if (c == '\r' &&
                        i + 1 < content.Length &&
                        content[i + 1] == '\n')
                    {
                        i++;
                    }
                }
                else
                {
                    field.Append(c);
                }
            }

            // Последняя строка
            if (field.Length > 0 || row.Count > 0)
            {
                row.Add(field.ToString());

                if (row.Any(x => x.Length > 0))
                    result.Add(row.ToArray());
            }

            return result;
        }

        // Импорт данных в таблицу
        private void ImportTable(
            string tableName,
            string filePath)
        {
            List<string[]> csv =
                ReadCsv(filePath);

            if (csv.Count < 2)
            {
                throw new Exception(
                    "CSV-файл пуст или содержит только заголовок.");
            }

            string[] headers = csv[0];

            if (headers.Length == 0)
            {
                throw new Exception(
                    "Не найдены столбцы CSV.");
            }

            // Проверка повторяющихся заголовков
            if (headers.Distinct(
                StringComparer.OrdinalIgnoreCase).Count()
                != headers.Length)
            {
                throw new Exception(
                    "CSV содержит повторяющиеся названия столбцов.");
            }

            string connectionString =
                DBconfig.BuildConnectionString();

            using (MySqlConnection conn =
                new MySqlConnection(connectionString))
            {
                conn.Open();

                // Проверка столбцов до транзакции
                DataTable schema = new DataTable();

                using (MySqlDataAdapter adapter =
                    new MySqlDataAdapter(
                        $"SELECT * FROM `{tableName}` LIMIT 0",
                        conn))
                {
                    adapter.Fill(schema);
                }

                foreach (string header in headers)
                {
                    if (!schema.Columns.Contains(header))
                    {
                        throw new Exception(
                            "Столбец '" + header +
                            "' отсутствует в таблице " +
                            tableName + ".");
                    }
                }

                using (MySqlTransaction transaction =
                    conn.BeginTransaction())
                {
                    try
                    {
                        string columns = string.Join(",",
                            headers.Select(h =>
                                "`" + h + "`"));

                        string parameters = string.Join(",",
                            headers.Select((h, i) =>
                                "@p" + i));

                        string query =
                        $"INSERT IGNORE INTO `{tableName}` " +
                        $"({columns}) VALUES ({parameters})";

                        using (MySqlCommand cmd =
                            new MySqlCommand(
                                query,
                                conn,
                                transaction))
                        {
                            // Создание параметров
                            for (int i = 0;
                                i < headers.Length;
                                i++)
                            {
                                cmd.Parameters.Add(
                                    "@p" + i,
                                    MySqlDbType.VarChar);
                            }

                            // Импорт строк
                            for (int r = 1;
                                r < csv.Count;
                                r++)
                            {
                                if (csv[r].Length !=
                                    headers.Length)
                                {
                                    throw new Exception(
                                        "Ошибка в строке " +
                                        (r + 1) +
                                        ": количество столбцов " +
                                        "не совпадает.");
                                }

                                for (int i = 0;
                                    i < headers.Length;
                                    i++)
                                {
                                    cmd.Parameters[
                                        "@p" + i].Value =
                                        csv[r][i] == ""
                                        ? DBNull.Value
                                        : (object)csv[r][i];
                                }

                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Кнопка button2 - Импорт
        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog =
                new OpenFileDialog())
            {
                dialog.Title =
                    "Выберите CSV-файл для импорта";

                dialog.Filter =
                    "CSV-файлы (*.csv)|*.csv";

                if (dialog.ShowDialog() ==
                    DialogResult.OK)
                {
                    try
                    {
                        string tableName =
                            Path.GetFileNameWithoutExtension(
                                dialog.FileName);

                        // Проверка имени таблицы
                        if (!tables.Contains(tableName))
                        {
                            MessageBox.Show(
                                "Имя файла должно совпадать " +
                                "с названием таблицы.\n\n" +
                                "Например: Products.csv",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                            return;
                        }

                        ImportTable(
                            tableName,
                            dialog.FileName);

                        MessageBox.Show(
                            "Импорт таблицы " +
                            tableName +
                            " выполнен успешно!",
                            "Импорт",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show(
                            "Ошибка БД при импорте:\n\n" +
                            ex.Message,
                            "Ошибка БД",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "Ошибка при импорте:\n\n" +
                            ex.Message,
                            "Ошибка",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        // =========================================
        // РЕЗЕРВНОЕ КОПИРОВАНИЕ
        // =========================================

        // Кнопка button3 - Резервное копирование
        private void button3_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog =
                new FolderBrowserDialog())
            {
                dialog.Description =
                    "Выберите место для резервной копии";

                if (dialog.ShowDialog() ==
                    DialogResult.OK)
                {
                    try
                    {
                        string backupFolder =
                            Path.Combine(
                                dialog.SelectedPath,
                                "Backup_ShopDB_" +
                                DateTime.Now.ToString(
                                    "yyyyMMdd_HHmmss"));

                        Directory.CreateDirectory(
                            backupFolder);

                        foreach (string table in tables)
                        {
                            ExportTable(
                                table,
                                backupFolder);
                        }

                        MessageBox.Show(
                            "Резервная копия создана успешно!\n\n" +
                            backupFolder,
                            "Резервное копирование",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show(
                            "Ошибка БД при резервном копировании:\n\n" +
                            ex.Message,
                            "Ошибка БД",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "Ошибка резервного копирования:\n\n" +
                            ex.Message,
                            "Ошибка",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
