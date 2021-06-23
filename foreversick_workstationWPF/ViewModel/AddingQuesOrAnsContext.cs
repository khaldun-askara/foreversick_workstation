using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace foreversick_workstationWPF.ViewModel
{
    public enum AddedType
    {
        Question,
        Answer
    }

    class AddingQuesOrAnsContext<T> : INotifyPropertyChanged, ICloseWindow
    {
        /// <summary>
        /// Возвращает vm для формы добавления варианта вопроса или ответа
        /// </summary>
        /// <param name="action">Изменение или добавление?</param>
        /// <param name="current_type">Тип: форма для вопроса или ответа</param>
        /// <param name="textOfQuesOrAns">Текст вопроса или ответа</param>
        /// <param name="addingQuesOrAnsFunctionAsync">Функция для добавления вопроса или ответа на сервер</param>
        /// <param name="adding_path">Какой путь засунуть в функцию добавления? Пример: GameContext/Question/</param>
        /// <param name="textValidationFuncAsync">Функция для проверки текста вопроса и ответа (функция поиска по подстроке, чтобы если найдётся такой же ответ, сказать об этом пользователю)</param>
        /// <param name="validation_path">Какой путь засунуть в функцию проверки? Пример: GameContext/QuestionBySubstring/</param>
        public AddingQuesOrAnsContext(AddedType current_type,
            string textOfQuesOrAns,
            Func<string, string, Task<int>> addingQuesOrAnsFunctionAsync,
            string adding_path,
            Func<string, Task<List<T>>> textValidationFuncAsync,
            string validation_path, TypeOfAction action = TypeOfAction.Insert)
        {
            this.action = action;
            SetTitleAndButtonText(action);
            this.current_type = current_type;
            this.Name = textOfQuesOrAns;

            switch (current_type)
            {
                case AddedType.Question:
                    quesOrAnsWord = "вопрос";
                    break;
                case AddedType.Answer:
                    quesOrAnsWord = "ответ";
                    break;
            }
            Label_text = quesOrAnsWord + "а";
            this.addingQuesOrAnsFunctionAsync = addingQuesOrAnsFunctionAsync;
            this.adding_path = adding_path;
            this.textValidationFuncAsync = textValidationFuncAsync;
            this.validation_path = validation_path;
        }

        private void SetTitleAndButtonText(TypeOfAction action)
        {
            switch (action)
            {
                case TypeOfAction.Insert:
                    Title_text = "Добавление ";
                    Button_text = "Добавить";
                    break;
                case TypeOfAction.Update:
                    Title_text = "Изменение ";
                    Button_text = "Изменить";
                    break;
            }
            Title_text += Label_text;
        }

        TypeOfAction action;
        AddedType current_type;
        private bool? dialogResult;
        private string label_text;
        private string title_text = "";
        private string button_text = "";
        public bool? DialogResult { get => dialogResult; set { dialogResult = value; OnPropertyChanged(nameof(DialogResult)); } }
        public string Label_text { get => label_text; set { label_text = value; OnPropertyChanged(nameof(Label_text)); } }
        public string Title_text { get => title_text; set { title_text = value; OnPropertyChanged(nameof(Title_text)); } }
        public string Button_text { get => button_text; set { button_text = value; OnPropertyChanged(nameof(Button_text)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        Func<string, string, Task<int>> addingQuesOrAnsFunctionAsync;
        string adding_path;
        Func<string, Task<List<T>>> textValidationFuncAsync;
        string validation_path;
        string quesOrAnsWord;
        string name;
        public string Name {
            get 
            { 
                return name; 
            } 
            set 
            {
                name = FirstLetterToUp(value);
                OnPropertyChanged(nameof(Name));
            }
        }

        ICommand addButtonCommand;
        public ICommand AddButtonCommand
        {
            get
            {
                addButtonCommand = new RelayCommand(obj =>
                {
                    Adding();
                });
                return addButtonCommand;
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

        public Action Close { get; set; }

        /// <summary>
        /// Проверка поля с вопросом/ответом Name на пустоту, на существование в бд, затем отправка.
        /// </summary>
        async void Adding()
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                bool is_name_valid = false;
                try
                {
                    is_name_valid = await NameValidation();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Не удалось проверить " + quesOrAnsWord + " на корректность. Попробуйте ещё раз. Ошибка: " + e.Message);
                }
                if (is_name_valid)
                {
                    int result = -1;
                    try
                    {
                        result = await addingQuesOrAnsFunctionAsync(Name, adding_path);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Не удалось отправить " + quesOrAnsWord + ". Попробуйте ещё раз. Ошибка: " + e.Message);
                    }
                    DialogResult = result != -1;
                    Close?.Invoke();
                }
            }
            else
            {
                MessageBox.Show("Временное предупреждение, которое надо переделать: " + ((string.IsNullOrWhiteSpace(Name)) ? "у вас пустая строка, вы дурачок" : "такой " + quesOrAnsWord + " уже есть!!!"));
            }
        }

        /// <summary>
        /// Проверка текста в форме на существование такого же вопроса или ответа
        /// </summary>
        /// <returns>Возвращает true, если всё в порядке (такого вопроса или ответа ещё нет), иначе false</returns>
        async Task<bool> NameValidation()
        {
            return (await textValidationFuncAsync(validation_path + Name)).Count == 0;
        }

        public bool CanClose()
        {
            return true;
        }

        public static string FirstLetterToUp(string substring)
        {
            if (string.IsNullOrWhiteSpace(substring))
                return string.Empty;
            return Regex.Replace(substring.ToLower(), @"^[a-zа-яё]", m => m.Value.ToUpper()); 
        }
    }
}
