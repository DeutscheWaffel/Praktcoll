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
    public partial class ProductEditForm : Form
    {
        private readonly Product? _product;
        private readonly User _user;
        private string _tempImagePath = "";

        public ProductEditForm(Product? product, User user)
        {
            _product = product;
            _user = user;
            InitializeComponent();
            ApplyStyle(this);
            LoadComboBoxes();
            
            Text = product == null ? "Добавление товара" : "Редактирование товара";

            if (product != null)
            {
                txtName.Text = product.Name;
                cbCategory.SelectedValue = product.CategoryId;
                txtDesc.Text = product.Description;
                txtManufacturer.Text = product.Manufacturer;
                cbSupplier.SelectedValue = product.SupplierId;
                numPrice.Value = product.Price;
                numQty.Value = product.Quantity;
                numDiscount.Value = product.Discount;
                txtUnit.Text = product.Unit;
                _tempImagePath = product.ImagePath;
                LoadImage(product.ImagePath);
            }
            else
            {
                LoadImage(""); // Заглушка
            }
        }

        private void LoadComboBoxes()
        {
            using var db = new AppDbContext();
            
            cbCategory.DataSource = db.Categories.ToList();
            cbCategory.DisplayMember = "Name";
            cbCategory.ValueMember = "Id";

            cbSupplier.DataSource = db.Suppliers.ToList();
            cbSupplier.DisplayMember = "Name";
            cbSupplier.ValueMember = "Id";
        }

        private void LoadImage(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                pbImage.Image = Image.FromFile(path);
                pbImage.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                pbImage.Image = CreatePlaceholderImage("NO IMAGE", Color.Gray);
                pbImage.SizeMode = PictureBoxSizeMode.CenterImage;
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            using var open = new OpenFileDialog();
            open.Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp";
            if (open.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    Directory.CreateDirectory(imagesDir);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(open.FileName);
                    string newPath = Path.Combine(imagesDir, fileName);

                    // Проверка размера и ресайз (упрощенно: сохранение с проверкой)
                    using (var img = Image.FromFile(open.FileName))
                    {
                        if (img.Width > 300 || img.Height > 200)
                        {
                            var resized = new Bitmap(img, new Size(300, 200));
                            resized.Save(newPath);
                        }
                        else
                        {
                            File.Copy(open.FileName, newPath, true);
                        }
                    }

                    // Удаление старого фото если это редактирование
                    if (!string.IsNullOrEmpty(_tempImagePath) && File.Exists(_tempImagePath) && _tempImagePath.Contains("Images"))
                    {
                        try { File.Delete(_tempImagePath); } catch { /* Игнорировать ошибки удаления */ }
                    }

                    _tempImagePath = newPath;
                    LoadImage(newPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите наименование товара", "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (numPrice.Value < 0 || numQty.Value < 0)
            {
                MessageBox.Show("Цена и количество не могут быть отрицательными", "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var db = new AppDbContext();

                if (_product == null)
                {
                    // Добавление
                    var newProd = new Product
                    {
                        Name = txtName.Text,
                        CategoryId = (int)cbCategory.SelectedValue,
                        Description = txtDesc.Text,
                        Manufacturer = txtManufacturer.Text,
                        SupplierId = (int)cbSupplier.SelectedValue,
                        Price = numPrice.Value,
                        Unit = txtUnit.Text,
                        Quantity = (int)numQty.Value,
                        Discount = numDiscount.Value,
                        ImagePath = _tempImagePath
                    };
                    db.Products.Add(newProd);
                }
                else
                {
                    // Редактирование
                    _product.Name = txtName.Text;
                    _product.CategoryId = (int)cbCategory.SelectedValue;
                    _product.Description = txtDesc.Text;
                    _product.Manufacturer = txtManufacturer.Text;
                    _product.SupplierId = (int)cbSupplier.SelectedValue;
                    _product.Price = numPrice.Value;
                    _product.Unit = txtUnit.Text;
                    _product.Quantity = (int)numQty.Value;
                    _product.Discount = numDiscount.Value;
                    _product.ImagePath = _tempImagePath;
                    
                    db.Products.Update(_product);
                }

                db.SaveChanges();
                MessageBox.Show("Данные успешно сохранены", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Bitmap CreatePlaceholderImage(string text, Color color)
        {
            var bmp = new Bitmap(300, 200);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                using (var font = new Font("Arial", 14))
                {
                    var size = g.MeasureString(text, font);
                    g.DrawString(text, font, Brushes.Gray, (300 - size.Width) / 2, (200 - size.Height) / 2);
                }
            }
            return bmp;
        }

        private void ApplyStyle(Control ctrl)
        {
            ctrl.Font = new Font("Times New Roman", 12);
            if (ctrl is Form) ctrl.BackColor = Color.White;
            if (ctrl is Button btn)
            {
                btn.BackColor = Color.FromArgb(0, 250, 154);
                btn.FlatStyle = FlatStyle.Flat;
            }
            foreach (Control c in ctrl.Controls) ApplyStyle(c);
        }
    }
}