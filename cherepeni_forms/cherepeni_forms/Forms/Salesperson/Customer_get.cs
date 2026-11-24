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

namespace cherepeni_forms.Forms.Salesperson
{
    public partial class Customer_get : Form
    {
        public int CustomerID { get; private set; }
        public string CustomerName { get; private set; }
        public Customer_get()
        {
            InitializeComponent();
            ConfigureDataGridView();

            dataGridView1.CellDoubleClick +=
                dataGridView1_CellDoubleClick;

            button1.Click += button1_Click;

            LoadCustomers();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void dataGridView1_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (!dataGridView1.Columns.Contains("StockQuantity"))
                return;

            DataGridViewRow row =
                dataGridView1.Rows[e.RowIndex];

            object value =
                row.Cells["StockQuantity"].Value;

            if (value == null || value == DBNull.Value)
                return;

            int stock = Convert.ToInt32(value);

            if (stock == 0)
            {
                row.DefaultCellStyle.BackColor =
                    Color.LightCoral;

                row.DefaultCellStyle.ForeColor =
                    Color.Black;
            }
            else
            {
                row.DefaultCellStyle.BackColor =
                    dataGridView1.DefaultCellStyle.BackColor;

                row.DefaultCellStyle.ForeColor =
                    dataGridView1.DefaultCellStyle.ForeColor;
            }
        }

        // =========================================================
        // DATAGRIDVIEW
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

            dataGridView1.CellFormatting +=
                dataGridView1_CellFormatting;
        }

        // =========================================================
        // ЗАГРУЗКА КЛИЕНТОВ
        // =========================================================

        private void LoadCustomers()
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
                            CustomerID,
                            FirstName,
                            MiddleName,
                            LastName,
                            CustomerPhone,
                            CustomerEmail,
                            CustomerAddress,
                            CustomerBirthDate
                        FROM Customers
                        ORDER BY LastName, FirstName;
                    ";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, connection))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        dataGridView1.DataSource = table;
                    }
                }

                SetColumnNames();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки клиентов:\n\n" +
                    ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // НАЗВАНИЯ СТОЛБЦОВ
        // =========================================================

        private void SetColumnNames()
        {
            if (dataGridView1.Columns.Count == 0)
                return;

            dataGridView1.Columns["CustomerID"].HeaderText = "ID";
            dataGridView1.Columns["FirstName"].HeaderText = "Имя";
            dataGridView1.Columns["MiddleName"].HeaderText = "Отчество";
            dataGridView1.Columns["LastName"].HeaderText = "Фамилия";
            dataGridView1.Columns["CustomerPhone"].HeaderText = "Телефон";
            dataGridView1.Columns["CustomerEmail"].HeaderText = "Email";
            dataGridView1.Columns["CustomerAddress"].HeaderText = "Адрес";
            dataGridView1.Columns["CustomerBirthDate"].HeaderText = "Дата рождения";
        }

        // =========================================================
        // ВЫБОР КЛИЕНТА
        // =========================================================

        private void dataGridView1_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            SelectCustomer();
        }

        private void SelectCustomer()
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите клиента.");
                return;
            }

            DataGridViewRow row =
                dataGridView1.SelectedRows[0];

            CustomerID = Convert.ToInt32(
                row.Cells["CustomerID"].Value);

            string firstName =
                row.Cells["FirstName"].Value.ToString();

            string middleName = "";

            if (row.Cells["MiddleName"].Value != DBNull.Value)
            {
                middleName =
                    row.Cells["MiddleName"].Value.ToString();
            }

            string lastName =
                row.Cells["LastName"].Value.ToString();

            CustomerName =
                lastName + " " + firstName + " " + middleName;

            DialogResult = DialogResult.OK;
            Close();
        }

        // =========================================================
        // ДОБАВЛЕНИЕ КЛИЕНТА
        // =========================================================

        private void button1_Click(object sender, EventArgs e)
        {
            using (Customer_make form = new Customer_make())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadCustomers();
                }
            }
        }
    }
}