using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using cherepeni_forms.Classes;

namespace cherepeni_forms.Forms.Sis_admin
{
    public partial class Roles_view : Form
    {
        private DataTable rolesTable;

        public Roles_view()
        {
            InitializeComponent();

            ConfigureDataGridView();
            LoadRoles();

            // Двойной щелчок по строке
            dataGridView1.CellDoubleClick +=
                dataGridView1_CellDoubleClick;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        // =====================================================
        // НАСТРОЙКА DATAGRIDVIEW
        // =====================================================

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
        }

        // =====================================================
        // ЗАГРУЗКА РОЛЕЙ
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

                    string query = @"
                        SELECT RoleID, RoleName
                        FROM Roles
                        ORDER BY RoleID;
                    ";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, connection))
                    {
                        rolesTable = new DataTable();

                        adapter.Fill(rolesTable);

                        dataGridView1.DataSource = rolesTable;
                    }
                }

                ConfigureColumnNames();
                UpdateRoleCount();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Ошибка при загрузке ролей:\n\n" +
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

        // =====================================================
        // НАЗВАНИЯ СТОЛБЦОВ
        // =====================================================

        private void ConfigureColumnNames()
        {
            if (dataGridView1.Columns.Count == 0)
                return;

            dataGridView1.Columns["RoleID"].HeaderText = "ID";
            dataGridView1.Columns["RoleName"].HeaderText =
                "Название роли";

            dataGridView1.Columns["RoleID"].FillWeight = 30;
            dataGridView1.Columns["RoleName"].FillWeight = 170;
        }

        // =====================================================
        // КОЛИЧЕСТВО РОЛЕЙ
        // =====================================================

        private void UpdateRoleCount()
        {
            label4.Text =
                "Количество ролей: " +
                dataGridView1.Rows.Count;
        }

        // =====================================================
        // РЕДАКТИРОВАНИЕ РОЛИ
        // =====================================================

        private void dataGridView1_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            int roleID = Convert.ToInt32(
                dataGridView1.Rows[e.RowIndex]
                    .Cells["RoleID"].Value);

            string roleName =
                dataGridView1.Rows[e.RowIndex]
                    .Cells["RoleName"].Value.ToString();

            DialogResult result = MessageBox.Show(
                "Вы хотите изменить выбранную роль?\n\n" +
                "Название роли: " + roleName,
                "Редактирование роли",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                OpenEditForm(roleID, roleName);
            }
        }

        private void OpenEditForm(
    int roleID,
    string roleName)
        {
            using (Role_edit editForm =
                new Role_edit(roleID, roleName))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadRoles();
                }
            }
        }
    }
}