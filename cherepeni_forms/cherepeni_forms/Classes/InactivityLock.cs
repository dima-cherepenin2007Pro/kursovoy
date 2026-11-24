
using System;
using System.Windows.Forms;

namespace cherepeni_forms.Classes
{
    public class InactivityLock : IMessageFilter, IDisposable
    {
        private readonly Timer inactivityTimer;

        private readonly Func<Form> authorizationFormFactory;

        private bool isDisposed = false;

        public InactivityLock(
            Func<Form> authorizationFormFactory)
        {
            this.authorizationFormFactory =
                authorizationFormFactory;

            inactivityTimer = new Timer();

            // 1 минута = 60000 миллисекунд
            inactivityTimer.Interval = 60000;

            inactivityTimer.Tick += InactivityTimer_Tick;
        }

        // =====================================================
        // ЗАПУСК ОТСЧЁТА
        // =====================================================

        public void Start()
        {
            if (isDisposed)
                return;

            Application.RemoveMessageFilter(this);

            Application.AddMessageFilter(this);

            ResetTimer();
        }

        // =====================================================
        // СБРОС ТАЙМЕРА
        // =====================================================

        private void ResetTimer()
        {
            inactivityTimer.Stop();
            inactivityTimer.Start();
        }

        // =====================================================
        // ОТСЛЕЖИВАНИЕ ДЕЙСТВИЙ ПОЛЬЗОВАТЕЛЯ
        // =====================================================

        public bool PreFilterMessage(ref Message m)
        {
            if (!inactivityTimer.Enabled)
                return false;

            switch (m.Msg)
            {
                // Движение мыши
                case 0x0200:

                // Левая кнопка мыши
                case 0x0201:
                case 0x0202:

                // Правая кнопка мыши
                case 0x0204:
                case 0x0205:

                // Средняя кнопка мыши
                case 0x0207:
                case 0x0208:

                // Колесо мыши
                case 0x020A:

                // Нажатие клавиш
                case 0x0100:
                case 0x0101:
                case 0x0104:
                case 0x0105:
                case 0x0102:

                    ResetTimer();
                    break;
            }

            // Сообщение продолжает обрабатываться
            return false;
        }

        // =====================================================
        // БЛОКИРОВКА ПРИ ОТСУТСТВИИ АКТИВНОСТИ
        // =====================================================

        private void InactivityTimer_Tick(
            object sender,
            EventArgs e)
        {
            // Останавливаем таймер
            inactivityTimer.Stop();

            // Запоминаем активную форму
            Form activeForm = Form.ActiveForm;

            try
            {
                // Создаём форму авторизации
                using (Form authForm =
                    authorizationFormFactory())
                {
                    DialogResult result;

                    if (activeForm != null &&
                        !activeForm.IsDisposed)
                    {
                        result = authForm.ShowDialog(activeForm);
                    }
                    else
                    {
                        result = authForm.ShowDialog();
                    }

                    // Если авторизация успешна
                    if (result == DialogResult.OK)
                    {
                        ResetTimer();
                    }
                    else
                    {
                        // Не удалось авторизоваться
                        Application.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка блокировки системы:\n\n" +
                    ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                Application.Exit();
            }
        }

        // =====================================================
        // ОСТАНОВКА
        // =====================================================

        public void Stop()
        {
            inactivityTimer.Stop();

            Application.RemoveMessageFilter(this);
        }

        // =====================================================
        // ОСВОБОЖДЕНИЕ РЕСУРСОВ
        // =====================================================

        public void Dispose()
        {
            if (isDisposed)
                return;

            Stop();

            inactivityTimer.Dispose();

            isDisposed = true;
        }
    }
}
