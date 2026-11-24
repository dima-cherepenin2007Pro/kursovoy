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
    public partial class Category_view : Form
    {
        private DataTable categoriesTable;
        public Category_view()
        {
            InitializeComponent();

            ConfigureDataGridView();
            LoadCategories();

            // Двойной щелчок по строке
            dataGridView1.CellDoubleClick +=
                dataGridView1_CellDoubleClick;

            // Кнопка добавления
            button2.Click += button2_Click;

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
        // ЗАГРУЗКА КАТЕГОРИЙ
        // =====================================================

        private void LoadCategories()
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
                        SELECT CategoryID, CategoryName
                        FROM Categories
                        ORDER BY CategoryID;
                    ";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, connection))
                    {
                        categoriesTable = new DataTable();

                        adapter.Fill(categoriesTable);

                        dataGridView1.DataSource = categoriesTable;
                    }
                }

                ConfigureColumnNames();
                UpdateCategoryCount();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Ошибка при загрузке категорий:\n\n" +
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

            dataGridView1.Columns["CategoryID"].HeaderText = "ID";

            dataGridView1.Columns["CategoryName"].HeaderText =
                "Название категории";

            dataGridView1.Columns["CategoryID"].FillWeight = 30;

            dataGridView1.Columns["CategoryName"].FillWeight = 170;
        }

        // =====================================================
        // КОЛИЧЕСТВО КАТЕГОРИЙ
        // =====================================================

        private void UpdateCategoryCount()
        {
            label4.Text =
                "Количество категорий: " +
                dataGridView1.Rows.Count;
        }

        // =====================================================
        // ДОБАВЛЕНИЕ КАТЕГОРИИ
        // =====================================================

        private void button2_Click(object sender, EventArgs e)
        {
            using (Category_add addForm = new Category_add())
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    LoadCategories();
                }
            }
        }

        // =====================================================
        // РЕДАКТИРОВАНИЕ КАТЕГОРИИ
        // =====================================================

        private void dataGridView1_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            int categoryID = Convert.ToInt32(
                dataGridView1.Rows[e.RowIndex]
                    .Cells["CategoryID"].Value);

            string categoryName =
                dataGridView1.Rows[e.RowIndex]
                    .Cells["CategoryName"].Value.ToString();

            DialogResult result = MessageBox.Show(
                "Вы хотите изменить выбранную категорию?\n\n" +
                "Название категории: " + categoryName,
                "Редактирование категории",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                OpenEditForm(categoryID, categoryName);
            }
        }

        // =====================================================
        // ОТКРЫТИЕ ФОРМЫ РЕДАКТИРОВАНИЯ
        // =====================================================

        private void OpenEditForm(
            int categoryID,
            string categoryName)
        {
            using (Category_edit editForm =
                new Category_edit(categoryID, categoryName))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadCategories();
                }
            }
        }
    }
}