using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ADO_WPFSH.Models;

namespace ADO_WPFSH
{
    public partial class EmployeeEdit : Window
    {
        Employee? p;
        AnketaContext? a;
        int CurrentTab;
        int? CurrentCountry = 0;
        // да, мне никто не мешает добавить аргументы к конструктору
        // и вызвать родительский конструктор окна base()
        public EmployeeEdit(AnketaContext a, int t) : base()
        {
            // в начале, т.к. иначе не будет UI-элементов для установки данных текущего сотрудника
            InitializeComponent();
            // это по существу не копия, а тот же самый экземпляр контекста данных
            this.a = a;
            // выполняем привязки - вынес отдельно !
            PerformBindings();
            // новый сотрудник или текущий
            CurrentTab = t;
            if (t == 0)
            {
                p = new(); // пустой экземпляр класса для новой строки в БД
            }
            else
            {
                p = a.Employees.Find(t); // извлекаем из коллекции
                LoadEmployee(); // загружаем данные в форму
            }
        }
        void PerformBindings()
        {
            // привязка ComboBox Country к сущности Countries
            // или то же самое в XAML - закомментировано
            // a.Countries.Local.ToObservableCollection() не работает
            Country.ItemsSource = a.Countries.ToList();
            Country.DisplayMemberPath = "Name";
            Country.SelectedValuePath = "id";
            // привязка ComboBox Region к сущности Regions
            Region.ItemsSource = null;
            Region.DisplayMemberPath = "Name";
            Region.SelectedValuePath = "Code";
            // привязка ListBox Qualification к сущности Qualifies
            Qualification.ItemsSource = a.Qualifies.ToList();
            Qualification.DisplayMemberPath = "Number";
            Qualification.SelectedValuePath = "id";
            // привязка ListBox Education к сущности Educations
            Education.ItemsSource = a.Educations.ToList();
            Education.DisplayMemberPath = "Name";
            Education.SelectedValuePath = "id";
        }
        private void Country_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var c = Country.SelectedItem as Country;
            if (c is not null)
            {
                CurrentCountry = c.id;
                var selectedRegions = from r in a.Regions.ToList()
                                      where r.CountryId == CurrentCountry
                                      select r;
                Region.ItemsSource = selectedRegions;
                // Region.ItemsSource = a.Regions.ToList().Where(c => c.CountryId == CurrentCountry).ToList();
            }
        }
        private void Region_DropDownOpened(object sender, EventArgs e)
        {
            var c = Country.SelectedItem as Country;
            if (c is not null)
            {
                CurrentCountry = c.id;
                var selectedRegions = from r in a.Regions.ToList()
                                      where r.CountryId == CurrentCountry
                                      select r;
                Region.ItemsSource = selectedRegions;
                // Region.ItemsSource = a.Regions.ToList().Where(c => c.CountryId == CurrentCountry).ToList();
            }
        }
        private void Country_DropDownOpened(object sender, EventArgs e)
        {
            Country.ItemsSource = a.Countries.ToList(); // освежимся 
        }
        private void LoadEmployee()
        {
            if (p is not null)
            {
                Tab.Text = Convert.ToString(p.Tab);
                SecondName.Text = p.SecondName;
                FirstName.Text = p.FirstName;
                ParentName.Text = p.ParentName;
                if (p.Gender is not null)
                {
                    Man.IsChecked = p.Gender == true; // p.Gender == true ? true : false
                    Woman.IsChecked = p.Gender == false;
                }
                // вот за это и боролся с привязкой DisplayMemberPath и SelectedValuePath
                // чтобы считать значение из базы беспроблемно и поставить
                Country.SelectedValue = p.CountryId is null ? -1 : (int)p.CountryId;
                Region.SelectedValue = p.RegionCode is null ? -1 : (int)p.RegionCode;
                Qualification.SelectedValue = p.QualifyId is null ? -1 : (int)p.QualifyId;
                Education.SelectedValue = p.EducationId is null ? -1 : (int)p.EducationId;
            }
        }
        private void SaveClick(object sender, RoutedEventArgs e)
        {
            if (p is not null)
            {
                p.Tab = Convert.ToInt32(Tab.Text);
                p.SecondName = SecondName.Text;
                p.FirstName = FirstName.Text;
                p.ParentName = ParentName.Text;
                if (Man.IsChecked == true)
                    p.Gender = true;
                if (Woman.IsChecked == true)
                    p.Gender = false;
                // вот за это и боролся с привязкой DisplayMemberPath и SelectedValuePath
                // чтобы обратно в базу поставить без доп поиска
                p.CountryId = (int)Country.SelectedValue;
                p.RegionCode = (int)Region.SelectedValue;
                p.QualifyId = (int)Qualification.SelectedValue;
                p.EducationId = (int)Education.SelectedValue;
                // да без обработки ошибок try / catch
                // иначе при ошибке в SQL он говорит просто InnerException
                // и непонятно как вывести SqlException
                if (CurrentTab == 0)
                {
                    a?.Employees.Add(p);
                }
                a?.SaveChanges();
                Status.Content = "Данные успешно сохранены";
            }
        }
        void CanExecuteSave(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            if (SecondName is not null && FirstName is not null &&
                Tab is not null &&
                Education is not null && Qualification is not null &&
                Country is not null && Region is not null)
            {
                if (SecondName.Text.Length > 1 && FirstName.Text.Length > 1 &&
                    Convert.ToInt32(Tab.Text) > 0 &&
                    Education.SelectedIndex != -1 && Qualification.SelectedIndex != -1 &&
                    Country.SelectedIndex != -1 && Region.SelectedIndex != -1)
                {
                    e.CanExecute = true;
                }
            }
        }
        private void Tab_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти из редактора ?",
                "Сохраните данные", MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.Yes) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}