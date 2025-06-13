using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ADO_WPFSH.Models;
using Theme.WPF.Themes;

namespace ADO_WPFSH
{
    public partial class MainWindow : Window
    {
        AnketaContext? a; // единый контекст данных
        WindowEdit? Edit;
        About? about;
        Employee? p;
        List<String> providers = ["SQLite", "SqlServer"];
        public MainWindow()
        {
            InitializeComponent();
            // Настройка горячих клавиш:
            // F5 - обновление данных
            this.InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => RefreshClick(null, null)),
                new KeyGesture(Key.F5)));
            // Ctrl+N - добавление сотрудника
            this.InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => AddClick(null, null)),
                new KeyGesture(Key.N, ModifierKeys.Control)));
            // Ctrl+E - редактирование сотрудника
            this.InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => EditClick(null, null)),
                new KeyGesture(Key.E, ModifierKeys.Control)));
            // Delete - удаление сотрудника
            this.InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => DeleteClick(null, null)),
                new KeyGesture(Key.D, ModifierKeys.Control)));
            DataGridEmployee.Columns.Add(new DataGridTextColumn()
            { Header = "Табельный номер", Binding = new Binding("Tab") });
            DataGridEmployee.Columns.Add(new DataGridTextColumn()
            { Header = "Фамилия", Binding = new Binding("SecondName") });
            DataGridEmployee.Columns.Add(new DataGridTextColumn()
            { Header = "Имя", Binding = new Binding("FirstName") });
            DataGridEmployee.Columns.Add(new DataGridTextColumn()
            { Header = "Отчество", Binding = new Binding("ParentName") });
            // источник по умолчанию - SQLite
            a = new("SQLite");
            DataGridEmployee.ItemsSource = a.Employees.ToList();
            // заполнение ComboBox для выбора провайдера БД
            ProviderDB.ItemsSource = providers;
            // ProviderDB.Text = "SQLite"; // так тоже можно
            ProviderDB.SelectedIndex = 0; // по умолчанию - первая БД в списке
        }
        private void ProviderDB_DropDownClosed(object sender, EventArgs e)
        {
            a?.Dispose();
            a = new(ProviderDB.Text);
            DataGridEmployee.ItemsSource = a.Employees.ToList();
        }
        private void AddClick(object sender, RoutedEventArgs e)
        {
            // создаю окно редактирования
            Edit = new(a, 0); // передаю контекст и номер сотрудника = 0 (новый)
            Edit?.Show();
        }
        private void EditClick(object sender, RoutedEventArgs e)
        {
            // создаю окно редактирования
            // передаю контекст и номер сотрудника
            // SelectedIndex
            var emp = DataGridEmployee.SelectedItem as Employee;
            if (emp is not null)
            {
                Edit = new(a, emp.Tab);
                Edit?.Show();
            }
            else
            {
                Status.Content = "Для редактирования сотрудника выберите его !!!"; 
            }
            // обновляем список
            RefreshClick(sender, e);
        }
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите удалить данные ?",
                "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                // SelectedIndex
                var emp = DataGridEmployee.SelectedItem as Employee;
                if (emp is not null)
                {
                    a.Employees.Remove(emp);
                    a.SaveChanges();
                    RefreshClick(sender, e); // обновляем список
                }
                else
                {
                    Status.Content = "Для удаления сотрудника выберите его !!!";
                }
            }
        }
        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            DataGridEmployee.ItemsSource = a?.Employees.ToList();
        }
        private void CloseWindow(object sender, EventArgs e)
        {
            Close();
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (about is null)
            {
                about = new();
            }
            */
            // создаю окно "О программе", если его нет
            about ??= new(); // надеюсь сейчас так все пишут, а не как в комментах ?
            about?.Show();
            about = null; 
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            a?.Dispose();
            Edit?.Close();
            about?.Close();
        }
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
	    private void ChangeTheme(object sender, RoutedEventArgs e)
		{
            switch (((MenuItem)sender).Uid)
            {
                case "0":
                    ThemesController.SetTheme(ThemeType.DeepDark);
                    break;
                case "1":
                    ThemesController.SetTheme(ThemeType.SoftDark);
                    break;
                case "2":
                    ThemesController.SetTheme(ThemeType.DarkGreyTheme);
                    break;
                case "3":
                    ThemesController.SetTheme(ThemeType.GreyTheme);
                    break;
                case "4":
                    ThemesController.SetTheme(ThemeType.LightTheme);
                    break;
                case "5":
                    ThemesController.SetTheme(ThemeType.RedBlackTheme);
                    break;
            }
            e.Handled = true;
        }
    }
    public class RelayCommand : ICommand //RelayCommand : ICommand позволяет реализовать привязку (Binding) команд в WPF, но не напрямую, а через механизм ICommand
                                         //Binding в XAML (Command="{Binding SaveCommand}") возможен, потому что RelayCommand реализует ICommand
    {
        private readonly Action<object> _execute;//_execute — делегат Action<object>, хранящий метод, который будет вызван при выполнении команды (например, сохранение данных)
        private readonly Predicate<object> _canExecute;//_canExecute — делегат Predicate<object>, определяющий, может ли команда быть выполнена (например, проверка валидности данных). Может быть null
                                                       //конструктор класса RelayCommand:
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null) //Action<object> execute делегат, содержащий метод, который будет вызван при выполнении команды(например, сохранение данных)
        {   //проверка на null если execute не передан(равен null), генерируется исключение ArgumentNullException
            //переданный метод сохраняется в поле _execute для последующего вызова в Execute()
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;//сохранение делегата canExecute в поле _canExecute
        }
        //может ли команда быть выполнена в данный момент (например, кнопка будет активна/неактивна)
        //если _canExecute не задан (равен null), возвращает true → команда всегда доступна
        //если _canExecute задан, вызывает его с параметром parameter и возвращает результат
        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        //public void Execute вызывает делегат _execute, переданный в конструкторе, с параметром parameter
        public void Execute(object parameter) => _execute(parameter);
        //Событие CanExecuteChanged уведомляет UI (например, кнопку), что состояние команды изменилось и нужно перепроверить CanExecute()
        public event EventHandler CanExecuteChanged
        {   //обеспечивает автоматическое обновление состояния команд (например, активности/неактивности кнопок) при изменении условий в приложении
            add => CommandManager.RequerySuggested += value; //add - аксессор события вызывается при подписке на событие (+=)
            remove => CommandManager.RequerySuggested -= value; //remove - вызывается при отписке от события (-=)

        }
    }
}

/*
    String provider = "SQLite";
    public AnketaContext(String provider)
    {
        this.provider = provider;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        switch (provider)
        {
            case "SQLite":
                optionsBuilder.UseSqlite(ConfigurationManager.ConnectionStrings["Anketa_SQLite"].
                           ConnectionString);
                break;
            case "SqlServer":
                optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["Anketa_Express"].
                                            ConnectionString);
                break;
            default:
                optionsBuilder.UseSqlite(ConfigurationManager.ConnectionStrings["Anketa_SQLite"].
                           ConnectionString);
                break;
        }
    }
 
*/