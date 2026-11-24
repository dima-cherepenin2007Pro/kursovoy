using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using cherepeni_forms.Classes;
using MySql.Data.MySqlClient;

namespace cherepeni_forms.Forms.Manager
{
    public partial class Product_add : Form
    {
        public Product_add()
        {
            InitializeComponent();

            // Обработчики событий
            textBox4.KeyPress += textBox4_KeyPress;
            textBox5.KeyPress += textBox5_KeyPress;

            button1.Click += button1_Click;
            button2.Click += button2_Click;

            // Загрузка категорий
            LoadCategories();

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
        // БЛОКИРОВКА СИМВОЛОВ В textBox4
        // =========================================================

        private void textBox4_KeyPress(
            object sender, KeyPressEventArgs e)
        {
            // Разрешаем только цифры и Backspace
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

            // Разрешаем одну точку или запятую
            if ((e.KeyChar == '.' || e.KeyChar == ',') &&
                !textBox5.Text.Contains(".") &&
                !textBox5.Text.Contains(","))
            {
                return;
            }

            e.Handled = true;
        }

        // =========================================================
        // ДОБАВЛЕНИЕ ТОВАРА
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

            // =====================================================
            // СОХРАНЕНИЕ В БД
            // =====================================================

            try
            {
                string connectionString =
                    DBconfig.BuildConnectionString();

                using (MySqlConnection connection =
                    new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        INSERT INTO Products
                        (
                            Name,
                            Description,
                            CategoryID,
                            StockQuantity,
                            Price
                        )
                        VALUES
                        (
                            @Name,
                            @Description,
                            @CategoryID,
                            @StockQuantity,
                            @Price
                        );
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

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Товар успешно добавлен.",
                    "Добавление",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Ошибка при добавлении товара:\n\n" +
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