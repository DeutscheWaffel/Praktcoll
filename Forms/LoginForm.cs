using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Windows.Forms;
using ShoeStore.Data;
using ShoeStore.Models;

namespace ShoeStore.Forms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            ApplyStyle(this);
            LoadLogo();
            
            // Создание БД при первом запуске
            using var db = new AppDbContext();
            db.Database.EnsureCreated();
        }

        private void LoadLogo()
        {
            // Попытка загрузить логотип, если нет - создаем заглушку
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logo.png");
            if (File.Exists(logoPath))
            {
                pbLogo.Image = Image.FromFile(logoPath);
                pbLogo.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                // Программная заглушка, если файла нет
                pbLogo.Image = CreatePlaceholderImage("LOGO", Color.FromArgb(0, 250, 154));
                pbLogo.SizeMode = PictureBoxSizeMode.CenterImage;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            using var db = new AppDbContext();
            var user = db.Users.FirstOrDefault(u => u.Login == tbLogin.Text && u.Password == tbPass.Text);

            if (user != null)
            {
                OpenMain(user);
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGuest_Click(object sender, EventArgs e)
        {
            var guest = new User { Role = "Guest", FullName = "Гость" };
            OpenMain(guest);
        }

        private void OpenMain(User user)
        {
            var mainForm = new ShoeStore.Forms.MainForm(user);
            mainForm.Show();
            this.Hide();
            
            // Закрытие приложения при закрытии главной формы
            mainForm.FormClosed += (s, args) => this.Close();
        }

        private void ApplyStyle(Control ctrl)
        {
            ctrl.Font = new Font("Times New Roman", 12);
            ctrl.BackColor = Color.White;
            
            if (ctrl is Button btn)
            {
                btn.BackColor = Color.FromArgb(0, 250, 154); // Accent
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.ForeColor = Color.Black;
            }
            else if (ctrl is TextBox tb)
            {
                tb.BackColor = Color.FromArgb(127, 255, 0); // Extra Back
            }

            foreach (Control c in ctrl.Controls)
            {
                ApplyStyle(c);
            }
        }

        private Bitmap CreatePlaceholderImage(string text, Color color)
        {
            var bmp = new Bitmap(200, 200);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                using (var brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, 0, 0, 200, 200);
                }
                using (var font = new Font("Arial", 20, FontStyle.Bold))
                {
                    var size = g.MeasureString(text, font);
                    g.DrawString(text, font, Brushes.Black, (200 - size.Width) / 2, (200 - size.Height) / 2);
                }
            }
            return bmp;
        }
    }
}