using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using cherepeni_forms.Classes;

namespace cherepeni_forms.Forms.Manager
{
    public partial class Products_view : Form
    {
        private DataTable productsTable;
        private DataTable pageTable;

        private int currentPage = 0;
        private int pageSize = 10;
        private int totalPages = 1;

        private Button prevPageButton;
        private Button nextPageButton;
        private Label pageLabel;
        public Products_view()
        {
            InitializeComponent();

            ConfigureDataGridView();
            LoadSortCombo();
            LoadFilterCombo();
            ConfigurePagination();

            // События
            textBox1.TextChanged += textBox1_TextChanged;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;

            button2.Click += button2_Click;
            button4.Click += button4_Click;

            dataGridView1.CellDoubleClick +=
                dataGridView1_CellDoubleClick;

            LoadProducts();

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

            // Добавляем элементы на форму
            this.Controls.Add(prevPageButton);
            this.Controls.Add(nextPageButton);
            this.Controls.Add(pageLabel);

            // Размещаем в нижней части формы
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

                ConfigureColumnNames();

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

        private void ConfigureColumnNames()
        {
            if (dataGridView1.Columns.Count == 0)
                return;

            // Названия будут применены после привязки
            // таблицы на первой странице.
        }

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

            comboBox2.SelectedIndex = 0;
        }

        private void comboBox2_SelectedIndexChanged(
            object sender, EventArgs e)
        {
            currentPage = 0;
            ApplyFilters();
        }

        private string GetFilterExpression()
        {
            switch (comboBox2.SelectedIndex)
            {
                case 1:
                    return "StockQuantity > 0";

                case 2:
                    return "StockQuantity = 0";

                default:
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

                string search = textBox1.Text.Trim()
                    .Replace("'", "''")
                    .Replace("[", "[[]")
                    .Replace("%", "[%]")
                    .Replace("*", "[*]");

                string filter = GetFilterExpression();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string searchFilter =
                        "Name LIKE '%" + search + "%'";

                    if (!string.IsNullOrWhiteSpace(filter))
                        filter += " AND ";

                    filter += searchFilter;
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

                // Формируем таблицу текущей страницы
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
            label4.Text = "Количество записей: " + count;
        }

        // =========================================================
        // ОБНОВЛЕНИЕ ПАГИНАЦИИ
        // =========================================================

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
        // ДОБАВЛЕНИЕ ТОВАРА
        // =========================================================

        private void button2_Click(object sender, EventArgs e)
        {
            using (Product_add form = new Product_add())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadProducts();
                }
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

        // =========================================================
        // ДЕЙСТВИЯ С ВЫБРАННЫМ ТОВАРОМ
        // =========================================================

        private void dataGridView1_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            int productID = Convert.ToInt32(
                dataGridView1.Rows[e.RowIndex]
                    .Cells["ProductID"].Value);

            string productName =
                dataGridView1.Rows[e.RowIndex]
                    .Cells["Name"].Value.ToString();

            DialogResult result = MessageBox.Show(
                "Выберите действие для товара:\n\n" +
                productName + "\n\n" +
                "ДА — изменить товар\n" +
                "НЕТ — удалить товар\n" +
                "ОТМЕНА — закрыть окно",
                "Действия с товаром",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                OpenEditForm(productID);
            }
            else if (result == DialogResult.No)
            {
                DeleteProduct(productID, productName);
            }
        }

        // =========================================================
        // РЕДАКТИРОВАНИЕ
        // =========================================================

        private void OpenEditForm(int productID)
        {
            using (Product_edit form =
                new Product_edit(productID))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadProducts();
                }
            }
        }

        // =========================================================
        // УДАЛЕНИЕ
        // =========================================================

        private void DeleteProduct(
            int productID,
            string productName)
        {
            DialogResult confirm = MessageBox.Show(
                "Вы действительно хотите удалить товар?\n\n" +
                productName,
                "Подтверждение удаления",
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

                    string query = @"
                        DELETE FROM Products
                        WHERE ProductID = @ProductID;
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@ProductID", productID);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Товар успешно удалён.",
                    "Удаление",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadProducts();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Не удалось удалить товар.\n\n" +
                    "Причина:\n" + ex.Message,
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
        // УСЛОВНОЕ ФОРМАТИРОВАНИЕ
        // =========================================================

        private void dataGridView1_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            DataGridViewRow row =
                dataGridView1.Rows[e.RowIndex];

            if (row.Cells["StockQuantity"].Value == null)
                return;

            if (row.Cells["StockQuantity"].Value == DBNull.Value)
                return;

            int stock = Convert.ToInt32(
                row.Cells["StockQuantity"].Value);

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
    }
}