using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace foreversick_workstationWPF
{
    public class ComboBoxDataContext<T> : INotifyPropertyChanged where T : IEquatable<T>
    {
        // таймеры для поиска
        private Timer _tmrDelaySearch;
        private const int DelayedTextChangedTimeout = 1000;

        string combobox_text;
        T selectedItem;
        ObservableCollection<T> searchListBySubstring = new();
        Func<string, Task<List<T>>> SearchItemsFunc;
        string searchPath;
        string errorMessage;
        /// <summary>
        /// Возвращает DataContext для комбобокса с регистронезависимым поиском по подстроке. Комбобокс странно работает, но работает.
        /// </summary>
        /// <param name="SearchItemsFunc">Асинхронная функция для поиска списка элементов с подстрокой в названии</param>
        /// <param name="selectedItemChange">Функция, выполняемая при изменении комбобокса</param>
        /// <param name="searchPath">Путь между главным URL и самой подстрокой, например, "GameContext/QuestionsBySubstring/"</param>
        /// <param name="errorMessage">Сообщение об ошибке, когда не удаётся найти список по подстроке. ТОЧКА В КОНЦЕ, ИНАЧЕ НЕКРАСИВО!!!</param>
        public ComboBoxDataContext(Func<string, Task<List<T>>> SearchItemsFunc, /*Action selectedItemChange,*/ string searchPath, string errorMessage)
        {
            this.SearchItemsFunc = SearchItemsFunc;
            //this.selectedItemChange = selectedItemChange;
            this.searchPath = searchPath;
            this.errorMessage = errorMessage;
        }

        public event EventHandler SelectedItemChanged;
        
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Текст комбобокса, куда будет производиться ввод и по которому будет поиск списка
        /// </summary>
        public string Combobox_text
        {
            get
            {
                return combobox_text;
            }
            set
            {
                combobox_text = value;
                OnPropertyChanged(nameof(Combobox_text));
            }
        }
        void OnPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(Combobox_text))
                OnItemChanged();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        void OnItemChanged()
        {
            if (_tmrDelaySearch != null)
                _tmrDelaySearch.Stop();

            if (_tmrDelaySearch == null)
            {
                _tmrDelaySearch = new Timer();
                _tmrDelaySearch.Elapsed += _tmrDelaySearch_TickAsync;
                _tmrDelaySearch.Interval = DelayedTextChangedTimeout;
            }

            _tmrDelaySearch.Start();
        }
        /// <summary>
        /// Список всех вариантов с подстрокой в названии
        /// </summary>
        public ObservableCollection<T> SearchListBySubstring
        {
            get
            {
                return searchListBySubstring;
            }
            set
            {
                searchListBySubstring = value;
                OnPropertyChanged(nameof(SearchListBySubstring));
            }
        }
        /// <summary>
        /// Выбранный элемент комбобокса
        /// </summary>
        public T SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                selectedItem = value;
                //selectedItemChange.Invoke();
                SelectedItemChanged?.Invoke(this, new EventArgs());
            }
        }
        async void _tmrDelaySearch_TickAsync(object sender, EventArgs e) 
        {
            Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
            if (_tmrDelaySearch != null)
                _tmrDelaySearch.Stop();
            ObservableCollection<T> old_search = new();
            ObservableCollection<T> new_search = new();
            if (!string.IsNullOrWhiteSpace(Combobox_text) && Combobox_text.Length > 2)
            {
                old_search = new(SearchListBySubstring.Where(x => !x.Equals(SelectedItem)).ToList<T>());
                try
                {
                    new_search = new(await SearchItemsFunc(searchPath + Combobox_text));
                }
                catch (Exception exc)
                {
                    MessageBox.Show(errorMessage + " Ошибка: " + exc.Message);
                }

                foreach (var old_item in old_search)
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                    {
                        searchListBySubstring.Remove(old_item);
                    }));
                foreach (var new_item in new_search)
                    if (selectedItem == null || !new_item.Equals(SelectedItem))
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                        {
                            searchListBySubstring.Add(new_item);
                        }));
            }
        }
    }
}
