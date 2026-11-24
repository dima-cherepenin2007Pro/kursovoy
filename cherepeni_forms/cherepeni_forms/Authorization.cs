using System;
using System.Drawing;
using System.Windows.Forms;
using cherepeni_forms.Classes;
using MySql.Data.MySqlClient;
using cherepeni_forms.Forms;
using cherepeni_forms.Forms.Salesperson;
using cherepeni_forms.Forms.Manager;

namespace cherepeni_forms
{
    public partial class Authorization : Form
    {
        private static InactivityLock inactivityLock;
        // Режим повторной авторизации
        private bool isReauthentication = false;

        // ID пользователя текущего сеанса
        private int expectedUserID = -1;

        private string currentCaptcha = "";
        private Random random = new Random();

        // Количество неудачных попыток авторизации
        private int failedAttempts = 0;

        // Время окончания блокировки
        private DateTime lockoutEndTime;

        // Таймер блокировки
        private Timer lockoutTimer;

        public Authorization()
        {
            InitializeComponent();

            GenerateCaptcha();

            // CAPTCHA скрыта при первом запуске
            ShowCaptchaControls(false);

            txtPassword.UseSystemPasswordChar = true;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Создание таймера блокировки
            lockoutTimer = new Timer();
            lockoutTimer.Interval = 1000;
            lockoutTimer.Tick += LockoutTimer_Tick;
        }
        // Конструктор для повторной авторизации
        public Authorization(bool reauthentication, int userID)
            : this()
        {
            isReauthentication = reauthentication;
            expectedUserID = userID;

            this.Text = "Разблокировка системы";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        // =====================================================
        // CAPTCHA
        // =====================================================

        private void GenerateCaptcha()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            currentCaptcha = "";

            for (int i = 0; i < 5; i++)
            {
                currentCaptcha += chars[random.Next(chars.Length)];
            }

            Bitmap bitmap = new Bitmap(200, 60);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);

                // Линии
                for (int i = 0; i < 5; i++)
                {
                    using (Pen pen = new Pen(Color.LightGray, 1))
                    {
                        graphics.DrawLine(
                            pen,
                            random.Next(0, 200),
                            random.Next(0, 60),
                            random.Next(0, 200),
                            random.Next(0, 60)
                        );
                    }
                }

                // Текст CAPTCHA
                using (Font font = new Font("Arial", 24, FontStyle.Bold))
                {
                    graphics.DrawString(
                        currentCaptcha,
                        font,
                        Brushes.Black,
                        20,
                        10
                    );
                }

                // Точки
                for (int i = 0; i < 100; i++)
                {
                    bitmap.SetPixel(
                        random.Next(0, 200),
                        random.Next(0, 60),
                        Color.Gray
                    );
                }
            }

            // Удаляем старую картинку
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
            }

            pictureBox1.Image = bitmap;
        }

        // =====================================================
        // Отображение элементов CAPTCHA
        // =====================================================

        private void ShowCaptchaControls(bool visible)
        {
            pictureBox1.Visible = visible;
            txtCaptcha.Visible = visible;
        }

        // =====================================================
        // Обновление CAPTCHA
        // =====================================================

        // =====================================================
        // Проверка блокировки
        // =====================================================

        private bool IsLocked()
        {
            if (DateTime.Now < lockoutEndTime)
            {
                TimeSpan remaining = lockoutEndTime - DateTime.Now;

                MessageBox.Show(
                    $"Вход временно заблокирован.\n\n" +
                    $"Осталось: {remaining.Seconds + 1} секунд.",
                    "Вход заблокирован",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return true;
            }

            return false;
        }

        // =====================================================
        // Управление доступностью элементов
        // =====================================================

        private void SetLoginControlsEnabled(bool enabled)
        {
            txtLogin.Enabled = enabled;
            txtPassword.Enabled = enabled;
            txtCaptcha.Enabled = enabled;

            btnVerify.Enabled = enabled;
        }

        // =====================================================
        // Блокировка на 10 секунд
        // =====================================================

        private void LockLogin()
        {
            lockoutEndTime = DateTime.Now.AddSeconds(10);

            SetLoginControlsEnabled(false);

            lockoutTimer.Stop();
            lockoutTimer.Start();

            MessageBox.Show(
                "Не удалось пройти авторизацию.\n\n" +
                "Вход заблокирован на 10 секунд.",
                "Вход временно заблокирован",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        // =====================================================
        // Таймер блокировки
        // =====================================================

        private void LockoutTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now >= lockoutEndTime)
            {
                lockoutTimer.Stop();

                SetLoginControlsEnabled(true);

                GenerateCaptcha();
                txtCaptcha.Clear();
                txtPassword.Clear();

                txtLogin.Focus();

                MessageBox.Show(
                    "Блокировка снята. Вы можете попробовать войти снова.",
                    "Вход разрешён",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        // =====================================================
        // Кнопка "Войти"
        // =====================================================

        private void btnVerify_Click(object sender, EventArgs e)
        {
            if (IsLocked())
            {
                return;
            }

            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text;
            string captcha = txtCaptcha.Text.Trim().ToUpper();

            // -----------------------------
            // Проверка логина
            // -----------------------------

            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show(
                    "Введите логин.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtLogin.Focus();
                return;
            }

            // -----------------------------
            // Проверка пароля
            // -----------------------------

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Введите пароль.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtPassword.Focus();
                return;
            }

            // -----------------------------
            // Проверка CAPTCHA
            // После первой неудачной попытки
            // -----------------------------

            if (failedAttempts >= 1)
            {
                if (string.IsNullOrWhiteSpace(captcha))
                {
                    MessageBox.Show(
                        "Введите CAPTCHA.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    txtCaptcha.Focus();
                    return;
                }

                if (captcha != currentCaptcha)
                {
                    failedAttempts++;

                    GenerateCaptcha();
                    txtCaptcha.Clear();

                    LockLogin();
                    return;
                }
            }

            // -----------------------------
            // Проверка пользователя
            // -----------------------------

            AuthorizeUser(login, password);
        }

        // =====================================================
        // Авторизация пользователя через БД
        // =====================================================

        private void AuthorizeUser(string login, string password)
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
                            u.RoleID,
                            r.RoleName
                        FROM Users u
                        INNER JOIN Roles r
                            ON u.RoleID = r.RoleID
                        WHERE u.Login = @login
                          AND u.Password = @password
                        LIMIT 1;
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@login",
                            login
                        );

                        command.Parameters.AddWithValue(
                            "@password",
                            password
                        );

                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            // -----------------------------
                            // Неверный логин или пароль
                            // -----------------------------

                            if (!reader.Read())
                            {
                                failedAttempts++;

                                // Первая неудачная попытка
                                if (failedAttempts == 1)
                                {
                                    MessageBox.Show(
                                        "Неверный логин или пароль.",
                                        "Ошибка авторизации",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error
                                    );

                                    // Показываем CAPTCHA
                                    ShowCaptchaControls(true);

                                    GenerateCaptcha();
                                    txtCaptcha.Clear();
                                    txtPassword.Clear();

                                    txtPassword.Focus();
                                }
                                else
                                {
                                    // Вторая и последующие попытки
                                    GenerateCaptcha();
                                    txtCaptcha.Clear();
                                    txtPassword.Clear();

                                    LockLogin();
                                }

                                return;
                            }

                            // -----------------------------
                            // Получение данных пользователя
                            // -----------------------------

                            int userID =
                                Convert.ToInt32(reader["UserID"]);

                            string firstName =
                                reader["FirstName"].ToString();

                            string lastName =
                                reader["LastName"].ToString();

                            string roleName =
                                reader["RoleName"].ToString();

                            // Закрываем reader
                            reader.Close();

                            // Сброс неудачных попыток
                            // Сброс неудачных попыток
                            failedAttempts = 0;

                            // Если выполняется повторная авторизация
                            if (isReauthentication)
                            {
                                // Проверяем, что вошёл тот же пользователь
                                if (userID != expectedUserID)
                                {
                                    MessageBox.Show(
                                        "Войти может только пользователь, " +
                                        "который ранее открыл данный сеанс.",
                                        "Ошибка авторизации",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning
                                    );

                                    txtPassword.Clear();
                                    return;
                                }

                                // Авторизация успешна — разблокируем систему
                                this.DialogResult = DialogResult.OK;
                                this.Close();

                                return;
                            }

                            // Обычная авторизация
                            OpenFormByRole(
                                userID,
                                firstName,
                                lastName,
                                roleName
                            );
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Ошибка при работе с базой данных:\n\n" +
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
        // Открытие формы в зависимости от роли
        // =====================================================

        private void OpenFormByRole(
            int userID,
            string firstName,
            string lastName,
            string roleName)
        {
            Form nextForm = null;

            switch (roleName)
            {
                case "Системный администратор":

                    nextForm = new Sistem_admin(
                        userID,
                        firstName,
                        lastName,
                        roleName
                    );

                    break;

                case "Продавец":

                    nextForm = new SalesPerson_form(
                        userID,
                        firstName,
                        lastName,
                        roleName
                    );

                    break;

                case "Менеджер":

                    nextForm = new Manager_form(
                        userID,
                        firstName,
                        lastName,
                        roleName
                    );

                    break;

                default:

                    MessageBox.Show(
                        "Для пользователя не назначена корректная роль.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );

                    return;
            }

            // Скрываем форму авторизации
            // Создаём систему контроля активности
            if (inactivityLock != null)
            {
                inactivityLock.Dispose();
            }

            inactivityLock = new InactivityLock(
                () => new Authorization(true, userID)
            );

            // Скрываем форму авторизации
            this.Hide();

            // При закрытии основной формы
            // закрываем приложение
            nextForm.FormClosed += (s, e) =>
            {
                if (inactivityLock != null)
                {
                    inactivityLock.Dispose();
                    inactivityLock = null;
                }

                this.Close();
            };

            // Открываем рабочую форму
            nextForm.Show();

            // Запускаем контроль активности
            inactivityLock.Start();
        }
    }
}