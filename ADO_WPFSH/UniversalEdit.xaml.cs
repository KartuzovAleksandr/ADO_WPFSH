using ADO_WPFSH.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;

namespace ADO_WPFSH
{
    public partial class UniversalEdit : Window
    {
        private readonly AnketaContext _context;
        private readonly Type _entityType;
        private readonly IQueryable _query;

        public UniversalEdit(AnketaContext context, Type entityType)
        {
            InitializeComponent();

            _context = context ?? throw new ArgumentNullException(nameof(context));
            _entityType = entityType ?? throw new ArgumentNullException(nameof(entityType));

            // Получаем DbSet через рефлексию
            var dbSetProperty = typeof(AnketaContext).GetProperty(
                entityType.Name + "s", // Например: "Educations", "Qualifies"
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );

            if (dbSetProperty == null)
                throw new ArgumentException($"DbSet для типа {entityType.Name} не найден в AnketaContext.");

            var dbSet = dbSetProperty.GetValue(_context) as IQueryable;
            if (dbSet == null)
                throw new InvalidOperationException($"Не удалось получить DbSet для {entityType.Name}.");

            _query = dbSet;

            // Загружаем данные (важно: без AsNoTracking!)
            var items = _query.Cast<object>().ToList();
            DataGrid.ItemsSource = items;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var entries = _context.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Unchanged)
                .ToList();
            if (entries.Count == 0)
            {
                MessageBox.Show("Нет изменений для сохранения.");
                return;
            }

            try
            {
                _context.SaveChanges();
                MessageBox.Show("Изменения успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}