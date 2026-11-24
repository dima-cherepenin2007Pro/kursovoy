using System;
using System.Windows.Forms;
using cherepeni_forms.Classes;
using cherepeni_forms.Services;

namespace cherepeni_forms
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1. Проверяем подключение при старте
            string connStr;
            string error = DBconfig.TestConnection(out connStr);

            if (error != null)
            {
                // 2. Сообщаем пользователю о сбое
                var result = MessageBox.Show(
                    "Не удалось подключиться к базе данных.\n\n" +
                    "Причина: " + error + "\n\n" +
                    "Открыть форму настройки подключения?",
                    "Ошибка подключения к СУБД",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                {
                    // 3. Открываем форму настроек
                    using (var connForm = new Connection())
                    {
                        if (connForm.ShowDialog() != DialogResult.OK)
                            return;
                    }

                    // Повторная проверка подключения
                    error = DBconfig.TestConnection(out connStr);

                    if (error != null)
                    {
                        MessageBox.Show(
                            "Повторное подключение не удалось:\n" + error,
                            "Ошибка",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            // 4. Сообщение об успешном подключении
            MessageBox.Show(
                "Подключение к СУБД успешно выполнено.",
                "Успешное подключение",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // 5. Подключаем автоматическое резервное копирование
            Application.ApplicationExit += Application_ApplicationExit;

            // 6. Запускаем авторизацию
            Application.Run(new Authorization());
        }

        // Автоматическое резервное копирование при завершении приложения
        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            try
            {
                AutoBackup.CreateBackup();

                MessageBox.Show(
                    "Автоматическая резервная копия создана успешно!",
                    "Резервное копирование",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось создать резервную копию:\n\n" +
                    ex.Message,
                    "Ошибка резервного копирования",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}