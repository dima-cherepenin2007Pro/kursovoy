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
using cherepeni_forms.Properties;

namespace cherepeni_forms.Services
{
    public partial class Connection : Form
    {
        public Connection()
        {
            InitializeComponent();
            LoadSettings();
        }
        private void LoadSettings()
        {
            txtHost.Text = Settings.Default.DbHost;
            txtPort.Text = Settings.Default.DbPort;
            txtUser.Text = Settings.Default.DbUser;
            txtPassword.Text = Settings.Default.DbPassword;
            txtDatabase.Text = Settings.Default.DbName;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtHost.Text) ||
                string.IsNullOrWhiteSpace(txtUser.Text) ||
                string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                MessageBox.Show("Заполните обязательные поля: хост, логин, имя БД.",
                    "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Сохраняем во временные переменные, чтобы не портить настройки при ошибке
            Settings.Default.DbHost = txtHost.Text.Trim();
            Settings.Default.DbPort = string.IsNullOrWhiteSpace(txtPort.Text) ? "3306" : txtPort.Text.Trim();
            Settings.Default.DbUser = txtUser.Text.Trim();
            Settings.Default.DbPassword = txtPassword.Text;
            Settings.Default.DbName = txtDatabase.Text.Trim();
            Settings.Default.Save();

            // Проверяем, что с новыми настройками можно подключиться
            string connStr;
            string error = DBconfig.TestConnection(out connStr);
            if (error != null)
            {
                MessageBox.Show(
                    "Подключиться с указанными параметрами не удалось:\n" + error,
                    "Ошибка подключения",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // остаёмся на форме, пользователь исправит
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
