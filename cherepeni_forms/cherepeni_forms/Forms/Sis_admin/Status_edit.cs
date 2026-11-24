using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using cherepeni_forms.Classes;

namespace cherepeni_forms.Forms.Sis_admin
{
    public partial class Status_edit : Form
    {
        private int statusID;

        public Status_edit(int statusID, string statusName)
        {
            InitializeComponent();

            this.statusID = statusID;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            textBox1.Text = statusName;

            button1.Click += button1_Click;
            button2.Click += button2_Click;
        }

        // =====================================================
        // КНОПКА "СОХРАНИТЬ ИЗМЕНЕНИЯ"
        // =====================================================

        private void button1_Click(object sender, EventArgs e)
        {
            string statusName = textBox1.Text.Trim();

            // Проверка названия статуса
            if (string.IsNullOrWhiteSpace(statusName))
            {
                MessageBox.Show(
                    "Введите название статуса.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                textBox1.Focus();
                return;
            }

            // Сохранение
            UpdateStatus(statusName);
        }

        // =====================================================
        // ОБНОВЛЕНИЕ СТАТУСА
        // =====================================================

        private void UpdateStatus(string statusName)
        {
            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(DBconfig.BuildConnectionString()))
                {
                    connection.Open();

                    // Проверка уникальности названия статуса
                    string checkQuery = @"
                        SELECT COUNT(*)
                        FROM Statuses
                        WHERE StatusName = @StatusName
                          AND StatusID <> @StatusID;
                    ";

                    using (MySqlCommand checkCommand =
                        new MySqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue(
                            "@StatusName",
                            statusName
                        );

                        checkCommand.Parameters.AddWithValue(
                            "@StatusID",
                            statusID
                        );

                        int count = Convert.ToInt32(
                            checkCommand.ExecuteScalar()
                        );

                        if (count > 0)
                        {
                            MessageBox.Show(
                                "Такой статус уже существует.",
                                "Дублирование статуса",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );

                            textBox1.Focus();
                            return;
                        }
                    }

                    // Обновление названия статуса
                    string query = @"
                        UPDATE Statuses
                        SET StatusName = @StatusName
                        WHERE StatusID = @StatusID;
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@StatusName",
                            statusName
                        );

                        command.Parameters.AddWithValue(
                            "@StatusID",
                            statusID
                        );

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Название статуса успешно изменено.",
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
                        "Такой статус уже существует.",
                        "Дублирование статуса",
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
        // КНОПКА "ОТМЕНА"
        // =====================================================

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}