using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using cherepeni_forms.Classes;

namespace cherepeni_forms.Forms.Sis_admin
{
    public partial class User_add : Form
    {
        public User_add()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // События для имени и фамилии
            textBox6.KeyPress += NameTextBox_KeyPress;
            textBox2.KeyPress += NameTextBox_KeyPress;

            textBox6.TextChanged += NameTextBox_TextChanged;
            textBox2.TextChanged += NameTextBox_TextChanged;

            // События кнопок
            button1.Click += button1_Click;
            button2.Click += button2_Click;

            // Загрузка ролей
            LoadRoles();
        }

        // =====================================================
        // Загрузка ролей из базы данных
        // =====================================================

        private void LoadRoles()
        {
            try
            {
                string connectionString =
                    DBconfig.BuildConnectionString();

                using (MySqlConnection connection =
                    new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT RoleID, RoleName FROM Roles";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, connection))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        comboBox1.DataSource = table;
                        comboBox1.DisplayMember = "RoleName";
                        comboBox1.ValueMember = "RoleID";

                        comboBox1.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки ролей:\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // =====================================================
        // Запрет цифр и посторонних символов
        // =====================================================

        private void NameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем управляющие символы
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Разрешаем буквы, дефис и пробел
            if (!char.IsLetter(e.KeyChar) &&
                e.KeyChar != '-' &&
                e.KeyChar != ' ')
            {
                e.Handled = true;
            }
        }

        // =====================================================
        // Автоматическая заглавная первая буква
        // =====================================================

        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox == null || textBox.Text.Length == 0)
            {
                return;
            }

            int cursorPosition = textBox.SelectionStart;

            string text = textBox.Text;

            // Заглавная первая буква
            text = char.ToUpper(text[0]) + text.Substring(1);

            if (textBox.Text != text)
            {
                textBox.Text = text;

                textBox.SelectionStart = Math.Min(
                    cursorPosition,
                    textBox.Text.Length
                );
            }
        }

        // =====================================================
        // Проверка имени и фамилии
        // =====================================================

        private bool IsValidName(string value)
        {
            value = value.Trim();

            // Только буквы, дефис и пробел
            // Имя не должно начинаться или заканчиваться
            // дефисом либо пробелом
            return Regex.IsMatch(
                value,
                @"^[\p{L}]+(?:[- ][\p{L}]+)*$"
            );
        }

        // =====================================================
        // Кнопка "Добавить"
        // =====================================================

        private void button1_Click(object sender, EventArgs e)
        {
            string firstName = textBox6.Text.Trim();
            string lastName = textBox2.Text.Trim();
            string login = textBox4.Text.Trim();
            string password = textBox5.Text;

            // -----------------------------
            // Проверка имени
            // -----------------------------

            if (string.IsNullOrWhiteSpace(firstName) ||
                !IsValidName(firstName))
            {
                MessageBox.Show(
                    "Введите корректное имя.\n\n" +
                    "Разрешены только буквы, пробелы и дефис.",
                    "Ошибка ввода",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                textBox6.Focus();
                return;
            }

            // -----------------------------
            // Проверка фамилии
            // -----------------------------

            if (string.IsNullOrWhiteSpace(lastName) ||
                !IsValidName(lastName))
            {
                MessageBox.Show(
                    "Введите корректную фамилию.\n\n" +
                    "Разрешены только буквы, пробелы и дефис.",
                    "Ошибка ввода",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                textBox2.Focus();
                return;
            }

            // -----------------------------
            // Проверка роли
            // -----------------------------

            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Выберите роль пользователя.",
                    "Ошибка ввода",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                comboBox1.Focus();
                return;
            }

            // -----------------------------
            // Проверка логина
            // -----------------------------

            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show(
                    "Введите логин.",
                    "Ошибка ввода",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                textBox4.Focus();
                return;
            }

            // -----------------------------
            // Проверка пароля
            // -----------------------------

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Введите пароль.",
                    "Ошибка ввода",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                textBox5.Focus();
                return;
            }

            // -----------------------------
            // Добавление пользователя
            // -----------------------------

            AddUser(
                firstName,
                lastName,
                Convert.ToInt32(comboBox1.SelectedValue),
                login,
                password
            );
        }

        // =====================================================
        // Добавление пользователя в БД
        // =====================================================

        private void AddUser(
            string firstName,
            string lastName,
            int roleID,
            string login,
            string password)
        {
            try
            {
                string connectionString =
                    DBconfig.BuildConnectionString();

                using (MySqlConnection connection =
                    new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // -----------------------------
                    // Проверка уникальности логина
                    // -----------------------------

                    string checkQuery = @"
                        SELECT COUNT(*)
                        FROM Users
                        WHERE Login = @login;
                    ";

                    using (MySqlCommand checkCommand =
                        new MySqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue(
                            "@login",
                            login
                        );

                        int count = Convert.ToInt32(
                            checkCommand.ExecuteScalar()
                        );

                        if (count > 0)
                        {
                            MessageBox.Show(
                                "Пользователь с таким логином уже существует.",
                                "Дублирование логина",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );

                            textBox4.Focus();
                            return;
                        }
                    }

                    // -----------------------------
                    // Добавление пользователя
                    // -----------------------------

                    string insertQuery = @"
                        INSERT INTO Users
                        (
                            FirstName,
                            LastName,
                            RoleID,
                            Login,
                            Password
                        )
                        VALUES
                        (
                            @firstName,
                            @lastName,
                            @roleID,
                            @login,
                            @password
                        );
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@firstName",
                            firstName
                        );

                        command.Parameters.AddWithValue(
                            "@lastName",
                            lastName
                        );

                        command.Parameters.AddWithValue(
                            "@roleID",
                            roleID
                        );

                        command.Parameters.AddWithValue(
                            "@login",
                            login
                        );

                        command.Parameters.AddWithValue(
                            "@password",
                            password
                        );

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Пользователь успешно добавлен!",
                    "Успешно",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (MySqlException ex)
            {
                // Ошибка уникальности логина в БД
                if (ex.Number == 1062)
                {
                    MessageBox.Show(
                        "Такой логин уже существует.",
                        "Дублирование логина",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    textBox4.Focus();
                }
                else
                {
                    MessageBox.Show(
                        "Ошибка базы данных:\n\n" + ex.Message,
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Произошла ошибка:\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // =====================================================
        // Кнопка "Отмена"
        // =====================================================

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}