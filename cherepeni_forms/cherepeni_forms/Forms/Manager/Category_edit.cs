using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using cherepeni_forms.Classes;
using MySql.Data.MySqlClient;

namespace cherepeni_forms.Forms.Manager
{
    public partial class Category_edit : Form
    {
        private int categoryID;
        public Category_edit(int categoryID, string categoryName)
        {
            InitializeComponent();

            this.categoryID = categoryID;
            textBox6.Text = categoryName;

            button1.Click += button1_Click;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string categoryName = textBox6.Text.Trim();

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                MessageBox.Show(
                    "Введите название категории!",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                textBox6.Focus();
                return;
            }

            try
            {
                string connectionString = DBconfig.BuildConnectionString();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверяем, есть ли другая категория с таким названием
                    string checkQuery = @"
                        SELECT COUNT(*)
                        FROM Categories
                        WHERE CategoryName = @CategoryName
                        AND CategoryID <> @CategoryID;
                    ";

                    using (MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@CategoryName", categoryName);
                        checkCommand.Parameters.AddWithValue("@CategoryID", categoryID);

                        int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show(
                                "Категория с таким названием уже существует!",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                            textBox6.Focus();
                            return;
                        }
                    }

                    // Обновляем категорию
                    string updateQuery = @"
                        UPDATE Categories
                        SET CategoryName = @CategoryName
                        WHERE CategoryID = @CategoryID;
                    ";

                    using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@CategoryName", categoryName);
                        updateCommand.Parameters.AddWithValue("@CategoryID", categoryID);

                        updateCommand.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Категория успешно изменена!",
                    "Успех",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Ошибка базы данных:\n\n" + ex.Message,
                    "Ошибка БД",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Произошла ошибка:\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}