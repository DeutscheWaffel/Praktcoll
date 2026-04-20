using System;
using System.Drawing;
using System.Windows.Forms;
using ShoeStore.Data;
using ShoeStore.Models;

namespace ShoeStore
{
    public partial class OrdersForm : Form
    {
        private readonly User _currentUser;

        public OrdersForm(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            ApplyStyle(this);
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                using var db = new AppDbContext();
                var orders = db.Orders.Include(o => o.Product).ToList();
                dgvOrders.DataSource = orders;
                
                // Настройка отображения
                if (dgvOrders.Columns["ProductId"] != null) dgvOrders.Columns["ProductId"].Visible = false;
                if (dgvOrders.Columns["Id"] != null) dgvOrders.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

            foreach (Control c in ctrl.Controls) ApplyStyle(c);
        }
    }
}