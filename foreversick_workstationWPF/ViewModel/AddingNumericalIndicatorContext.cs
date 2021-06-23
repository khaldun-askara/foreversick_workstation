using foreversick_workstationWPF.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace foreversick_workstationWPF.ViewModel
{
    public enum TypeOfAction
    {
        Insert,
        Update
    }

    public class AddingNumericalIndicatorContext : INotifyPropertyChanged, ICloseWindow
    {
        string title_text = "";
        public string Title_text
        {
            get { return title_text; }
            set
            {
                title_text = value; OnPropertyChanged(nameof(Title_text));
            }
        }
        string button_text = "";
        public string Button_text
        {
            get { return button_text; }
            set
            {
                button_text = value;
                OnPropertyChanged(nameof(Button_text));
            }
        }
        TypeOfAction action;

        int indicator_id;
        public int Indicator_id
        {
            get { return indicator_id; }
            set
            {
                indicator_id = value;
                OnPropertyChanged(nameof(Indicator_id));
            }
        }
        string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = AddingQuesOrAnsContext<Question>.FirstLetterToUp(value);
                OnPropertyChanged(nameof(Name));
            }
        }
        string min_value;
        public string Min_Value
        {
            get { return min_value; }
            set
            {
                min_value = value;
                OnPropertyChanged(nameof(Min_Value));
            }
        }
        string max_value;
        public string Max_Value
        {
            get { return max_value; }
            set
            {
                max_value = value;
                OnPropertyChanged(nameof(Max_Value));
            }
        }
        string normal_min;
        public string Normal_min
        {
            get { return normal_min; }
            set
            {
                normal_min = value;
                OnPropertyChanged(nameof(Normal_min));
            }
        }
        public string normal_max;
        public string Normal_max
        {
            get { return normal_max; }
            set
            {
                normal_max = value;
                OnPropertyChanged(nameof(Normal_max));
            }
        }
        string units_name;
        public string Units_name
        {
            get { return units_name; }
            set
            {
                units_name = value;
                OnPropertyChanged(nameof(Units_name));
            }
        }
        string accuracy;
        public string Accuracy
        {
            get { return accuracy; }
            set
            {
                accuracy = value;
                OnPropertyChanged(nameof(Accuracy));
            }
        }
        Func<string, string, double, double, double, double, string, int, Task<int>> addingNumericalIndicatorFunctionAsync;
        string adding_path;
        Func<string, Task<List<NumericalIndicator>>> textValidationFuncAsync;
        string validation_path;
        public AddingNumericalIndicatorContext()
        {
            this.action = TypeOfAction.Insert;
            SetTitleAndButtonText(action);
        }
        public AddingNumericalIndicatorContext(string numerical_indicator_text,
            Func<string, string, double, double, double, double, string, int, Task<int>> addingNumericalIndicatorFunctionAsync,
            string adding_path,
            Func<string, Task<List<NumericalIndicator>>> textValidationFuncAsync,
            string validation_path, TypeOfAction action = TypeOfAction.Insert)
        {
            this.Name = numerical_indicator_text;
            this.addingNumericalIndicatorFunctionAsync = addingNumericalIndicatorFunctionAsync;
            this.adding_path = adding_path;
            this.textValidationFuncAsync = textValidationFuncAsync;
            this.validation_path = validation_path;
            this.action = action;
            SetTitleAndButtonText(action);
        }

        private void SetTitleAndButtonText(TypeOfAction action)
        {
            switch (action)
            {
                case TypeOfAction.Insert:
                    Title_text = "Добавление числового индикатора";
                    Button_text = "Добавить";
                    break;
                case TypeOfAction.Update:
                    Title_text = "Изменение числового индикатора";
                    Button_text = "Изменить";
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        private bool? dialogResult;
        public bool? DialogResult { get => dialogResult; set { dialogResult = value; OnPropertyChanged(nameof(DialogResult)); } }

        ICommand actionButtonCommand;
        public ICommand ActionButtonCommand
        {
            get
            {
                actionButtonCommand = new RelayCommand(obj =>
                {
                    switch (action)
                    {
                        case TypeOfAction.Insert:
                            Adding();
                            break;
                        case TypeOfAction.Update:
                            Updating();
                            break;
                    }
                });
                return actionButtonCommand;
            }
        }

        async void Adding()
        {
            string whats_wrong = "";
            bool is_all_ok = true;
            // проверить корректность всех полей:
            // название не пустое
            if (string.IsNullOrWhiteSpace(Name))
            {
                whats_wrong += "Название числового индикатора не может быть пустым.\n";
                is_all_ok = false;
            }
            double minvalue, maxvalue, normmin, normmax;
            bool is_minvalue_ok = double.TryParse(Min_Value, out minvalue);
            bool is_maxvalue_ok = double.TryParse(Max_Value, out maxvalue);
            bool is_normmin_ok = double.TryParse(Normal_min, out normmin);
            bool is_normmax_ok = double.TryParse(Normal_max, out normmax);
            if (is_minvalue_ok && is_maxvalue_ok && is_normmin_ok && is_normmax_ok)
            {
                // минимальное значение рациональное и не больше максимального
                if (minvalue > maxvalue)
                {
                    whats_wrong += "Минимальное значение не может быть больше максимального.\n";
                    is_all_ok = false;
                }
                // максимальное значение рациональное
                // нормальное минимальное не меньше минимального, не больше нормального максимального и не больше максмимального
                if (normmin < minvalue || normmin > maxvalue)
                {
                    whats_wrong += "Минимальное нормальное значение выходит за пределы допустимых значений.\n";
                    is_all_ok = false;
                }
                if (normmin > normmax)
                {
                    whats_wrong += "Минимальное нормальное значение не может быть больше максимального.\n";
                    is_all_ok = false;
                }
                // номальное максимальное не меньше минимального, не больше максимального
                if (normmin < minvalue || normmin > maxvalue)
                {
                    whats_wrong += "Минимальное нормальное значение выходит за пределы допустимых значений.\n";
                    is_all_ok = false;
                }
            }
            else
            {
                whats_wrong += "Заполните числовые поля корректными значениями.\n";
                is_all_ok = false;
            }
            // единица измерения не пустая
            if (string.IsNullOrWhiteSpace(Units_name))
            {
                whats_wrong += "Название единиц измерений не может быть пустым.\n";
                is_all_ok = false;
            }
            // точность 0 или натуральная
            int accur;
            if (int.TryParse(Accuracy, out accur) && (accur < 0 || accur > 8))
            {
                whats_wrong += "Количество цифр после запятой должно быть от 0 до 8.\n";
                is_all_ok = false;
            }

            if (!is_all_ok)
            { MessageBox.Show(whats_wrong); return; }

            // проверить, существует ли такой показатель уже
            try
            {
                bool is_exists = (await textValidationFuncAsync(validation_path + Name)).Count != 0;
                if (is_exists)
                {
                    MessageBox.Show("Числовой индикатор с таким названием уже существует");
                    return;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Не удалось проверить название индикатора на уникальность. Ошибка: " + e.Message);
                return;
            }
            // попытаться добавить
            try
            {
                await addingNumericalIndicatorFunctionAsync(adding_path, Name, minvalue, maxvalue, normmin, normmax, Units_name, accur);
                MessageBox.Show("Числовой индикатор успешно добавлен.");
                DialogResult = true;
                Close?.Invoke();
            }
            // если не добавился, ошика
            catch (Exception e)
            {
                MessageBox.Show("Не удалось добавить числовой индикатор. Ошибка: " + e.Message);
            }

        }
        void Updating() { }

        ICommand cancelButtonCommand;

        public ICommand CancelButtonCommand
        {
            get
            {
                cancelButtonCommand = new RelayCommand(obj =>
                {
                    Close?.Invoke();
                });
                return cancelButtonCommand;
            }
        }


        public Action Close { get; set; }


        public bool CanClose()
        {
            return true;
        }
    }
}
