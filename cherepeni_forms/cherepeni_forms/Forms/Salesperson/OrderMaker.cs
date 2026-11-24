using System;
using System.Data;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using cherepeni_forms.Classes;

namespace cherepeni_forms.Forms.Salesperson
{
    public partial class OrderMaker : Form
    {
        private DataTable productsTable;
        private DataTable pageTable;

        private int currentPage = 0;
        private int pageSize = 10;
        private int totalPages = 1;

        private Button prevPageButton;
        private Button nextPageButton;
        private Label pageLabel;
        private int userID;

        // Корзина: ключ - ID товара
        private Dictionary<int, CartItem> cart =
            new Dictionary<int, CartItem>();

        public OrderMaker(int userID)
        {
            InitializeComponent();

            this.userID = userID;

            ConfigureDataGridView();
            LoadSortCombo();
            LoadFilterCombo();
            ConfigurePagination();

            textBox1.TextChanged += textBox1_TextChanged;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;

            button2.Click += button2_Click;
            button4.Click += button4_Click;
            button6.Click += button6_Click;
            button7.Click += button7_Click;
            button8.Click += button8_Click;

            dataGridView1.CellDoubleClick +=
                dataGridView1_CellDoubleClick;

            LoadProducts();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        // =========================================================
        // DATAGRIDVIEW
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

            dataGridView1.CellFormatting +=
                dataGridView1_CellFormatting;
        }

        // =========================================================
        // ПАГИНАЦИЯ
        // =========================================================

        private void ConfigurePagination()
        {
            prevPageButton = new Button();
            nextPageButton = new Button();
            pageLabel = new Label();

            prevPageButton.Text = "Назад";
            nextPageButton.Text = "Вперёд";

            prevPageButton.Width = 80;
            nextPageButton.Width = 80;

            pageLabel.AutoSize = true;

            prevPageButton.Click += PrevPageButton_Click;
            nextPageButton.Click += NextPageButton_Click;

            this.Controls.Add(prevPageButton);
            this.Controls.Add(nextPageButton);
            this.Controls.Add(pageLabel);

            prevPageButton.Location = new Point(20, 500);
            pageLabel.Location = new Point(120, 508);
            nextPageButton.Location = new Point(220, 500);
        }

        private void PrevPageButton_Click(object sender, EventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                ApplyFilters();
            }
        }

        private void NextPageButton_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages - 1)
            {
                currentPage++;
                ApplyFilters();
            }
        }

        private void UpdatePagination()
        {
            pageLabel.Text =
                "Страница " + (currentPage + 1) +
                " из " + totalPages;

            prevPageButton.Enabled = currentPage > 0;

            nextPageButton.Enabled =
                currentPage < totalPages - 1;
        }

        // =========================================================
        // ЗАГРУЗКА ТОВАРОВ
        // =========================================================

        private void LoadProducts()
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
                            p.ProductID,
                            p.Name,
                            p.Description,
                            c.CategoryName,
                            p.StockQuantity,
                            p.Price
                        FROM Products p
                        INNER JOIN Categories c
                            ON p.CategoryID = c.CategoryID;
                    ";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, connection))
                    {
                        productsTable = new DataTable();
                        adapter.Fill(productsTable);
                    }
                }

                currentPage = 0;
                ApplyFilters();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Ошибка при загрузке товаров:\n\n" +
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

        private void SetColumnNames()
        {
            if (dataGridView1.Columns.Count == 0)
                return;

            dataGridView1.Columns["ProductID"].HeaderText = "ID";
            dataGridView1.Columns["Name"].HeaderText = "Наименование";
            dataGridView1.Columns["Description"].HeaderText = "Описание";
            dataGridView1.Columns["CategoryName"].HeaderText = "Категория";
            dataGridView1.Columns["StockQuantity"].HeaderText = "Остаток";
            dataGridView1.Columns["Price"].HeaderText = "Цена";

            dataGridView1.Columns["ProductID"].FillWeight = 40;
            dataGridView1.Columns["Name"].FillWeight = 140;
            dataGridView1.Columns["Description"].FillWeight = 180;
            dataGridView1.Columns["CategoryName"].FillWeight = 100;
            dataGridView1.Columns["StockQuantity"].FillWeight = 70;
            dataGridView1.Columns["Price"].FillWeight = 80;
        }

        // =========================================================
        // ПОИСК
        // =========================================================

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            currentPage = 0;
            ApplyFilters();
        }

        // =========================================================
        // СОРТИРОВКА
        // =========================================================

        private void LoadSortCombo()
        {
            comboBox1.Items.Clear();

            comboBox1.Items.Add("По ID");
            comboBox1.Items.Add("Наименование А-Я");
            comboBox1.Items.Add("Наименование Я-А");
            comboBox1.Items.Add("Цена по возрастанию");
            comboBox1.Items.Add("Цена по убыванию");
            comboBox1.Items.Add("Остаток по возрастанию");
            comboBox1.Items.Add("Остаток по убыванию");

            comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(
            object sender, EventArgs e)
        {
            currentPage = 0;
            ApplyFilters();
        }

        private string GetSortExpression()
        {
            switch (comboBox1.SelectedIndex)
            {
                case 1:
                    return "Name ASC";

                case 2:
                    return "Name DESC";

                case 3:
                    return "Price ASC";

                case 4:
                    return "Price DESC";

                case 5:
                    return "StockQuantity ASC";

                case 6:
                    return "StockQuantity DESC";

                default:
                    return "ProductID ASC";
            }
        }

        // =========================================================
        // ФИЛЬТРАЦИЯ
        // =========================================================

        private void LoadFilterCombo()
        {
            comboBox2.Items.Clear();

            comboBox2.Items.Add("Все товары");
            comboBox2.Items.Add("Товары в наличии");
            comboBox2.Items.Add("Нет в наличии");

            comboBox2.Items.Add("Цена: до 100");
            comboBox2.Items.Add("Цена: 100 - 500");
            comboBox2.Items.Add("Цена: 500 - 1000");
            comboBox2.Items.Add("Цена: от 1000");

            try
            {
                string connectionString =
                    DBconfig.BuildConnectionString();

                using (MySqlConnection connection =
                    new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT CategoryName
                        FROM Categories
                        ORDER BY CategoryName;
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                comboBox2.Items.Add(
                                    "Категория: " +
                                    reader["CategoryName"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки категорий:\n" +
                    ex.Message);
            }

            comboBox2.SelectedIndex = 0;
        }

        private void comboBox2_SelectedIndexChanged(
            object sender, EventArgs e)
        {
            currentPage = 0;
            ApplyFilters();
        }

        private string EscapeFilterValue(string value)
        {
            return value
                .Replace("'", "''")
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("*", "[*]");
        }

        private string GetFilterExpression()
        {
            switch (comboBox2.SelectedIndex)
            {
                case 1:
                    return "StockQuantity > 0";

                case 2:
                    return "StockQuantity = 0";

                case 3:
                    return "Price < 100";

                case 4:
                    return "Price >= 100 AND Price < 500";

                case 5:
                    return "Price >= 500 AND Price < 1000";

                case 6:
                    return "Price >= 1000";

                default:
                    // Категории обрабатываются отдельно
                    return "";
            }
        }

        // =========================================================
        // ОБЩЕЕ ПРИМЕНЕНИЕ ПОИСКА, ФИЛЬТРА И СОРТИРОВКИ
        // =========================================================

        private void ApplyFilters()
        {
            if (productsTable == null)
                return;

            try
            {
                DataView view = productsTable.DefaultView;

                string search =
                    EscapeFilterValue(textBox1.Text.Trim());

                string filter = GetFilterExpression();

                // Поиск по наименованию
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string searchFilter =
                        "Name LIKE '%" + search + "%'";

                    if (!string.IsNullOrWhiteSpace(filter))
                        filter += " AND ";

                    filter += searchFilter;
                }

                // Фильтрация по категории
                if (comboBox2.SelectedItem != null)
                {
                    string selected =
                        comboBox2.SelectedItem.ToString();

                    if (selected.StartsWith("Категория: "))
                    {
                        string category =
                            EscapeFilterValue(
                                selected.Substring(
                                    "Категория: ".Length));

                        if (!string.IsNullOrWhiteSpace(filter))
                            filter += " AND ";

                        filter +=
                            "CategoryName = '" +
                            category + "'";
                    }
                }

                view.RowFilter = filter;
                view.Sort = GetSortExpression();

                DataTable filteredTable = view.ToTable();

                int recordsCount = filteredTable.Rows.Count;

                totalPages = Math.Max(
                    1,
                    (int)Math.Ceiling(
                        (double)recordsCount / pageSize));

                if (currentPage >= totalPages)
                    currentPage = totalPages - 1;

                if (currentPage < 0)
                    currentPage = 0;

                pageTable = filteredTable.Clone();

                int startIndex = currentPage * pageSize;
                int endIndex = Math.Min(
                    startIndex + pageSize,
                    recordsCount);

                for (int i = startIndex; i < endIndex; i++)
                {
                    pageTable.ImportRow(filteredTable.Rows[i]);
                }

                dataGridView1.DataSource = pageTable;

                SetColumnNames();

                UpdateCount(recordsCount);
                UpdatePagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при применении фильтров:\n\n" +
                    ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // КОЛИЧЕСТВО ЗАПИСЕЙ
        // =========================================================

        private void UpdateCount(int count)
        {
            label4.Text =
                "Количество товаров: " + count +
                " | В корзине: " + cart.Count;
        }

        // =========================================================
        // УСЛОВНОЕ ФОРМАТИРОВАНИЕ
        // =========================================================

        private void dataGridView1_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (!dataGridView1.Columns.Contains("StockQuantity"))
                return;

            DataGridViewRow row =
                dataGridView1.Rows[e.RowIndex];

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
                    dataGridView1.DefaultCellStyle.BackColor;

                row.DefaultCellStyle.ForeColor =
                    dataGridView1.DefaultCellStyle.ForeColor;
            }
        }

        // =========================================================
        // ДОБАВЛЕНИЕ В КОРЗИНУ
        // =========================================================

        private void button2_Click(object sender, EventArgs e)
        {
            AddSelectedProductToCart();
        }

        private void dataGridView1_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            AddSelectedProductToCart();
        }

        private void AddSelectedProductToCart()
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Выберите товар.",
                    "Добавление в корзину",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            DataGridViewRow row =
                dataGridView1.SelectedRows[0];

            int productID = Convert.ToInt32(
                row.Cells["ProductID"].Value);

            string name =
                row.Cells["Name"].Value.ToString();

            decimal price = Convert.ToDecimal(
                row.Cells["Price"].Value);

            int stock = Convert.ToInt32(
                row.Cells["StockQuantity"].Value);

            if (stock <= 0)
            {
                MessageBox.Show(
                    "Данный товар отсутствует на складе.",
                    "Добавление в корзину",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            int currentQuantity = cart.ContainsKey(productID)
                ? cart[productID].Quantity
                : 0;

            int available = stock - currentQuantity;

            if (available <= 0)
            {
                MessageBox.Show(
                    "Все доступные единицы товара уже находятся в корзине.",
                    "Добавление в корзину",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using (QuantityDialog dialog =
                new QuantityDialog(available))
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                int quantity = dialog.Quantity;

                if (cart.ContainsKey(productID))
                {
                    cart[productID].Quantity += quantity;
                }
                else
                {
                    cart.Add(
                        productID,
                        new CartItem(
                            productID,
                            name,
                            price,
                            quantity,
                            stock));
                }
            }

            UpdateCount(
                productsTable.DefaultView.Count);

            MessageBox.Show(
                "Товар добавлен в корзину.",
                "Корзина",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // =========================================================
        // ПРОСМОТР КОРЗИНЫ
        // =========================================================

        private void button6_Click(object sender, EventArgs e)
        {
            if (cart.Count == 0)
            {
                MessageBox.Show(
                    "Корзина пуста.",
                    "Корзина",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (Korzina form = new Korzina(cart.Values.ToList(), userID))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    cart.Clear();
                    LoadProducts();
                }
            }
        }

        // =========================================================
        // СБРОС КОРЗИНЫ
        // =========================================================

        private void button7_Click(object sender, EventArgs e)
        {
            if (cart.Count == 0)
            {
                MessageBox.Show("Корзина уже пуста.");
                return;
            }

            DialogResult result = MessageBox.Show(
                "Вы действительно хотите очистить корзину?",
                "Сброс корзины",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            cart.Clear();

            UpdateCount(
                productsTable.DefaultView.Count);

            MessageBox.Show(
                "Корзина очищена.",
                "Корзина",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // =========================================================
        // ОТМЕНА
        // =========================================================

        private void button8_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Вы действительно хотите отменить составление заказа?\n" +
                "Все товары в корзине будут удалены.",
                "Отмена заказа",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                cart.Clear();
                this.Close();
            }
        }

        // =========================================================
        // ОБНОВЛЕНИЕ
        // =========================================================

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Clear();

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;

            currentPage = 0;

            LoadProducts();
        }
    }

    // =============================================================
    // ЭЛЕМЕНТ КОРЗИНЫ
    // =============================================================

    public class CartItem
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }

        public decimal Total
        {
            get { return Price * Quantity; }
        }

        public CartItem(
            int productID,
            string name,
            decimal price,
            int quantity,
            int stockQuantity)
        {
            ProductID = productID;
            Name = name;
            Price = price;
            Quantity = quantity;
            StockQuantity = stockQuantity;
        }
    }

    // =============================================================
    // ВСПЛЫВАЮЩЕЕ ОКНО ВЫБОРА КОЛИЧЕСТВА
    // =============================================================

    public class QuantityDialog : Form
    {
        private NumericUpDown numericUpDown;
        private Button okButton;
        private Button cancelButton;

        public int Quantity
        {
            get { return (int)numericUpDown.Value; }
        }

        public QuantityDialog(int maxQuantity)
        {
            Text = "Выбор количества";
            Width = 300;
            Height = 160;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            Label label = new Label();
            label.Text = "Количество:";
            label.Location = new Point(20, 20);
            label.AutoSize = true;

            numericUpDown = new NumericUpDown();
            numericUpDown.Location = new Point(110, 18);
            numericUpDown.Width = 120;
            numericUpDown.Minimum = 1;
            numericUpDown.Maximum = Math.Max(1, maxQuantity);
            numericUpDown.Value = 1;

            okButton = new Button();
            okButton.Text = "Добавить";
            okButton.Location = new Point(30, 70);
            okButton.DialogResult = DialogResult.OK;

            cancelButton = new Button();
            cancelButton.Text = "Отмена";
            cancelButton.Location = new Point(150, 70);
            cancelButton.DialogResult = DialogResult.Cancel;

            Controls.Add(label);
            Controls.Add(numericUpDown);
            Controls.Add(okButton);
            Controls.Add(cancelButton);

            AcceptButton = okButton;
            CancelButton = cancelButton;
        }
    }
}