using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using cherepeni_forms.Classes;
using MySql.Data.MySqlClient;
using System.IO;
using Word = Microsoft.Office.Interop.Word;

namespace cherepeni_forms.Forms.Salesperson
{
    public partial class Korzina : Form
    {
        private List<CartItem> cart;
        private int userID;

        private int selectedCustomerID = -1;

        public Korzina(List<CartItem> items, int userID)
        {
            InitializeComponent();

            this.cart = items;
            this.userID = userID;

            ConfigureDataGridView();
            LoadCart();

            button1.Click += button1_Click;
            button2.Click += button2_Click;
            button3.Click += button3_Click;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void CreateWordReceipt(int orderID)
        {
            string connectionString = DBconfig.BuildConnectionString();

            DataTable orderInfo = new DataTable();
            DataTable orderItems = new DataTable();

            using (MySqlConnection connection =
                new MySqlConnection(connectionString))
            {
                connection.Open();

                // Получаем информацию о заказе, покупателе и продавце
                string orderQuery = @"
            SELECT
                o.OrderID,
                o.OrderDate,
                c.FirstName,
                c.MiddleName,
                c.LastName,
                c.CustomerPhone,
                u.FirstName AS SellerFirstName,
                u.LastName AS SellerLastName
            FROM Orders o
            INNER JOIN Customers c
                ON o.CustomerID = c.CustomerID
            INNER JOIN Users u
                ON o.UserID = u.UserID
            WHERE o.OrderID = @OrderID;
        ";

                using (MySqlCommand command =
                    new MySqlCommand(orderQuery, connection))
                {
                    command.Parameters.AddWithValue("@OrderID", orderID);

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(command))
                    {
                        adapter.Fill(orderInfo);
                    }
                }

                if (orderInfo.Rows.Count == 0)
                    throw new Exception("Не удалось найти информацию о заказе.");

                // Получаем товары заказа
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
                    command.Parameters.AddWithValue("@OrderID", orderID);

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

            DateTime orderDate = Convert.ToDateTime(order["OrderDate"]);

            decimal grandTotal = 0;

            foreach (DataRow row in orderItems.Rows)
            {
                grandTotal += Convert.ToDecimal(row["Total"]);
            }

            // Создаём папку для чеков
            string receiptsFolder = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                "ShopDB_Receipts"
            );

            Directory.CreateDirectory(receiptsFolder);

            string filePath = Path.Combine(
                receiptsFolder,
                "Чек_заказ_" + orderID + ".docx"
            );

            Word.Application wordApp = null;
            Word.Document document = null;

            try
            {
                wordApp = new Word.Application();
                document = wordApp.Documents.Add();

                // Заголовок
                Word.Paragraph title = document.Paragraphs.Add();

                title.Range.Text = "ЧЕК ЗАКАЗА № " + orderID;
                title.Range.Font.Size = 18;
                title.Range.Font.Bold = 1;
                title.Range.ParagraphFormat.Alignment =
                    Word.WdParagraphAlignment.wdAlignParagraphCenter;

                title.Range.InsertParagraphAfter();

                // Информация о заказе
                Word.Paragraph info = document.Paragraphs.Add();

                info.Range.Text =
                    "\nДата оформления: " + orderDate.ToString("dd.MM.yyyy HH:mm") +
                    "\nПокупатель: " + customerName +
                    "\nТелефон: " + order["CustomerPhone"] +
                    "\nПродавец: " + sellerName +
                    "\n\nСписок товаров:";

                info.Range.Font.Size = 11;
                info.Range.Font.Bold = 0;
                info.Range.ParagraphFormat.Alignment =
                    Word.WdParagraphAlignment.wdAlignParagraphLeft;

                info.Range.InsertParagraphAfter();

                // Создаём таблицу товаров
                Word.Range tableRange = document.Content;
                tableRange.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                Word.Table table = document.Tables.Add(
                    tableRange,
                    orderItems.Rows.Count + 1,
                    4
                );

                table.Borders.Enable = 1;
                table.Range.Font.Size = 10;

                // Заголовки столбцов
                table.Cell(1, 1).Range.Text = "Товар";
                table.Cell(1, 2).Range.Text = "Кол-во";
                table.Cell(1, 3).Range.Text = "Цена";
                table.Cell(1, 4).Range.Text = "Сумма";

                for (int column = 1; column <= 4; column++)
                {
                    table.Cell(1, column).Range.Bold = 1;
                }

                // Заполняем таблицу товарами
                for (int i = 0; i < orderItems.Rows.Count; i++)
                {
                    DataRow item = orderItems.Rows[i];

                    table.Cell(i + 2, 1).Range.Text =
                        item["Name"].ToString();

                    table.Cell(i + 2, 2).Range.Text =
                        item["Quantity"].ToString();

                    table.Cell(i + 2, 3).Range.Text =
                        Convert.ToDecimal(item["PriceAtPurchase"])
                        .ToString("N2") + " руб.";

                    table.Cell(i + 2, 4).Range.Text =
                        Convert.ToDecimal(item["Total"])
                        .ToString("N2") + " руб.";
                }

                // Итоговая сумма
                Word.Range totalRange = document.Content;
                totalRange.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
                totalRange.InsertParagraphAfter();
                totalRange.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                totalRange.Text =
                    "\nИТОГО К ОПЛАТЕ: " +
                    grandTotal.ToString("N2") + " руб.";

                totalRange.Font.Size = 14;
                totalRange.Font.Bold = 1;
                totalRange.ParagraphFormat.Alignment =
                    Word.WdParagraphAlignment.wdAlignParagraphRight;

                // Завершающая надпись
                Word.Range footerRange = document.Content;
                footerRange.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
                footerRange.InsertParagraphAfter();
                footerRange.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                footerRange.Text =
                    "\nСпасибо за покупку!";

                footerRange.Font.Size = 11;
                footerRange.Font.Italic = 1;
                footerRange.ParagraphFormat.Alignment =
                    Word.WdParagraphAlignment.wdAlignParagraphCenter;

                MessageBox.Show(filePath);
                // Сохраняем документ
                object fileName = filePath;
                object fileFormat = Word.WdSaveFormat.wdFormatXMLDocument;
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
                    ref missing
                );

                // Показываем Word
                wordApp.Visible = true;
                wordApp.Activate();
            }
            catch
            {
                if (document != null)
                    document.Close(Word.WdSaveOptions.wdDoNotSaveChanges);

                if (wordApp != null)
                    wordApp.Quit();

                throw;
            }
        }

        private void dataGridView1_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (!dataGridView.Columns.Contains("StockQuantity"))
                return;

            DataGridViewRow row =
                dataGridView.Rows[e.RowIndex];

            object value =
                row.Cells["StockQuantity"].Value;

            if (value == null || value == DBNull.Value)
                return;

            int stock = Convert.ToInt32(value);

            if (stock == 0)
            {
                row.DefaultCellStyle.BackColor =
                    Color.LightCoral;

                row.DefaultCellStyle.ForeColor =
                    Color.Black;
            }
            else
            {
                row.DefaultCellStyle.BackColor =
                    dataGridView.DefaultCellStyle.BackColor;

                row.DefaultCellStyle.ForeColor =
                    dataGridView.DefaultCellStyle.ForeColor;
            }
        }

        // =========================================================
        // DATAGRIDVIEW
        // =========================================================

        private void ConfigureDataGridView()
        {
            dataGridView.AutoGenerateColumns = true;
            dataGridView.ReadOnly = true;

            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.AllowUserToResizeRows = false;

            dataGridView.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dataGridView.MultiSelect = false;

            dataGridView.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView.RowHeadersVisible = false;

            dataGridView.CellFormatting +=
                dataGridView1_CellFormatting;
        }

        // =========================================================
        // ЗАГРУЗКА КОРЗИНЫ
        // =========================================================

        private void LoadCart()
        {
            DataTable table = new DataTable();

            table.Columns.Add("ProductID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Price", typeof(decimal));
            table.Columns.Add("Quantity", typeof(int));
            table.Columns.Add("Total", typeof(decimal));

            foreach (CartItem item in cart)
            {
                table.Rows.Add(
                    item.ProductID,
                    item.Name,
                    item.Price,
                    item.Quantity,
                    item.Total);
            }

            dataGridView.DataSource = table;

            if (dataGridView.Columns.Count > 0)
            {
                dataGridView.Columns["ProductID"].HeaderText = "ID";
                dataGridView.Columns["Name"].HeaderText = "Товар";
                dataGridView.Columns["Price"].HeaderText = "Цена";
                dataGridView.Columns["Quantity"].HeaderText = "Количество";
                dataGridView.Columns["Total"].HeaderText = "Сумма";
            }

            decimal total = cart.Sum(x => x.Total);

            label4.Text =
                "Итоговая сумма: " + total.ToString("N2") + " руб.";

            label1.Text = "Клиент: не выбран";
        }

        // =========================================================
        // ВЫБОР КЛИЕНТА
        // =========================================================

        private void button3_Click(object sender, EventArgs e)
        {
            using (Customer_get form = new Customer_get())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    selectedCustomerID = form.CustomerID;

                    label1.Text = "Клиент: " + form.CustomerName;
                }
            }
        }

        // =========================================================
        // ВОЗВРАТ К ТОВАРАМ
        // =========================================================

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        // =========================================================
        // ОФОРМЛЕНИЕ ЗАКАЗА
        // =========================================================

        private void button2_Click(object sender, EventArgs e)
        {
            if (cart == null || cart.Count == 0)
            {
                MessageBox.Show(
                    "Корзина пуста.",
                    "Оформление заказа",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (selectedCustomerID == -1)
            {
                MessageBox.Show(
                    "Сначала выберите клиента.",
                    "Оформление заказа",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Оформить заказ на сумму " +
                cart.Sum(x => x.Total).ToString("N2") +
                " руб.?",
                "Подтверждение заказа",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            SaveOrder();
        }

        // =========================================================
        // СОХРАНЕНИЕ ЗАКАЗА В БД
        // =========================================================

        private void SaveOrder()
        {
            string connectionString =
                DBconfig.BuildConnectionString();

            using (MySqlConnection connection =
                new MySqlConnection(connectionString))
            {
                MySqlTransaction transaction = null;

                try
                {
                    connection.Open();

                    transaction = connection.BeginTransaction();

                    // -------------------------------------------------
                    // Получаем статус заказа
                    // -------------------------------------------------

                    int statusID;

                    string statusQuery = @"
                        SELECT StatusID
                        FROM Statuses
                        WHERE StatusName = @StatusName
                        LIMIT 1;
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(statusQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue(
                            "@StatusName", "Продано");

                        object result = command.ExecuteScalar();

                        if (result == null || result == DBNull.Value)
                        {
                            throw new Exception(
                                "В таблице Statuses отсутствует статус 'Продано'.");
                        }

                        statusID = Convert.ToInt32(result);
                    }

                    // -------------------------------------------------
                    // Проверяем остатки и блокируем строки
                    // -------------------------------------------------

                    foreach (CartItem item in cart)
                    {
                        string stockQuery = @"
                            SELECT StockQuantity
                            FROM Products
                            WHERE ProductID = @ProductID
                            FOR UPDATE;
                        ";

                        using (MySqlCommand command =
                            new MySqlCommand(
                                stockQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue(
                                "@ProductID", item.ProductID);

                            object result = command.ExecuteScalar();

                            if (result == null || result == DBNull.Value)
                            {
                                throw new Exception(
                                    "Товар " + item.Name +
                                    " отсутствует в базе данных.");
                            }

                            int stock = Convert.ToInt32(result);

                            if (stock < item.Quantity)
                            {
                                throw new Exception(
                                    "Недостаточно товара '" +
                                    item.Name + "' на складе.\n" +
                                    "Доступно: " + stock +
                                    "\nВ заказе: " + item.Quantity);
                            }
                        }
                    }

                    // -------------------------------------------------
                    // Создаём заказ
                    // -------------------------------------------------

                    string orderQuery = @"
                        INSERT INTO Orders
                        (UserID, StatusID, CustomerID)
                        VALUES
                        (@UserID, @StatusID, @CustomerID);

                        SELECT LAST_INSERT_ID();
                    ";

                    int orderID;

                    using (MySqlCommand command =
                        new MySqlCommand(
                            orderQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue(
                            "@UserID", userID);

                        command.Parameters.AddWithValue(
                            "@StatusID", statusID);

                        command.Parameters.AddWithValue(
                            "@CustomerID", selectedCustomerID);

                        orderID = Convert.ToInt32(
                            command.ExecuteScalar());
                    }

                    // -------------------------------------------------
                    // Сохраняем товары и уменьшаем остатки
                    // -------------------------------------------------

                    foreach (CartItem item in cart)
                    {
                        string itemQuery = @"
                            INSERT INTO OrderItems
                            (OrderID, ProductID, Quantity, PriceAtPurchase)
                            VALUES
                            (@OrderID, @ProductID, @Quantity, @Price);
                        ";

                        using (MySqlCommand command =
                            new MySqlCommand(
                                itemQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue(
                                "@OrderID", orderID);

                            command.Parameters.AddWithValue(
                                "@ProductID", item.ProductID);

                            command.Parameters.AddWithValue(
                                "@Quantity", item.Quantity);

                            command.Parameters.AddWithValue(
                                "@Price", item.Price);

                            command.ExecuteNonQuery();
                        }

                        string updateQuery = @"
                            UPDATE Products
                            SET StockQuantity = StockQuantity - @Quantity
                            WHERE ProductID = @ProductID
                              AND StockQuantity >= @Quantity;
                        ";

                        using (MySqlCommand command =
                            new MySqlCommand(
                                updateQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue(
                                "@Quantity", item.Quantity);

                            command.Parameters.AddWithValue(
                                "@ProductID", item.ProductID);

                            int affectedRows =
                                command.ExecuteNonQuery();

                            if (affectedRows != 1)
                            {
                                throw new Exception(
                                    "Не удалось обновить остаток товара: " +
                                    item.Name);
                            }
                        }
                    }

                    // -------------------------------------------------
                    // Завершаем транзакцию
                    // -------------------------------------------------

                    transaction.Commit();

                    // Заказ уже сохранён в базе данных
                    MessageBox.Show(
                        "Заказ успешно оформлен!\n\n" +
                        "Номер заказа: " + orderID,
                        "Оформление заказа",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // Создаём чек в Word
                    try
                    {
                        CreateWordReceipt(orderID);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "Заказ сохранён, но не удалось создать чек:\n\n" +
                            ex.Message,
                            "Ошибка создания чека",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }

                    cart.Clear();

                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                            // Транзакция могла завершиться ранее
                        }
                    }

                    MessageBox.Show(
                        "Ошибка оформления заказа:\n\n" +
                        ex.Message,
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}