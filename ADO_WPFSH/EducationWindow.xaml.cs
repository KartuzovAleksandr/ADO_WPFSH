using System.Windows;
using Microsoft.EntityFrameworkCore;
using ADO_WPFSH.Models;

namespace ADO_WPFSH
{
    /// <summary>
    /// Логика взаимодействия для EducationWindow.xaml
    /// </summary>
    public partial class EducationWindow : Window
    {
        AnketaContext? _context;

        public EducationWindow(AnketaContext a)
        {
            InitializeComponent();
            _context = a;
            LoadData();
        }

        private void LoadData()
        {
            var items = _context.Educations.ToList();
            dataGrid.ItemsSource = items;
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            var items = (List<Education>)dataGrid.ItemsSource;

            // Обновляем контекст
            foreach (var item in items)
            {
                _context.Entry(item).State = item.id == 0 ? EntityState.Added : EntityState.Modified;
            }

            _context.SaveChanges();
            MessageBox.Show("Изменения сохранены!");
        }
    }
}