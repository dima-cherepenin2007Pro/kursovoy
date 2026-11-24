using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using cherepeni_forms.Classes;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace cherepeni_forms.Forms
{
    public partial class Product_edit : Form
    {
        private int productID;

        public Product_edit(int productID)
        {
            InitializeComponent();

            this.productID = productID;

            // Обработчики событий
            textBox4.KeyPress += textBox4_KeyPress;
            textBox5.KeyPress += textBox5_KeyPress;

            button1.Click += button1_Click;
            button2.Click += button2_Click;

            // Загрузка категорий и товара
            LoadCategories();
            LoadProduct();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        // =========================================================
        // ЗАГРУЗКА КАТЕГОРИЙ
        // =========================================================

        private void LoadCategories()
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
                        SELECT CategoryID, CategoryName
                        FROM Categories
                        ORDER BY CategoryName;
                    ";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, connection))
                    {
                        DataTable categoriesTable = new DataTable();

                        adapter.Fill(categoriesTable);

                        comboBox1.DataSource = categoriesTable;
                        comboBox1.DisplayMember = "CategoryName";
                        comboBox1.ValueMember = "CategoryID";
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Ошибка при загрузке категорий:\n\n" +
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
        // ЗАГРУЗКА ТОВАРА
        // =========================================================

        private void LoadProduct()
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
                            Name,
                            Description,
                            CategoryID,
                            StockQuantity,
                            Price
                        FROM Products
                        WHERE ProductID = @ProductID;
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@ProductID", productID);

                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                textBox6.Text =
                                    reader["Name"].ToString();

                                textBox2.Text =
                                    reader["Description"] == DBNull.Value
                                    ? ""
                                    : reader["Description"].ToString();

                                textBox4.Text =
                                    reader["StockQuantity"].ToString();

                                textBox5.Text =
                                    Convert.ToDecimal(reader["Price"])
                                    .ToString(
                                        CultureInfo.InvariantCulture);

                                comboBox1.SelectedValue =
                                    Convert.ToInt32(reader["CategoryID"]);
                            }
                            else
                            {
                                MessageBox.Show(
                                    "Товар не найден.",
                                    "Ошибка",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                                this.Close();
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Ошибка при загрузке товара:\n\n" +
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
        // БЛОКИРОВКА СИМВОЛОВ В textBox4
        // =========================================================

        private void textBox4_KeyPress(
            object sender, KeyPressEventArgs e)
        {
            // Только цифры и Backspace
            if (!char.IsDigit(e.KeyChar) &&
                e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        // =========================================================
        // БЛОКИРОВКА СИМВОЛОВ В textBox5
        // =========================================================

        private void textBox5_KeyPress(
            object sender, KeyPressEventArgs e)
        {
            // Backspace разрешён
            if (e.KeyChar == '\b')
                return;

            // Цифры разрешены
            if (char.IsDigit(e.KeyChar))
                return;

            // Разрешаем один десятичный разделитель
            if ((e.KeyChar == '.' || e.KeyChar == ',') &&
                !textBox5.Text.Contains(".") &&
                !textBox5.Text.Contains(","))
            {
                return;
            }

            e.Handled = true;
        }

        // =========================================================
        // СОХРАНЕНИЕ ИЗМЕНЕНИЙ
        // =========================================================

        private void button1_Click(object sender, EventArgs e)
        {
            // Проверка названия
            if (string.IsNullOrWhiteSpace(textBox6.Text))
            {
                MessageBox.Show(
                    "Введите название товара.",
                    "Проверка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                textBox6.Focus();
                return;
            }

            // Проверка категории
            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show(
                    "Выберите категорию.",
                    "Проверка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                comboBox1.Focus();
                return;
            }

            // Проверка количества
            if (!int.TryParse(textBox4.Text, out int stock))
            {
                MessageBox.Show(
                    "Введите корректное количество товара.",
                    "Проверка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                textBox4.Focus();
                return;
            }

            // Проверка цены
            string priceText = textBox5.Text.Replace(',', '.');

            if (!decimal.TryParse(
                priceText,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out decimal price))
            {
                MessageBox.Show(
                    "Введите корректную цену.",
                    "Проверка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                textBox5.Focus();
                return;
            }

            if (price < 0)
            {
                MessageBox.Show(
                    "Цена не может быть отрицательной.",
                    "Проверка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                textBox5.Focus();
                return;
            }

            if (stock < 0)
            {
                MessageBox.Show(
                    "Количество не может быть отрицательным.",
                    "Проверка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                textBox4.Focus();
                return;
            }

            try
            {
                string connectionString =
                    DBconfig.BuildConnectionString();

                using (MySqlConnection connection =
                    new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        UPDATE Products
                        SET
                            Name = @Name,
                            Description = @Description,
                            CategoryID = @CategoryID,
                            StockQuantity = @StockQuantity,
                            Price = @Price
                        WHERE ProductID = @ProductID;
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@Name", textBox6.Text.Trim());

                        command.Parameters.AddWithValue(
                            "@Description",
                            string.IsNullOrWhiteSpace(textBox2.Text)
                            ? (object)DBNull.Value
                            : textBox2.Text.Trim());

                        command.Parameters.AddWithValue(
                            "@CategoryID",
                            Convert.ToInt32(comboBox1.SelectedValue));

                        command.Parameters.AddWithValue(
                            "@StockQuantity", stock);

                        command.Parameters.AddWithValue(
                            "@Price", price);

                        command.Parameters.AddWithValue(
                            "@ProductID", productID);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Данные товара успешно изменены.",
                    "Сохранение",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Ошибка при изменении товара:\n\n" +
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
        // ОТМЕНА
        // =========================================================

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}