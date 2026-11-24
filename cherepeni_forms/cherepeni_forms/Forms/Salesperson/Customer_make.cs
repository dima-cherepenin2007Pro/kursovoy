using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using cherepeni_forms.Classes;

namespace cherepeni_forms.Forms.Salesperson
{
    public partial class Customer_make : Form
    {
        public Customer_make()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            maskedTextBox1.Mask = "+7 (000) 000-00-00";
            maskedTextBox1.PromptChar = '_';
            maskedTextBox1.TextMaskFormat =
                System.Windows.Forms.MaskFormat.ExcludePromptAndLiterals;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            // Получение данных из полей
            string firstName = textBox6.Text.Trim();
            string lastName = textBox2.Text.Trim();
            string middleName = textBox1.Text.Trim();
            string phone = maskedTextBox1.Text.Trim();
            string email = textBox4.Text.Trim();
            string address = textBox3.Text.Trim();
            DateTime birthDate = dateTimePicker1.Value.Date;

            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(address))
            {
                MessageBox.Show(
                    "Заполните все обязательные поля.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // Проверка телефона
            if (!maskedTextBox1.MaskCompleted)
            {
                MessageBox.Show(
                    "Введите номер телефона полностью.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // Получение только цифр телефона
            string phoneDigits = new string(
                phone.Where(char.IsDigit).ToArray()
            );

            // Проверка электронной почты
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (!Regex.IsMatch(email, emailPattern))
            {
                MessageBox.Show(
                    "Введите корректный адрес электронной почты.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // Проверка даты рождения
            if (birthDate > DateTime.Today)
            {
                MessageBox.Show(
                    "Дата рождения не может быть в будущем.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // SQL-запрос
            string query = @"
                INSERT INTO Customers
                (FirstName, MiddleName, LastName,
                 CustomerPhone, CustomerEmail,
                 CustomerAddress, CustomerBirthDate)
                VALUES
                (@FirstName, @MiddleName, @LastName,
                 @CustomerPhone, @CustomerEmail,
                 @CustomerAddress, @CustomerBirthDate)";

            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(DBconfig.BuildConnectionString()))
                {
                    connection.Open();

                    using (MySqlCommand cmd =
                        new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.Add("@FirstName",
                            MySqlDbType.VarChar).Value = firstName;

                        cmd.Parameters.Add("@MiddleName",
                            MySqlDbType.VarChar).Value =
                            string.IsNullOrWhiteSpace(middleName)
                            ? DBNull.Value
                            : (object)middleName;

                        cmd.Parameters.Add("@LastName",
                            MySqlDbType.VarChar).Value = lastName;

                        cmd.Parameters.Add("@CustomerPhone",
                            MySqlDbType.VarChar).Value = phoneDigits;

                        cmd.Parameters.Add("@CustomerEmail",
                            MySqlDbType.VarChar).Value = email;

                        cmd.Parameters.Add("@CustomerAddress",
                            MySqlDbType.VarChar).Value = address;

                        cmd.Parameters.Add("@CustomerBirthDate",
                            MySqlDbType.Date).Value = birthDate;

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Покупатель успешно добавлен!",
                    "Успех",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // Сообщаем предыдущей форме об успешном добавлении
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при добавлении покупателя:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}