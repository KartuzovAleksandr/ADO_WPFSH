using ADO_WPFSH.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ADO_WPFSH
{
    public partial class UniversalEdit : Window
    {
        private readonly AnketaContext _context;
        private readonly Type _entityType;
        private readonly ObservableCollection<object> _items;

        public UniversalEdit(AnketaContext context, Type entityType)
        {
            InitializeComponent();

            _context = context ?? throw new ArgumentNullException(nameof(context));
            _entityType = entityType ?? throw new ArgumentNullException(nameof(entityType));

            // Получаем DbSet
            //var dbSetProperty = typeof(AnketaContext).GetProperties()
            //                .FirstOrDefault(p => p.PropertyType.IsGenericType &&
            //                       p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
            //                       p.PropertyType.GetGenericArguments()[0] == entityType);

            var dbSetProperty = typeof(AnketaContext).GetProperty(
                entityType.Name + "s",
                BindingFlags.Public | BindingFlags.Instance
            );

            if (dbSetProperty == null)
                throw new ArgumentException($"DbSet для {entityType.Name} не найден.");

            var dbSet = dbSetProperty.GetValue(_context) as IQueryable;
            if (dbSet == null)
                throw new InvalidOperationException($"Не удалось получить DbSet для {entityType.Name}.");

            // Загружаем существующие сущности
            var existingEntities = dbSet.Cast<object>().ToList();

            // Создаём ObservableCollection для поддержки уведомлений
            _items = new ObservableCollection<object>(existingEntities);
            DataGrid.ItemsSource = _items;

            // Подписываемся на изменение коллекции (добавление/удаление строк)
            ((INotifyCollectionChanged)_items).CollectionChanged += OnCollectionChanged;
        }
        private void DataGrid_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var property = e.PropertyDescriptor as System.ComponentModel.PropertyDescriptor;
            var displayAttr = property?.Attributes[typeof(DisplayAttribute)] as DisplayAttribute;

            if (displayAttr != null)
            {
                e.Column.Header = displayAttr.Name;
                if (displayAttr.GetAutoGenerateField() == false)
                    e.Cancel = true;
            }
        }
        private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object newItem in e.NewItems)
                {
                    // Добавляем новый объект в контекст EF
                    _context.Add(newItem);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object oldItem in e.OldItems)
                {
                    // Удаляем из контекста
                    _context.Remove(oldItem);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var entries = _context.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Unchanged)
                .ToList();
            try
            {
                _context.SaveChanges();
                MessageBox.Show($"Изменения {entries.Count} успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            //if (e.Key == Key.Delete)
            //{
            //    if (MessageBox.Show("Вы действительно хотите удалить строку ?",
            //        "Это может привести к нарушению целостности данных", MessageBoxButton.YesNo, MessageBoxImage.Question,
            //        MessageBoxResult.Yes) == MessageBoxResult.Yes)
            //    {
            //        _items.Remove(DataGrid.SelectedItem);
            //    }
            //}
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var entries = _context.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Unchanged)
                .ToList();
            if (entries.Count > 0)
            {
                if (MessageBox.Show("Вы действительно хотите выйти из редактора ?",
                    "Сохраните данные", MessageBoxButton.YesNo, MessageBoxImage.Question,
                    MessageBoxResult.Yes) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
        private void DataGrid_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                // Показываем своё подтверждение
                var result = MessageBox.Show(
                    "Вы действительно хотите удалить строку?",
                    "Это может привести к нарушению целостности данных",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                {
                    // Отменяем стандартное удаление
                    e.Handled = true;
                }
            }
        }

    }
}
