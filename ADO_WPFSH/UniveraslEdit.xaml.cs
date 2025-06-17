using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ADO_WPFSH.Models;
using Microsoft.EntityFrameworkCore;

namespace ADO_WPFSH
{
    /// <summary>
    /// Логика взаимодействия для UniversalEdit.xaml
    /// </summary>
    public partial class UniversalEdit : Window
    {
        private AnketaContext? _context;
        private List<Education>? _items;

        public UniversalEdit(AnketaContext a)
        {
            InitializeComponent();
            _context = a;
            LoadData();
        }

        private void LoadData()
        {
            _items = _context?.Educations.ToList();
            dataGrid.ItemsSource = _items;
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем контекст
            foreach (var item in _items)
            {
                if (_context?.Entry(item).State == EntityState.Detached)
                {
                    _context.Educations.Add(item); // Добавление нового продукта
                }
                else
                {
                    _context.Entry(item).State = EntityState.Modified; // Обновление существующего продукта
                }
            }

            _context?.SaveChanges();
            MessageBox.Show("Изменения сохранены!");
        }
        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Установка состояния для отслеживания изменений
            var editedProduct = (Education)e.Row.DataContext;
            _context.Entry(editedProduct).State = EntityState.Modified;
        }

        private void dataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && dataGrid.SelectedItem != null)
            {
                var productToRemove = (Education)dataGrid.SelectedItem;
                _context?.Educations.Remove(productToRemove);
                _items?.Remove(productToRemove);
                dataGrid.ItemsSource = null; // Обнуляем источник
                dataGrid.ItemsSource = _items; // Обновляем источник
                MessageBox.Show("Продукт удалён!");
            }
        }
    }
}