using System;
using System.Windows.Forms;
using cherepeni_forms.Classes;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace cherepeni_forms.Forms.Sis_admin
{
    public partial class Role_edit : Form
    {
        private int roleID;

        public Role_edit(int roleID, string roleName)
        {
            InitializeComponent();

            this.roleID = roleID;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            textBox1.Text = roleName;

            button1.Click += button1_Click;
            button2.Click += button2_Click;
        }

        // =====================================================
        // КНОПКА "СОХРАНИТЬ ИЗМЕНЕНИЯ"
        // =====================================================

        private void button1_Click(object sender, EventArgs e)
        {
            string roleName = textBox1.Text.Trim();

            // Проверка названия роли
            if (string.IsNullOrWhiteSpace(roleName))
            {
                MessageBox.Show(
                    "Введите название роли.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                textBox1.Focus();
                return;
            }

            // Сохранение
            UpdateRole(roleName);
        }

        // =====================================================
        // ОБНОВЛЕНИЕ РОЛИ
        // =====================================================

        private void UpdateRole(string roleName)
        {
            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(DBconfig.BuildConnectionString()))
                {
                    connection.Open();

                    // Проверка уникальности названия роли
                    string checkQuery = @"
                        SELECT COUNT(*)
                        FROM Roles
                        WHERE RoleName = @RoleName
                          AND RoleID <> @RoleID;
                    ";

                    using (MySqlCommand checkCommand =
                        new MySqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue(
                            "@RoleName",
                            roleName
                        );

                        checkCommand.Parameters.AddWithValue(
                            "@RoleID",
                            roleID
                        );

                        int count = Convert.ToInt32(
                            checkCommand.ExecuteScalar()
                        );

                        if (count > 0)
                        {
                            MessageBox.Show(
                                "Такая роль уже существует.",
                                "Дублирование роли",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );

                            textBox1.Focus();
                            return;
                        }
                    }

                    // Обновление названия роли
                    string query = @"
                        UPDATE Roles
                        SET RoleName = @RoleName
                        WHERE RoleID = @RoleID;
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@RoleName",
                            roleName
                        );

                        command.Parameters.AddWithValue(
                            "@RoleID",
                            roleID
                        );

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Название роли успешно изменено.",
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
                        "Такая роль уже существует.",
                        "Дублирование роли",
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