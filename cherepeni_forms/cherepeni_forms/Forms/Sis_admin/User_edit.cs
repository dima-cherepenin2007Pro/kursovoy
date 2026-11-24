using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using cherepeni_forms.Classes;

namespace cherepeni_forms.Forms.Sis_admin
{
    public partial class User_edit : Form
    {
        private int userID;

        public User_edit(int userID)
        {
            InitializeComponent();

            this.userID = userID;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            textBox6.KeyPress += NameTextBox_KeyPress;
            textBox2.KeyPress += NameTextBox_KeyPress;

            textBox6.TextChanged += NameTextBox_TextChanged;
            textBox2.TextChanged += NameTextBox_TextChanged;

            button1.Click += button1_Click;
            button2.Click += button2_Click;

            textBox5.UseSystemPasswordChar = true;

            LoadRoles();
            LoadUserData();
        }

        // =====================================================
        // Загрузка ролей
        // =====================================================

        private void LoadRoles()
        {
            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(DBconfig.BuildConnectionString()))
                {
                    connection.Open();

                    string query =
                        "SELECT RoleID, RoleName FROM Roles";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, connection))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        comboBox1.DataSource = table;
                        comboBox1.DisplayMember = "RoleName";
                        comboBox1.ValueMember = "RoleID";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки ролей:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // =====================================================
        // Загрузка данных пользователя
        // =====================================================

        private void LoadUserData()
        {
            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(DBconfig.BuildConnectionString()))
                {
                    connection.Open();

                    string query = @"
                        SELECT FirstName, LastName, RoleID, Login
                        FROM Users
                        WHERE UserID = @UserID;
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@UserID",
                            userID
                        );

                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                textBox6.Text =
                                    reader["FirstName"].ToString();

                                textBox2.Text =
                                    reader["LastName"].ToString();

                                textBox4.Text =
                                    reader["Login"].ToString();

                                comboBox1.SelectedValue =
                                    Convert.ToInt32(reader["RoleID"]);
                            }
                            else
                            {
                                MessageBox.Show(
                                    "Пользователь не найден.",
                                    "Ошибка",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );

                                this.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки пользователя:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // =====================================================
        // Ограничение ввода имени и фамилии
        // =====================================================

        private void NameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
                return;

            if (!char.IsLetter(e.KeyChar) &&
                e.KeyChar != '-' &&
                e.KeyChar != ' ')
            {
                e.Handled = true;
            }
        }

        // =====================================================
        // Заглавная первая буква
        // =====================================================

        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox == null || textBox.Text.Length == 0)
                return;

            int cursorPosition = textBox.SelectionStart;

            string text = textBox.Text;
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
            return Regex.IsMatch(
                value.Trim(),
                @"^[\p{L}]+(?:[- ][\p{L}]+)*$"
            );
        }

        // =====================================================
        // Кнопка "Сохранить изменения"
        // =====================================================

        private void button1_Click(object sender, EventArgs e)
        {
            string firstName = textBox6.Text.Trim();
            string lastName = textBox2.Text.Trim();
            string login = textBox4.Text.Trim();
            string password = textBox5.Text;

            // Проверка имени
            if (string.IsNullOrWhiteSpace(firstName) ||
                !IsValidName(firstName))
            {
                MessageBox.Show(
                    "Введите корректное имя.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                textBox6.Focus();
                return;
            }

            // Проверка фамилии
            if (string.IsNullOrWhiteSpace(lastName) ||
                !IsValidName(lastName))
            {
                MessageBox.Show(
                    "Введите корректную фамилию.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                textBox2.Focus();
                return;
            }

            // Проверка роли
            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Выберите роль.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                comboBox1.Focus();
                return;
            }

            // Проверка логина
            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show(
                    "Введите логин.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                textBox4.Focus();
                return;
            }

            // Сохранение
            UpdateUser(
                firstName,
                lastName,
                Convert.ToInt32(comboBox1.SelectedValue),
                login,
                password
            );
        }

        // =====================================================
        // Обновление пользователя
        // =====================================================

        private void UpdateUser(
            string firstName,
            string lastName,
            int roleID,
            string login,
            string password)
        {
            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(DBconfig.BuildConnectionString()))
                {
                    connection.Open();

                    // Проверка уникальности логина
                    string checkQuery = @"
                        SELECT COUNT(*)
                        FROM Users
                        WHERE Login = @login
                          AND UserID <> @UserID;
                    ";

                    using (MySqlCommand checkCommand =
                        new MySqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue(
                            "@login",
                            login
                        );

                        checkCommand.Parameters.AddWithValue(
                            "@UserID",
                            userID
                        );

                        int count = Convert.ToInt32(
                            checkCommand.ExecuteScalar()
                        );

                        if (count > 0)
                        {
                            MessageBox.Show(
                                "Этот логин уже занят другим пользователем.",
                                "Дублирование логина",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );

                            textBox4.Focus();
                            return;
                        }
                    }

                    // Если пароль пуст, старый пароль сохраняется
                    string query;

                    if (string.IsNullOrWhiteSpace(password))
                    {
                        query = @"
                            UPDATE Users
                            SET FirstName = @FirstName,
                                LastName = @LastName,
                                RoleID = @RoleID,
                                Login = @Login
                            WHERE UserID = @UserID;
                        ";
                    }
                    else
                    {
                        query = @"
                            UPDATE Users
                            SET FirstName = @FirstName,
                                LastName = @LastName,
                                RoleID = @RoleID,
                                Login = @Login,
                                Password = @Password
                            WHERE UserID = @UserID;
                        ";
                    }

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@FirstName",
                            firstName
                        );

                        command.Parameters.AddWithValue(
                            "@LastName",
                            lastName
                        );

                        command.Parameters.AddWithValue(
                            "@RoleID",
                            roleID
                        );

                        command.Parameters.AddWithValue(
                            "@Login",
                            login
                        );

                        command.Parameters.AddWithValue(
                            "@UserID",
                            userID
                        );

                        if (!string.IsNullOrWhiteSpace(password))
                        {
                            command.Parameters.AddWithValue(
                                "@Password",
                                password
                            );
                        }

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Данные пользователя успешно изменены.",
                    "Успешно",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    MessageBox.Show(
                        "Этот логин уже существует.",
                        "Дублирование логина",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
                else
                {
                    MessageBox.Show(
                        "Ошибка базы данных:\n" + ex.Message,
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Произошла ошибка:\n" + ex.Message,
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