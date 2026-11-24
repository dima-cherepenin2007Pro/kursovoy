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

namespace cherepeni_forms.Forms.Sis_admin
{
    public partial class Users_view : Form
    {
        private DataTable usersTable;

        public Users_view()
        {
            InitializeComponent();

            // Настройка DataGridView
            ConfigureDataGridView();

            // Заполнение ComboBox
            LoadSortCombo();
            LoadFilterCombo();

            // Загрузка пользователей
            LoadUsers();

            // Обработчики кнопок
            SortButton.Click += SortButton_Click;
            FilterButton.Click += FilterButton_Click;
            AddUserButton.Click += AddUserButton_Click;
            RefreshButton.Click += RefreshButton_Click;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            label1.Visible = false;
            SortCombo.Visible = false;
            SortButton.Visible = false;
            label3.Visible = false;
            FilterCombo.Visible = false;
            FilterButton.Visible = false;
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

            // Маскировка имени и фамилии
            dataGridView1.CellFormatting +=
                dataGridView1_CellFormatting;
        }

        // =========================================================
        // ЗАГРУЗКА ПОЛЬЗОВАТЕЛЕЙ
        // =========================================================

        private void LoadUsers()
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
                            u.UserID,
                            u.FirstName,
                            u.LastName,
                            r.RoleName,
                            u.Login
                        FROM Users u
                        INNER JOIN Roles r
                            ON u.RoleID = r.RoleID
                        ORDER BY u.UserID;
                    ";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, connection))
                    {
                        usersTable = new DataTable();

                        adapter.Fill(usersTable);

                        dataGridView1.DataSource = usersTable;
                    }
                }

                ConfigureColumnNames();
                UpdateUserCount();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Ошибка при загрузке пользователей:\n\n" +
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

            dataGridView1.Columns["UserID"].HeaderText =
                "ID";

            dataGridView1.Columns["FirstName"].HeaderText =
                "Имя";

            dataGridView1.Columns["LastName"].HeaderText =
                "Фамилия";

            dataGridView1.Columns["RoleName"].HeaderText =
                "Роль";

            dataGridView1.Columns["Login"].HeaderText =
                "Логин";

            // ID можно сделать немного уже
            dataGridView1.Columns["UserID"].FillWeight = 40;

            dataGridView1.Columns["FirstName"].FillWeight = 100;
            dataGridView1.Columns["LastName"].FillWeight = 120;
            dataGridView1.Columns["RoleName"].FillWeight = 160;
            dataGridView1.Columns["Login"].FillWeight = 100;
        }

        // =========================================================
        // ПОДСЧЁТ ПОЛЬЗОВАТЕЛЕЙ
        // =========================================================

        private void UpdateUserCount()
        {
            int count = dataGridView1.Rows.Count;

            txtCount.Text = "Пользователей: " + count;
        }


        // =========================================================
        // СОРТИРОВКА
        // =========================================================

        private void LoadSortCombo()
        {
            SortCombo.Items.Clear();

            SortCombo.Items.Add("По ID");
            SortCombo.Items.Add("Имя А-Я");
            SortCombo.Items.Add("Имя Я-А");
            SortCombo.Items.Add("Фамилия А-Я");
            SortCombo.Items.Add("Фамилия Я-А");
            SortCombo.Items.Add("Роль А-Я");
            SortCombo.Items.Add("Роль Я-А");
            SortCombo.Items.Add("Логин А-Я");
            SortCombo.Items.Add("Логин Я-А");

            SortCombo.SelectedIndex = 0;
        }

        private void SortButton_Click(object sender, EventArgs e)
        {
            if (usersTable == null)
                return;

            string sort = "";

            switch (SortCombo.SelectedIndex)
            {
                case 0:
                    sort = "UserID ASC";
                    break;

                case 1:
                    sort = "FirstName ASC";
                    break;

                case 2:
                    sort = "FirstName DESC";
                    break;

                case 3:
                    sort = "LastName ASC";
                    break;

                case 4:
                    sort = "LastName DESC";
                    break;

                case 5:
                    sort = "RoleName ASC";
                    break;

                case 6:
                    sort = "RoleName DESC";
                    break;

                case 7:
                    sort = "Login ASC";
                    break;

                case 8:
                    sort = "Login DESC";
                    break;
            }

            usersTable.DefaultView.Sort = sort;

            UpdateUserCount();
        }

        // =========================================================
        // ФИЛЬТРАЦИЯ
        // =========================================================

        private void LoadFilterCombo()
        {
            FilterCombo.Items.Clear();

            FilterCombo.Items.Add("Все пользователи");
            FilterCombo.Items.Add("Системные администраторы");
            FilterCombo.Items.Add("Продавцы");
            FilterCombo.Items.Add("Менеджеры");

            FilterCombo.SelectedIndex = 0;
        }

        private void FilterButton_Click(object sender, EventArgs e)
        {
            if (usersTable == null)
                return;

            string filter = "";

            switch (FilterCombo.SelectedIndex)
            {
                case 0:
                    filter = "";
                    break;

                case 1:
                    filter =
                        "RoleName = 'Системный администратор'";
                    break;

                case 2:
                    filter =
                        "RoleName = 'Продавец'";
                    break;

                case 3:
                    filter =
                        "RoleName = 'Менеджер'";
                    break;
            }

            usersTable.DefaultView.RowFilter = filter;

            UpdateUserCount();
        }

        // =========================================================
        // ДОБАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯ
        // =========================================================

        private void AddUserButton_Click(object sender, EventArgs e)
        {
            using (User_add form = new User_add())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadUsers();
                }
                RefreshButton_Click(sender, e);
            }
        }

        // =========================================================
        // ОБНОВЛЕНИЕ
        // =========================================================

        private void RefreshButton_Click(object sender, EventArgs e)
        {

            SortCombo.SelectedIndex = 0;
            FilterCombo.SelectedIndex = 0;

            LoadUsers();
        }

        private void dataGridView1_CellDoubleClick(
    object sender,
    DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            int userID = Convert.ToInt32(
                dataGridView1.Rows[e.RowIndex].Cells["UserID"].Value
            );

            string firstName = dataGridView1.Rows[e.RowIndex]
                .Cells["FirstName"].Value.ToString();

            string lastName = dataGridView1.Rows[e.RowIndex]
                .Cells["LastName"].Value.ToString();

            DialogResult result = MessageBox.Show(
                "Выберите действие для пользователя:\n\n" +
                firstName + " " + lastName + "\n\n" +
                "ДА — изменить пользователя\n" +
                "НЕТ — удалить пользователя\n" +
                "ОТМЕНА — закрыть окно",
                "Действия с пользователем",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                // Редактирование
                OpenEditForm(userID);
            }
            else if (result == DialogResult.No)
            {
                // Удаление
                DeleteUser(userID, firstName, lastName);
            }
        }
        private void OpenEditForm(int userID)
        {
            using (User_edit form = new User_edit(userID))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadUsers();
                }
            }
        }
        private void DeleteUser(
    int userID,
    string firstName,
    string lastName)
        {
            DialogResult confirm = MessageBox.Show(
                "Вы действительно хотите удалить пользователя?\n\n" +
                firstName + " " + lastName,
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

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
                DELETE FROM Users
                WHERE UserID = @UserID;
            ";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@UserID",
                            userID
                        );

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Пользователь успешно удалён.",
                    "Удаление",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // Обновляем таблицу
                LoadUsers();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Не удалось удалить пользователя.\n\n" +
                    "Причина:\n" +
                    ex.Message,
                    "Ошибка БД",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Произошла ошибка:\n\n" +
                    ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private void dataGridView1_CellFormatting(
    object sender,
    DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName =
                dataGridView1.Columns[e.ColumnIndex].Name;

            if (columnName == "FirstName" ||
                columnName == "LastName")
            {
                if (e.Value != null &&
                    e.Value != DBNull.Value)
                {
                    string name = e.Value.ToString();

                    if (name.Length > 1)
                    {
                        e.Value = name.Substring(0, 1) +
                            new string('*', name.Length - 1);
                    }

                    e.FormattingApplied = true;
                }
            }
        }
    }
}
