using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ADO_WPFSH.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
                // Проверяем, является ли продукт новым или существующим
                if (item.id == 0) // Новый продукт
                {
                    _context.Educations.Add(item); // Добавление нового продукта
                    _context?.SaveChanges();
                }
                //else
                //{
                //    // Если вы хотите изменить Id, нужно удалить старый и добавить новый
                //    if (_context.Entry(item).State == EntityState.Detached)
                //    {
                //        var existingProduct = _context.Educations.Find(item.id);
                //        if (existingProduct != null)
                //        {
                //            // Удаляем старый продукт
                //            _context.Educations.Remove(existingProduct);
                //        }

                //        // Добавляем новый с измененным Id
                //        _context.Educations.Add(item);
                //    }
                    //else
                    //{
                    //    if (!idChanged)
                    //    {
                    //        _context.Entry(item).State = EntityState.Modified; // Обновление существующего продукта
                    //    }
                    //}
            }

            // _context?.SaveChanges();
            MessageBox.Show("Изменения сохранены!");
        }
        private int _oldId;
        private bool idChanged;

        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            idChanged = false;
            // Получаем старый Id перед началом редактирования
            var education = e.Row.Item as Education;
            if (education != null)
            {
                _oldId = education.id; // Сохраняем oldId
            }
        }
        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Проверяем, что редактирование завершено
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Получаем редактируемую строку
                var editedItem = e.Row.Item as Education; // Пример вашей модели

                // Определяем, какая колонка была отредактирована
                var editedColumn = e.Column as DataGridTextColumn;

                if (editedColumn != null)
                {
                    // Получаем имя колонки
                    string columnName = editedColumn.Header.ToString();

                    // Выводим информацию о редактировании
                    Console.WriteLine($"Редактированная колонка: {columnName}");

                    // Здесь можете добавить логику для обработки изменений
                    // Например, если вам нужно изменить Id:
                    if (columnName == "id")
                    {
                        // Получаем новый Id из редактируемого элемента
                        int newId = Convert.ToInt32(((TextBox)e.EditingElement).Text);
                        ChangeEducationId(_oldId, newId); // Используйте oldId и новый Id
                        // ChangeEducationId(editedItem.id, newId); // Используйте oldId и новый Id
                    }
                    else
                    {
                        _context.Entry(editedItem).State = EntityState.Modified;
                        _context?.SaveChanges();
                    }
                }
            }
        }
        public void ChangeEducationId(int oldId, int newId)
        {
            using (var context = new AnketaContext())
            {
                var existingEducation = context.Educations.Find(oldId);
                if (existingEducation != null)
                {
                    // Удаляем существующую запись
                    context.Educations.Remove(existingEducation);
                    context.SaveChanges(); // Сохраняем изменения

                    // Создаем новую запись с новым Id
                    var newEducation = new Education
                    {
                        id = newId, // Устанавливаем новый Id
                        Name = existingEducation.Name,
                    };

                    context.Educations.Add(newEducation);
                    context.SaveChanges();
                    Console.WriteLine($"Изменен: {newId}");
                    idChanged = true;
                }
                else
                {
                    throw new Exception("Запись с указанным Id не найдена.");
                }
            }
        }
        private void dataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete && dataGrid.SelectedItem != null)
            {
                // диалог подтверждения !!!
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