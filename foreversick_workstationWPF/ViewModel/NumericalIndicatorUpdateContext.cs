using foreversick_workstationWPF.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace foreversick_workstationWPF.ViewModel
{
    class NumericalIndicatorUpdateContext: INotifyPropertyChanged, ICloseWindow
    {
        public ComboBoxDataContext<NumericalIndicator> NumericalIndicatorContext { get; set; } = new(NumericalIndicatorList.GetNumericalIndicatorListAsync,
                                                                "GameContext/NumericalIndicatorsBySubstring/",
                                                                "Не удалось загрузить список числовых индикаторов. Проверьте подключение к интернету и повторите попытку.");

        int diagnosis_id;
        NumericalIndicator old_numericalIndicator;
        double old_min;
        double old_max;
        public NumericalIndicatorUpdateContext(int diagnosis_id, NumericalIndicator numericalIndicator, double min, double max)
        {
            this.diagnosis_id = diagnosis_id;
            this.old_numericalIndicator = numericalIndicator;
            this.old_min = min;
            this.old_max = max;

            NumericalIndicatorContext.SelectedItemChanged += SelectedNumericalIndicatorChanged;
            NumericalIndicatorContext.SelectedItem = numericalIndicator;
            NumericalIndicatorContext.Combobox_text = numericalIndicator.ToString();
            MinValue = min.ToString();
            MaxValue = max.ToString();
        }
        void SelectedNumericalIndicatorChanged(Object sender, EventArgs e)
        {
            NumericalIndicator current = NumericalIndicatorContext.SelectedItem;
            NumericalIndicatorTooltip = (current != null) ? current.Tooltip : "Выберите индикатор";
        }

        string numericalIndicatorTooltip = "Выберите индикатор";
        public string NumericalIndicatorTooltip
        {
            get
            {
                return numericalIndicatorTooltip;
            }
            set
            {
                numericalIndicatorTooltip = value;
                OnPropertyChanged(nameof(NumericalIndicatorTooltip));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Action Close { get; set; }
        public bool CanClose()
        {
            return true;
        }

        private bool? dialogResult;
        public bool? DialogResult { get => dialogResult; set { dialogResult = value; OnPropertyChanged(nameof(DialogResult)); } }

        ICommand addNumericalIndicalorCommand;
        public ICommand AddNumericalIndicalorCommand
        {
            get
            {
                addNumericalIndicalorCommand = new RelayCommand(obj =>
                {
                    DataContext.CreateaddNumericalIndicalorCommand(TypeOfAction.Insert, NumericalIndicatorContext.Combobox_text);
                });
                return addNumericalIndicalorCommand;
            }
        }

        string minValue;
        string maxValue;
        public string MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                minValue = value;
                OnPropertyChanged(nameof(MinValue));
            }
        }
        public string MaxValue
        {
            get
            {
                return maxValue;
            }
            set
            {
                maxValue = value;
                OnPropertyChanged(nameof(MaxValue));
            }
        }

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
        ICommand updateButtonCommand;
        public ICommand UpdateButtonCommand
        {
            get
            {
                updateButtonCommand = new RelayCommand(obj =>
                {
                    Update();
                });
                return updateButtonCommand;
            }
        }

        async void Update()
        {
            NumericalIndicator current_indicator = NumericalIndicatorContext.SelectedItem;
            double min_value = 0, max_value = 0;

            // проверка минимального значения и максимального, всё ли с ними в порядке
            string whats_wrong = "";
            bool is_num_values_ok1 = double.TryParse(MinValue, out min_value);
            bool is_num_values_ok2 = double.TryParse(MaxValue, out max_value);
            if (is_num_values_ok1)
            {
                if (min_value > max_value)
                    whats_wrong += "Минимальное значение не может быть больше максимального!\n";
                if (current_indicator != null && (min_value < current_indicator.min_value || min_value > current_indicator.max_value))
                    whats_wrong += "Минимальное значение выходит за пределы допустимых значений.\n";
            }

            if (is_num_values_ok2 && current_indicator != null && (max_value < current_indicator.min_value || max_value > current_indicator.max_value))
                whats_wrong += "Максимальное значение выходит за пределы допустимых значений.\n";

            // если всё ок
            if (current_indicator != null && is_num_values_ok1 && is_num_values_ok2 && whats_wrong == "")
            {
                bool isExists = false;
                try
                {
                    // проверяем индикатор-диагноз на уникальность
                    if (current_indicator.indicator_id != old_numericalIndicator.indicator_id)
                    isExists = await NumericalIndicatorList.DiagnosisNumericalIndicatorValidation("GameContext/DiagnosisNumericalIndicatorValidation/",
                                                                                      diagnosis_id,
                                                                                      current_indicator.indicator_id);
                    if (isExists)
                    {
                        MessageBox.Show("Не удалось изменить числовой индикатор для диагноза. Выбранный индикатор уже указан для диагноза.");
                        return;
                    }
                    // проверяем, а меняется ли вообще что-то
                    if (current_indicator.indicator_id == old_numericalIndicator.indicator_id && old_max == max_value && old_min == min_value)
                    {
                        MessageBox.Show("Измените значения или индикатор, либо нажмите \"Отмена\".");
                        return;
                    }
                    // ХМММ А ВЫ УВЕРЕНЫ????
                    if (MessageBox.Show("Вы действительно ходите выполнить изменение?" +
                                "\nСтарый индикатор: " + old_numericalIndicator +
                                "\nНовый индикатор: " + current_indicator +
                                "\nСтарые значения минимума: " + old_min + ", максимума: " + old_max +
                                "\nНовые значения минимума: " + min_value + ", максимума: " + max_value, "Вы уверены?", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;
                    try
                    {
                        NumericalIndicatorInDiagnosis.UpdateNumericalIndicatorForDiagnosis("GameContext/NumericalIndicatorsUpdate/",
                                                                               old_numericalIndicator.indicator_id,
                                                                               new num_indicator_in_diagnosis() { diagnosis_id = diagnosis_id, 
                                                                                                                  indicator_id = current_indicator.indicator_id,
                                                                                                                  value_min = min_value,
                                                                                                                  value_max = max_value});
                        // сообщение об успешном добавлении
                        MessageBox.Show("Индикатор и значения изменены.");
                        // закрытие формы с дайлог резалт тру
                        DialogResult = true;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Не удалось добавить числовой индикатор к диагнозу. Попробуйте ещё раз. Ошибка: " + e.Message);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Не удалось проверить уникальность пары диагноз-индиктор. Попробуйте ещё раз. Ошибка: " + e.Message);
                }
            }
            else MessageBox.Show
                    ((current_indicator == null ? "Индикатор не выбран.\n" : "")
                   + whats_wrong
                   + (!is_num_values_ok1 || !is_num_values_ok2 ? "Минимальное и максимальное значения должны быть рациональными числами. В качестве разделителя используйте запятую.\n" : ""));

        }
    }
}
