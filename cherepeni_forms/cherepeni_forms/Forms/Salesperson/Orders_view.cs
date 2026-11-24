using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using cherepeni_forms.Classes;
using MySql.Data.MySqlClient;
using Word = Microsoft.Office.Interop.Word;

namespace cherepeni_forms.Forms.Salesperson
{
    public partial class Orders_view : Form
    {
        private DataTable ordersTable;

        public Orders_view()
        {
            InitializeComponent();

            ConfigureDataGridView();
            LoadOrders();

            // Двойной щелчок по заказу
            dataGridView1.CellDoubleClick +=
                dataGridView1_CellDoubleClick;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        // =========================================================
        // НАСТРОЙКА DATAGRIDVIEW
        // =========================================================

        private void ConfigureDataGridView()
        {
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;

            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;

            dataGridView1.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dataGridView1.MultiSelect = false;

            dataGridView1.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView1.RowHeadersVisible = false;
        }

        // =========================================================
        // ЗАГРУЗКА ЗАКАЗОВ
        // =========================================================

        private void LoadOrders()
        {
            try
            {
                string connectionString =
                    DBconfig.BuildConnectionString();

                using (MySqlConnection connection =
                    new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT
                            o.OrderID,
                            o.OrderDate,

                            CONCAT(
                                c.LastName, ' ',
                                c.FirstName, ' ',
                                COALESCE(c.MiddleName, '')
                            ) AS CustomerName,

                            COALESCE(SUM(
                                oi.Quantity * oi.PriceAtPurchase
                            ), 0) AS OrderTotal,

                            s.StatusName

                        FROM Orders o

                        INNER JOIN Customers c
                            ON o.CustomerID = c.CustomerID

                        INNER JOIN Statuses s
                            ON o.StatusID = s.StatusID

                        LEFT JOIN OrderItems oi
                            ON o.OrderID = oi.OrderID

                        GROUP BY
                            o.OrderID,
                            o.OrderDate,
                            c.LastName,
                            c.FirstName,
                            c.MiddleName,
                            s.StatusName

                        ORDER BY o.OrderID DESC;
                    ";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, connection))
                    {
                        ordersTable = new DataTable();

                        adapter.Fill(ordersTable);

                        dataGridView1.DataSource = ordersTable;
                    }
                }

                ConfigureColumnNames();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Ошибка при загрузке заказов:\n\n" +
                    ex.Message,
                    "Ошибка БД",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Произошла ошибка:\n\n" +
                    ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // НАЗВАНИЯ СТОЛБЦОВ
        // =========================================================

        private void ConfigureColumnNames()
        {
            if (dataGridView1.Columns.Count == 0)
                return;

            dataGridView1.Columns["OrderID"].HeaderText =
                "Номер заказа";

            dataGridView1.Columns["OrderDate"].HeaderText =
                "Дата заказа";

            dataGridView1.Columns["CustomerName"].HeaderText =
                "Клиент";

            dataGridView1.Columns["OrderTotal"].HeaderText =
                "Сумма";

            dataGridView1.Columns["StatusName"].HeaderText =
                "Статус";

            dataGridView1.Columns["OrderID"].FillWeight = 60;
            dataGridView1.Columns["OrderDate"].FillWeight = 110;
            dataGridView1.Columns["CustomerName"].FillWeight = 170;
            dataGridView1.Columns["OrderTotal"].FillWeight = 90;
            dataGridView1.Columns["StatusName"].FillWeight = 100;

            dataGridView1.Columns["OrderDate"]
                .DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";

            dataGridView1.Columns["OrderTotal"]
                .DefaultCellStyle.Format = "N2";
        }

        // =========================================================
        // ВЫБОР ДЕЙСТВИЯ С ЗАКАЗОМ
        // =========================================================

        private void dataGridView1_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row =
                dataGridView1.Rows[e.RowIndex];

            int orderID = Convert.ToInt32(
                row.Cells["OrderID"].Value);

            string customerName =
                row.Cells["CustomerName"].Value.ToString();

            string status =
                row.Cells["StatusName"].Value.ToString();

            DialogResult result = MessageBox.Show(
                "Заказ № " + orderID + "\n\n" +
                "Клиент: " + customerName + "\n" +
                "Статус: " + status + "\n\n" +

                "ДА — сформировать чек Word\n" +
                "НЕТ — отменить заказ\n" +
                "ОТМЕНА — закрыть окно",

                "Действия с заказом",

                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Формирование чека
                try
                {
                    CreateWordReceipt(orderID);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Не удалось сформировать чек:\n\n" +
                        ex.Message,
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            else if (result == DialogResult.No)
            {
                // Отмена заказа
                CancelOrder(orderID, status);
            }
        }

        // =========================================================
        // ОТМЕНА ЗАКАЗА
        // =========================================================

        private void CancelOrder(int orderID, string currentStatus)
        {
            if (currentStatus == "Отменён" ||
                currentStatus == "Отменен")
            {
                MessageBox.Show(
                    "Этот заказ уже отменён.",
                    "Отмена заказа",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Вы действительно хотите отменить заказ № " +
                orderID + "?\n\n" +
                "После подтверждения статус заказа изменится в базе данных.",

                "Подтверждение отмены",

                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                string connectionString =
                    DBconfig.BuildConnectionString();

                using (MySqlConnection connection =
                    new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Получаем ID статуса "Отменён"
                    string statusQuery = @"
                        SELECT StatusID
                        FROM Statuses
                        WHERE StatusName IN ('Отменено', 'Отменено')
                        LIMIT 1;
                    ";

                    int cancelledStatusID;

                    using (MySqlCommand command =
                        new MySqlCommand(statusQuery, connection))
                    {
                        object result = command.ExecuteScalar();

                        if (result == null ||
                            result == DBNull.Value)
                        {
                            throw new Exception(
                                "В таблице Statuses отсутствует статус 'Отменено'.");
                        }

                        cancelledStatusID =
                            Convert.ToInt32(result);
                    }

                    // Обновляем статус заказа
                    string updateQuery = @"
                        UPDATE Orders
                        SET StatusID = @StatusID
                        WHERE OrderID = @OrderID;
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@StatusID", cancelledStatusID);

                        command.Parameters.AddWithValue(
                            "@OrderID", orderID);

                        int affectedRows =
                            command.ExecuteNonQuery();

                        if (affectedRows == 0)
                        {
                            throw new Exception(
                                "Заказ не найден или не был изменён.");
                        }
                    }
                }

                MessageBox.Show(
                    "Заказ № " + orderID +
                    " успешно отменён.",
                    "Отмена заказа",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Обновляем таблицу
                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось отменить заказ:\n\n" +
                    ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // ФОРМИРОВАНИЕ ЧЕКА WORD
        // =========================================================

        private void CreateWordReceipt(int orderID)
        {
            string connectionString =
                DBconfig.BuildConnectionString();

            DataTable orderInfo = new DataTable();
            DataTable orderItems = new DataTable();

            using (MySqlConnection connection =
                new MySqlConnection(connectionString))
            {
                connection.Open();

                // Информация о заказе
                string orderQuery = @"
                    SELECT
                        o.OrderID,
                        o.OrderDate,

                        c.FirstName,
                        c.MiddleName,
                        c.LastName,
                        c.CustomerPhone,

                        u.FirstName AS SellerFirstName,
                        u.LastName AS SellerLastName,

                        s.StatusName

                    FROM Orders o

                    INNER JOIN Customers c
                        ON o.CustomerID = c.CustomerID

                    INNER JOIN Users u
                        ON o.UserID = u.UserID

                    INNER JOIN Statuses s
                        ON o.StatusID = s.StatusID

                    WHERE o.OrderID = @OrderID;
                ";

                using (MySqlCommand command =
                    new MySqlCommand(orderQuery, connection))
                {
                    command.Parameters.AddWithValue(
                        "@OrderID", orderID);

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(command))
                    {
                        adapter.Fill(orderInfo);
                    }
                }

                if (orderInfo.Rows.Count == 0)
                {
                    throw new Exception(
                        "Заказ не найден.");
                }

                // Товары заказа
                string itemsQuery = @"
                    SELECT
                        p.Name,
                        oi.Quantity,
                        oi.PriceAtPurchase,

                        (oi.Quantity * oi.PriceAtPurchase) AS Total

                    FROM OrderItems oi

                    INNER JOIN Products p
                        ON oi.ProductID = p.ProductID

                    WHERE oi.OrderID = @OrderID;
                ";

                using (MySqlCommand command =
                    new MySqlCommand(itemsQuery, connection))
                {
                    command.Parameters.AddWithValue(
                        "@OrderID", orderID);

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(command))
                    {
                        adapter.Fill(orderItems);
                    }
                }
            }

            DataRow order = orderInfo.Rows[0];

            string customerName =
                order["LastName"] + " " +
                order["FirstName"] + " " +
                (order["MiddleName"] == DBNull.Value
                    ? ""
                    : order["MiddleName"].ToString());

            string sellerName =
                order["SellerLastName"] + " " +
                order["SellerFirstName"];

            DateTime orderDate =
                Convert.ToDateTime(order["OrderDate"]);

            string customerPhone =
                order["CustomerPhone"] == DBNull.Value
                    ? "Не указан"
                    : order["CustomerPhone"].ToString();

            string status =
                order["StatusName"].ToString();

            decimal grandTotal = 0;

            foreach (DataRow item in orderItems.Rows)
            {
                grandTotal += Convert.ToDecimal(
                    item["Total"]);
            }

            // Папка для чеков
            string receiptsFolder = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                "ShopDB_Receipts");

            Directory.CreateDirectory(receiptsFolder);

            string filePath = Path.Combine(
                receiptsFolder,
                "Чек_заказ_" + orderID + ".docx");

            Word.Application wordApp = null;
            Word.Document document = null;

            try
            {
                wordApp = new Word.Application();

                document = wordApp.Documents.Add();

                // Заголовок
                Word.Paragraph title =
                    document.Paragraphs.Add();

                title.Range.Text =
                    "ЧЕК ЗАКАЗА № " + orderID;

                title.Range.Font.Size = 18;
                title.Range.Font.Bold = 1;

                title.Range.ParagraphFormat.Alignment =
                    Word.WdParagraphAlignment
                    .wdAlignParagraphCenter;

                title.Range.InsertParagraphAfter();

                // Информация о заказе
                Word.Paragraph info =
                    document.Paragraphs.Add();

                info.Range.Text =
                    "\nДата оформления: " +
                    orderDate.ToString("dd.MM.yyyy HH:mm") +

                    "\nПокупатель: " + customerName +

                    "\nТелефон: " + customerPhone +

                    "\nПродавец: " + sellerName +

                    "\nСтатус: " + status +

                    "\n\nСписок товаров:";

                info.Range.Font.Size = 11;
                info.Range.Font.Bold = 0;

                info.Range.ParagraphFormat.Alignment =
                    Word.WdParagraphAlignment
                    .wdAlignParagraphLeft;

                info.Range.InsertParagraphAfter();

                // Таблица товаров
                Word.Range tableRange =
                    document.Content;

                tableRange.Collapse(
                    Word.WdCollapseDirection.wdCollapseEnd);

                Word.Table table = document.Tables.Add(
                    tableRange,
                    orderItems.Rows.Count + 1,
                    4);

                table.Borders.Enable = 1;

                table.Range.Font.Size = 10;

                // Заголовки
                table.Cell(1, 1).Range.Text = "Товар";
                table.Cell(1, 2).Range.Text = "Количество";
                table.Cell(1, 3).Range.Text = "Цена";
                table.Cell(1, 4).Range.Text = "Сумма";

                for (int column = 1; column <= 4; column++)
                {
                    table.Cell(1, column).Range.Bold = 1;
                }

                // Заполнение товаров
                for (int i = 0; i < orderItems.Rows.Count; i++)
                {
                    DataRow item = orderItems.Rows[i];

                    table.Cell(i + 2, 1).Range.Text =
                        item["Name"].ToString();

                    table.Cell(i + 2, 2).Range.Text =
                        item["Quantity"].ToString();

                    table.Cell(i + 2, 3).Range.Text =
                        Convert.ToDecimal(
                            item["PriceAtPurchase"])
                            .ToString("N2") + " руб.";

                    table.Cell(i + 2, 4).Range.Text =
                        Convert.ToDecimal(item["Total"])
                            .ToString("N2") + " руб.";
                }

                // Итоговая сумма
                Word.Range totalRange =
                    document.Content;

                totalRange.Collapse(
                    Word.WdCollapseDirection.wdCollapseEnd);

                totalRange.InsertParagraphAfter();

                totalRange.Collapse(
                    Word.WdCollapseDirection.wdCollapseEnd);

                totalRange.Text =
                    "\nИТОГО: " +
                    grandTotal.ToString("N2") + " руб.";

                totalRange.Font.Size = 14;
                totalRange.Font.Bold = 1;

                totalRange.ParagraphFormat.Alignment =
                    Word.WdParagraphAlignment
                    .wdAlignParagraphRight;

                // Завершающая надпись
                Word.Range footerRange =
                    document.Content;

                footerRange.Collapse(
                    Word.WdCollapseDirection.wdCollapseEnd);

                footerRange.InsertParagraphAfter();

                footerRange.Collapse(
                    Word.WdCollapseDirection.wdCollapseEnd);

                footerRange.Text =
                    "\nСпасибо за покупку!";

                footerRange.Font.Size = 11;
                footerRange.Font.Italic = 1;

                footerRange.ParagraphFormat.Alignment =
                    Word.WdParagraphAlignment
                    .wdAlignParagraphCenter;

                // Сохранение DOCX
                object fileName = filePath;

                object fileFormat =
                    Word.WdSaveFormat.wdFormatXMLDocument;

                object missing = Type.Missing;

                document.SaveAs(
                    ref fileName,
                    ref fileFormat,
                    ref missing,
                    ref missing,
                    ref missing,
                    ref missing,
                    ref missing,
                    ref missing,
                    ref missing,
                    ref missing,
                    ref missing,
                    ref missing,
                    ref missing,
                    ref missing,
                    ref missing,
                    ref missing);

                // Открываем документ
                wordApp.Visible = true;
                wordApp.Activate();

                MessageBox.Show(
                    "Чек успешно сформирован!\n\n" +
                    "Файл сохранён:\n" + filePath,
                    "Печать чека",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch
            {
                if (document != null)
                {
                    document.Close(
                        Word.WdSaveOptions.wdDoNotSaveChanges);
                }

                if (wordApp != null)
                {
                    wordApp.Quit();
                }

                throw;
            }
        }
    }
}
