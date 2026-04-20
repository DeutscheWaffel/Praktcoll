using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ShoeStore.Data;
using ShoeStore.Models;
using Microsoft.EntityFrameworkCore;

namespace ShoeStore.Forms
{
    public partial class MainForm : Form
    {
        private readonly User _currentUser;
        private bool _isEditingOpen = false; // Блокировка множественного редактирования

        public MainForm(User user)
        {
            _currentUser = user;
            InitializeComponent();
            ApplyStyle(this);
            
            lblUser.Text = $"{user.FullName} ({TranslateRole(user.Role)})";
            
            SetupRoleAccess();
            LoadSuppliersFilter();
            LoadProducts();
        }

        private string TranslateRole(string role)
        {
            return role switch
            {
                "Admin" => "Администратор",
                "Manager" => "Менеджер",
                "Client" => "Клиент",
                "Guest" => "Гость",
                _ => role
            };
        }

        private void SetupRoleAccess()
        {
            bool isAdmin = _currentUser.Role == "Admin";
            bool isManagerOrAdmin = isAdmin || _currentUser.Role == "Manager";

            btnAdd.Visible = isAdmin;
            btnEdit.Visible = isAdmin;
            btnDelete.Visible = isAdmin;
            btnOrders.Visible = isManagerOrAdmin;

            // Поиск и фильтры только для Менеджера и Админа
            tbSearch.Visible = isManagerOrAdmin;
            cbFilterSupplier.Visible = isManagerOrAdmin;
            cbSort.Visible = isManagerOrAdmin;
            
            lblSearch.Visible = isManagerOrAdmin;
            lblFilter.Visible = isManagerOrAdmin;
            lblSort.Visible = isManagerOrAdmin;

            if (_currentUser.Role == "Guest" || _currentUser.Role == "Client")
            {
                Text = "Список товаров (Просмотр)";
            }
            else
            {
                Text = "Рабочее место: " + TranslateRole(_currentUser.Role);
            }
        }

        private void LoadSuppliersFilter()
        {
            using var db = new AppDbContext();
            var suppliers = db.Suppliers.Select(s => s.Name).ToList();
            cbFilterSupplier.Items.Clear();
            cbFilterSupplier.Items.Add("Все поставщики");
            cbFilterSupplier.Items.AddRange(suppliers.ToArray());
            cbFilterSupplier.SelectedIndex = 0;
        }

        private void LoadProducts()
        {
            try
            {
                using var db = new AppDbContext();
                var query = db.Products.Include(p => p.Supplier).Include(p => p.Category).AsQueryable();

                // Поиск (Real-time) - выполняется на стороне сервера
                if (!string.IsNullOrEmpty(tbSearch.Text))
                {
                    string search = tbSearch.Text;
                    query = query.Where(p => 
                        EF.Functions.Like(p.Name, $"%{search}%") || 
                        EF.Functions.Like(p.Description, $"%{search}%") ||
                        EF.Functions.Like(p.Manufacturer, $"%{search}%") ||
                        EF.Functions.Like(p.SupplierName, $"%{search}%") ||
                        EF.Functions.Like(p.CategoryName, $"%{search}%")
                    );
                }

                // Сортировка - выполняется на стороне сервера
                if (cbSort.SelectedItem != null)
                {
                    string sortOption = cbSort.SelectedItem.ToString();
                    if (sortOption == "Количество (по возр.)")
                        query = query.OrderBy(p => p.Quantity);
                    else if (sortOption == "Количество (по убыв.)")
                        query = query.OrderByDescending(p => p.Quantity);
                }

                // Загрузка данных в память
                var list = query.ToList();

                // Фильтр по поставщику - выполняется на стороне клиента (в памяти)
                if (cbFilterSupplier.SelectedIndex > 0)
                {
                    string selectedSupplier = cbFilterSupplier.SelectedItem.ToString();
                    list = list.Where(p => p.SupplierName == selectedSupplier).ToList();
                }

                dgvProducts.DataSource = null;
                dgvProducts.DataSource = list;

                StyleGrid();
                
                // Настройка колонок
                if (dgvProducts.Columns["ImagePath"] != null) dgvProducts.Columns["ImagePath"].Visible = false;
                if (dgvProducts.Columns["Id"] != null) dgvProducts.Columns["Id"].Visible = false;
                if (dgvProducts.Columns["CategoryId"] != null) dgvProducts.Columns["CategoryId"].Visible = false;
                if (dgvProducts.Columns["SupplierId"] != null) dgvProducts.Columns["SupplierId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StyleGrid()
        {
            foreach (DataGridViewRow row in dgvProducts.Rows)
            {
                if (row.DataBoundItem is not Product prod) continue;

                // Сброс стиля
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.ForeColor = Color.Black;
                row.DefaultCellStyle.Font = new Font("Times New Roman", 12);

                // Логика подсветки
                if (prod.Quantity == 0)
                {
                    row.DefaultCellStyle.BackColor = Color.LightBlue; // Голубой если нет на складе
                }
                else if (prod.Discount > 15)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(46, 139, 87); // #2E8B57 если скидка > 15%
                    row.DefaultCellStyle.ForeColor = Color.White;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(127, 255, 0); // #7FFF00 обычный фон
                }

                // Отображение цены со скидкой
                // Примечание: Полное форматирование ячейки цены требует создания кастомной колонки или обработки события CellFormatting.
                // Для простоты здесь реализована цветовая индикация строки.
            }
        }

        // Обработчики событий фильтров (Real-time)
        private void tbSearch_TextChanged(object sender, EventArgs e) => LoadProducts();
        private void cbFilterSupplier_SelectedIndexChanged(object sender, EventArgs e) => LoadProducts();
        private void cbSort_SelectedIndexChanged(object sender, EventArgs e) => LoadProducts();

        private void btnAdd_Click(object sender, EventArgs e)
        {
            OpenProductForm(null);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow?.DataBoundItem is Product prod)
                OpenProductForm(prod);
            else
                MessageBox.Show("Выберите товар для редактирования", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow?.DataBoundItem is not Product prod)
            {
                MessageBox.Show("Выберите товар для удаления", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var db = new AppDbContext();
            bool hasOrders = db.Orders.Any(o => o.ProductId == prod.Id);

            if (hasOrders)
            {
                MessageBox.Show("Нельзя удалить товар, который присутствует в заказах.", "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var res = MessageBox.Show($"Вы уверены, что хотите удалить товар \"{prod.Name}\"?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                try
                {
                    // Удаление файла изображения
                    if (!string.IsNullOrEmpty(prod.ImagePath) && File.Exists(prod.ImagePath))
                        File.Delete(prod.ImagePath);

                    db.Products.Remove(prod);
                    db.SaveChanges();
                    LoadProducts();
                    MessageBox.Show("Товар успешно удален", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenProductForm(Product? product)
        {
            if (_isEditingOpen)
            {
                MessageBox.Show("Окно редактирования уже открыто. Закройте его перед открытием нового.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var form = new ShoeStore.Forms.ProductEditForm(product, _currentUser);
            form.FormClosed += (s, args) => { _isEditingOpen = false; LoadProducts(); };
            _isEditingOpen = true;
            form.Show();
        }

        private void btnOrders_Click(object sender, EventArgs e)
        {
            var ordersForm = new ShoeStore.Forms.OrdersForm(_currentUser);
            ordersForm.Show();
        }

        private void ApplyStyle(Control ctrl)
        {
            ctrl.Font = new Font("Times New Roman", 12);
            if (ctrl is Form) ctrl.BackColor = Color.White;
            
            if (ctrl is Button btn)
            {
                btn.BackColor = Color.FromArgb(0, 250, 154);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
            }
            else if (ctrl is ComboBox cb || ctrl is TextBox tb)
            {
                ctrl.BackColor = Color.FromArgb(127, 255, 0);
            }

            foreach (Control c in ctrl.Controls) ApplyStyle(c);
        }
    }
}