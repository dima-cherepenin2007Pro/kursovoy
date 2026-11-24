using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using cherepeni_forms.Classes;

namespace cherepeni_forms.Forms.Sis_admin
{
    public partial class Statuses : Form
    {
        private DataTable statusesTable;

        public Statuses()
        {
            InitializeComponent();

            ConfigureDataGridView();
            LoadStatuses();

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
        // ЗАГРУЗКА СТАТУСОВ
        // =====================================================

        private void LoadStatuses()
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
                        SELECT StatusID, StatusName
                        FROM Statuses
                        ORDER BY StatusID;
                    ";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, connection))
                    {
                        statusesTable = new DataTable();

                        adapter.Fill(statusesTable);

                        dataGridView1.DataSource = statusesTable;
                    }
                }

                ConfigureColumnNames();
                UpdateStatusCount();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Ошибка при загрузке статусов:\n\n" +
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

        // =====================================================
        // НАЗВАНИЯ СТОЛБЦОВ
        // =====================================================

        private void ConfigureColumnNames()
        {
            if (dataGridView1.Columns.Count == 0)
                return;

            dataGridView1.Columns["StatusID"].HeaderText = "ID";

            dataGridView1.Columns["StatusName"].HeaderText =
                "Название статуса";

            dataGridView1.Columns["StatusID"].FillWeight = 30;
            dataGridView1.Columns["StatusName"].FillWeight = 170;
        }

        // =====================================================
        // КОЛИЧЕСТВО СТАТУСОВ
        // =====================================================

        private void UpdateStatusCount()
        {
            label4.Text =
                "Количество статусов: " +
                dataGridView1.Rows.Count;
        }

        // =====================================================
        // РЕДАКТИРОВАНИЕ СТАТУСА
        // =====================================================

        private void dataGridView1_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            int statusID = Convert.ToInt32(
                dataGridView1.Rows[e.RowIndex]
                    .Cells["StatusID"].Value
            );

            string statusName =
                dataGridView1.Rows[e.RowIndex]
                    .Cells["StatusName"].Value.ToString();

            DialogResult result = MessageBox.Show(
                "Вы хотите изменить выбранный статус?\n\n" +
                "Название статуса: " + statusName,
                "Редактирование статуса",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                OpenEditForm(statusID, statusName);
            }
        }

        // =====================================================
        // ОТКРЫТИЕ ФОРМЫ РЕДАКТИРОВАНИЯ
        // =====================================================

        private void OpenEditForm(
            int statusID,
            string statusName)
        {
            using (Status_edit editForm =
                new Status_edit(statusID, statusName))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadStatuses();
                }
            }
        }
    }
}