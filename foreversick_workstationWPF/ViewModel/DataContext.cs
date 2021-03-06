using foreversick_workstationWPF.Model;
using foreversick_workstationWPF.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace foreversick_workstationWPF.ViewModel
{
    public class DataContext : INotifyPropertyChanged, ICloseWindow
    {
        #region contexts
        public ComboBoxDataContext<Question> QuestionsDataContext { get; set; } = new(QuestionList.GetQuestionsListAsync,
                                                                "GameContext/QuestionsBySubstring/",
                                                                "Не удалось загрузить список вопросов. Проверьте подключение к интернету и повторите попытку.");
        public ComboBoxDataContext<Answer> AnswersDataContext { get; set; } = new(AnswerList.GetAnswersListAsync,
                                                                "GameContext/AnswersBySubstring/",
                                                                "Не удалось загрузить список ответов. Проверьте подключение к интернету и повторите попытку.");
        public ComboBoxDataContext<Diagnosis> DiagnosisDataContext { get; set; } = new(DiagnosisList.GetDiagnosisListAsync,
                                                                "GameContext/DiagnosesBySubstring/",
                                                                "Не удалось загрузить список диагнозов. Проверьте подключение к интернету и повторите попытку.");

        public ComboBoxDataContext<NumericalIndicator> NumericalIndicatorContext { get; set; } = new(NumericalIndicatorList.GetNumericalIndicatorListAsync,
                                                                "GameContext/NumericalIndicatorsBySubstring/",
                                                                "Не удалось загрузить список числовых индикаторов. Проверьте подключение к интернету и повторите попытку.");
        public ComboBoxDataContext<EnumeratedIndicator> EnumeratedIndicatorContext { get; set; } = new(EnumeratedIndicatorList.GetEnumeratedIndicatorListAsync,
                                                                "GameContext/EnumeratedIndicatorsBySubstring/",
                                                                "Не удалось загрузить список перечислимых индикаторов. Проверьте подключение к интернету и повторите попытку.");
        public ComboBoxDataContext<EnumeratedValue> EnumeratedValueContext { get; set; } = new(EnumeratedValueList.GetEnumeratedValueListAsync,
                                                                "GameContext/EnumeratedValuesBySubstring/",
                                                                "Не удалось загрузить список значений индикаторов. Проверьте подключение к интернету и повторите попытку.");

        #endregion

        // дурашка, это конструктор
        public DataContext()
        {
            // эта строчка нужна, чтобы подписаться на изменение диагноза и обрабатывать это в SelectedDiagnosisChanged
            DiagnosisDataContext.SelectedItemChanged += SelectedDiagnosisChanged;
            NumericalIndicatorContext.SelectedItemChanged += SelectedNumericalIndicatorChanged;
        }
        /// <summary>
        /// Вызывается при изменении диагноза. Тут происходит инициализация табличек сущностей и предложений пользователей
        /// </summary>
        void SelectedDiagnosisChanged(object sender, EventArgs e)
        {
            if (DiagnosisDataContext.SelectedItem != null)
            {
                int current_diagnosis = DiagnosisDataContext.SelectedItem.id;
                GetSuggestionsForDiagnosis(current_diagnosis);
                GetAswersAndQuestionsForDiagnosis(current_diagnosis);
                GetNumericalIndicatorsForDiagnosis(current_diagnosis);
                GetEnumeratedIndicatorsForDiagnosis(current_diagnosis);
            }
        }

        void SelectedNumericalIndicatorChanged(Object sender, EventArgs e)
        {
            NumericalIndicator current = NumericalIndicatorContext.SelectedItem;
            NumericalIndicatorTooltip = (current != null) ? current.Tooltip : "Выберите индикатор";
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

        ICommand toMainWindowCommand;
        public ICommand ToMainWindowCommand
        {
            get
            {
                toMainWindowCommand = new RelayCommand(obj =>
                {
                    MainWindow mainWindow = new();
                    mainWindow.Show();
                    Close?.Invoke();
                });
                return toMainWindowCommand;
            }
        }

        #region user suggestion listview handling
        ObservableCollection<UserSuggestion> userSuggestions = new();
        public ObservableCollection<UserSuggestion> UserSuggestions
        {
            get
            {
                return userSuggestions;
            }
            set
            {
                userSuggestions = value;
                OnPropertyChanged(nameof(UserSuggestions));
            }
        }

        async void GetSuggestionsForDiagnosis(int diagnosis_id)
        {
            try
            {
                UserSuggestions = new(await UserSuggestionList.GetSuggestionsForDiagnosis("GameContext/Suggestions/", diagnosis_id));

            }
            catch (Exception exc)
            {
                MessageBoxResult result = MessageBox.Show(exc.Message + ". Попробовать снова?", "Не удалось загрузить предложения по диагнозу", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        GetSuggestionsForDiagnosis(diagnosis_id);
                        break;
                }
            }
        }

        ICommand deleteSuggCommand;
        public ICommand DeleteSuggCommand
        {
            get
            {
                deleteSuggCommand = new RelayCommand(obj =>
                {
                    UserSuggestion current_suggestion = obj as UserSuggestion;
                    if (current_suggestion == null)
                        return;
                    MessageBoxResult dialog_result = MessageBox.Show("Вы уверены, что хотите удалить данное предложение?", "Удаление предложения пользователя", MessageBoxButton.YesNo);
                    if (dialog_result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            UserSuggestionList.DeleteSuggestion("GameContext/Suggestion/", current_suggestion.id);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Не удалось удалить. Ошибка: " + e.Message);
                        }
                        userSuggestions.Remove(current_suggestion);
                    }
                });
                return deleteSuggCommand;
            }
        }
        #endregion

        #region questions and answers tabitem
        #region add buttons for questions and answers
        /// <summary>
        /// Генерация (если это можно так назвать, хахах) команды для кнопочки
        /// </summary>
        /// <param name="type">Тип формы: про вопросы или про ответы?</param>
        /// <param name="text">Текст вопроса или ответа, который изначально будет в поле ввода</param>
        /// <returns>Возвращает команду, которая создаёт окно добавления вопроса или ответа и контекст к нему</returns>
        public static bool CreateAddAQCommand(AddedType type, string text)
        {
            AddingQuesOrAnsContext<Question> addingFormContextQ = new(type,
                                                                    text,
                                                                    Question.PostQuestion,
                                                                    "GameContext/Question/",
                                                                    QuestionList.GetQuestionsListAsync,
                                                                    "GameContext/QuestionsBySubstring/");
            AddingQuesOrAnsContext<Answer> addingFormContextA = new(type,
                                                                    text,
                                                                    Answer.PostAnswer,
                                                                    "GameContext/Answer/",
                                                                    AnswerList.GetAnswersListAsync,
                                                                    "GameContext/AnswersBySubstring/");
            AddingQuesOrAnsContext<EnumeratedValue> addingFormContextV = new(type,
                                                                    text,
                                                                    EnumeratedValue.PostEnumeratedValue,
                                                                    "GameContext/EnumeratedValue/",
                                                                    EnumeratedValueList.GetEnumeratedValueListAsync,
                                                                    "GameContext/EnumeratedValuesBySubstring/");
            AddingQuestion addingAQForm = new()
            {
                DataContext = (type == AddedType.Question) ? addingFormContextQ : (type == AddedType.Answer) ? addingFormContextA : addingFormContextV
            };
            bool? resultDialog = addingAQForm.ShowDialog();
            return (resultDialog.HasValue) ? resultDialog.Value : false;
        }

        ICommand addQuestionCommand;
        public ICommand AddQuestionCommand
        {
            get
            {
                addQuestionCommand = new RelayCommand(obj =>
                {
                    CreateAddAQCommand(AddedType.Question, QuestionsDataContext.Combobox_text);
                });
                return addQuestionCommand;
            }
        }

        ICommand addAnswerCommand;
        public ICommand AddAnswerCommand
        {
            get
            {
                addAnswerCommand = new RelayCommand(obj =>
                {
                    CreateAddAQCommand(AddedType.Answer, AnswersDataContext.Combobox_text);
                });
                return addAnswerCommand;
            }
        }
        ICommand addAnswerAndQuestionToDiagnosis;
        public ICommand AddAnswerAndQuestionToDiagnosis
        {
            get
            {
                addAnswerAndQuestionToDiagnosis = new RelayCommand(obj =>
                {
                    AddingAnswerAndQuestionToDiagnosis();
                });
                return addAnswerAndQuestionToDiagnosis;
            }
        }
        /// <summary>
        /// Перед добавлением ответа на вопрос к диагнозу производится проверка
        /// заполнения полей: если какой-то комбобокс не выбран, то об этом выйдет сообщение
        /// </summary>
        async void AddingAnswerAndQuestionToDiagnosis()
        {
            Diagnosis current_duagnosis = DiagnosisDataContext.SelectedItem;
            Answer current_answer = AnswersDataContext.SelectedItem;
            Question current_question = QuestionsDataContext.SelectedItem;
            if (current_duagnosis != null
                && current_answer != null
                && current_question != null)
            {
                bool isExists = true;
                try
                {
                    isExists = await QuestionOnAnswerList.DiagnosisQuestionValidation("GameContext/DiagnosisQuestionValidation/",
                                                                                      current_duagnosis.id,
                                                                                      current_question.id);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Не удалось проверить уникальность пары диагноз-вопрос. Попробуйте ещё раз. Ошибка: " + e.Message);
                }
                if (isExists)
                {
                    MessageBox.Show("Не удалось добавить ответ на вопрос к диагнозу. Этот вопрос уже указан для диагноза.");
                    return;
                }
                try
                {
                    int result = await QuestionOnAnswerList.PostAnswerOnQuestionForDiagnosis("GameContext/DiagnosisQuestionAnswer/",
                                                                           current_duagnosis.id,
                                                                           current_answer.id,
                                                                           current_question.id);
                    if (result > 0)
                        questionOnAnswers.Add(new(current_question.id, current_question.question_text, current_answer.id, current_answer.answer_text));
                    else
                        MessageBox.Show("Не удалось добавить ответ на вопрос к диагнозу. Попробуйте ещё раз.");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Не удалось добавить ответ на вопрос к диагнозу. Попробуйте ещё раз. Ошибка: " + e.Message);
                }
            }
            else MessageBox.Show
                    ((current_duagnosis == null ? "Диагноз не выбран.\n" : "")
                   + (current_answer == null ? "Ответ не выбран.\n" : "")
                   + (current_question == null ? "Вопрос не выбран.\n" : ""));
        }

        #endregion add buttons for questions and answers

        #region answer on question for diagnosis listview buttons handling
        ICommand editAQCommand;
        public ICommand EditAQCommand
        {
            get
            {
                editAQCommand = new RelayCommand(obj =>
                {
                    QuestionOnAnswer current_pair = obj as QuestionOnAnswer;
                    if (current_pair != null)
                    {
                        AnswerOnQuestionUpdateContext answerOnQuestionUpdateContext = new(DiagnosisDataContext.SelectedItem.id,
                                                                                            new Question(current_pair.question_id, current_pair.question_text),
                                                                                            new Answer(current_pair.answer_id, current_pair.answer_text));
                        AnswerOnQuestionUpdateWindow updateAQForm = new()
                        {
                            DataContext = answerOnQuestionUpdateContext
                        };
                        bool? resultDialog = updateAQForm.ShowDialog();
                        if (resultDialog.HasValue && resultDialog.Value)
                            GetAswersAndQuestionsForDiagnosis(DiagnosisDataContext.SelectedItem.id);
                    }
                });
                return editAQCommand;
            }
        }

        ICommand deleteAQCommand;
        public ICommand DeleteAQCommand
        {
            get
            {
                deleteAQCommand = new RelayCommand(obj =>
                {
                    QuestionOnAnswer current_pair = obj as QuestionOnAnswer;
                    MessageBoxResult dialog_result = MessageBox.Show("Вы уверены, что хотите удалить следующую запись:\nВопрос: \""
                        + current_pair.question_text + "\"\nОтвет: \"" + current_pair.answer_text
                        + "\"\nдля диагноза \"" + DiagnosisDataContext.SelectedItem + "\"", "Удаление записи", MessageBoxButton.YesNo);
                    if (dialog_result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            QuestionOnAnswerList.DeleteAnswerOnQuestionForDiagnosis("GameContext/AnswerOnQuestionDelete/",
                                                                                    DiagnosisDataContext.SelectedItem.id,
                                                                                    current_pair.question_id);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Не удалось удалить. Ошибка: " + e.Message);
                        }
                        questionOnAnswers.Remove(current_pair);
                    }
                });
                return deleteAQCommand;
            }
        }
        #endregion

        #region initialising table with answers on questions for diagnosis
        ObservableCollection<QuestionOnAnswer> questionOnAnswers = new();
        public ObservableCollection<QuestionOnAnswer> QuestionOnAnswers
        {
            get
            {
                return questionOnAnswers;
            }
            set
            {
                questionOnAnswers = value;
                OnPropertyChanged(nameof(QuestionOnAnswers));
            }
        }
        async void GetAswersAndQuestionsForDiagnosis(int diagnosis_id)
        {
            try
            {
                QuestionOnAnswers = new(await QuestionOnAnswerList.GetAswersAndQuestionsForDiagnosis("GameContext/AnswersOnQuestions/", diagnosis_id));
            }
            catch (Exception exc)
            {
                MessageBoxResult result = MessageBox.Show(exc.Message + ". Попробовать снова?", "Не удалось загрузить вопросы и ответы", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        GetAswersAndQuestionsForDiagnosis(diagnosis_id);
                        break;
                }
            }
        }
        #endregion
        #endregion questions and answers tabitem

        #region numerical indicators tabitem

        #region add buttons for numerical indicators
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

        ICommand addNumericalIndicalorCommand;
        public ICommand AddNumericalIndicalorCommand
        {
            get
            {
                addNumericalIndicalorCommand = new RelayCommand(obj =>
                {
                    CreateaddNumericalIndicalorCommand(TypeOfAction.Insert, NumericalIndicatorContext.Combobox_text);
                });
                return addNumericalIndicalorCommand;
            }
        }
        
        public static bool CreateaddNumericalIndicalorCommand(TypeOfAction type, string combobox_text)
        {
            AddingNumericalIndicatorContext context = new AddingNumericalIndicatorContext(combobox_text,
                                                                                          NumericalIndicatorList.PostNumericalIndicatorListAsync,
                                                                                          "GameContext/NumericalIndicator/",
                                                                                          NumericalIndicatorList.GetNumericalIndicatorListAsync,
                                                                                          "GameContext/NumericalIndicatorsBySubstring/");
            AddingNumericalIndicator addingNumericalIndicatorWindow = new()
            {
                DataContext = context
            };
            bool? resultDialog = addingNumericalIndicatorWindow.ShowDialog();
            return (resultDialog.HasValue) ? resultDialog.Value : false;
        }

        ICommand addNumericalIndicatorToDiagnosis;
        public ICommand AddNumericalIndicatorToDiagnosis
        {
            get
            {
                addNumericalIndicatorToDiagnosis = new RelayCommand(obj =>
                {
                    AddingNumericalIndicatorToDiagnosis();
                });
                return addNumericalIndicatorToDiagnosis;
            }
        }

        async void AddingNumericalIndicatorToDiagnosis()
        {
            Diagnosis current_diagnosis = DiagnosisDataContext.SelectedItem;
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
            if (current_diagnosis != null && current_indicator != null && is_num_values_ok1 && is_num_values_ok2 && whats_wrong == "")
            {
                bool isExists = true;
                try
                {
                    isExists = await NumericalIndicatorList.DiagnosisNumericalIndicatorValidation("GameContext/DiagnosisNumericalIndicatorValidation/",
                                                                                      current_diagnosis.id,
                                                                                      current_indicator.indicator_id);
                    if (isExists)
                    {
                        MessageBox.Show("Не удалось добавить числовой индикатор к диагнозу. Этот индикатор уже указан для диагноза.");
                        return;
                    }
                    try
                    {
                        int result = await NumericalIndicatorInDiagnosisList.PostNumericalIndicatorForDiagnosis("GameContext/NumericalIndicators/",
                                                                               current_diagnosis.id,
                                                                               current_indicator.indicator_id,
                                                                               min_value, max_value);
                        if (result > 0)
                            numericalIndicators.Add(new(current_diagnosis.id, current_indicator, min_value, max_value));
                        else
                            MessageBox.Show("Не удалось добавить числовой индикатор к диагнозу. Попробуйте ещё раз.");
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
                    ((current_diagnosis == null ? "Диагноз не выбран.\n" : "")
                   + (current_indicator == null ? "Индикатор не выбран.\n" : "")
                   + whats_wrong
                   + (!is_num_values_ok1 || !is_num_values_ok2 ? "Минимальное и максимальное значения должны быть рациональными числами. В качестве разделителя используйте запятую.\n" : ""));
        }

        #endregion

        #region initialising table with numerical indicators for diagnosis
        ObservableCollection<NumericalIndicatorInDiagnosis> numericalIndicators = new();
        public ObservableCollection<NumericalIndicatorInDiagnosis> NumericalIndicators { get => numericalIndicators; set { numericalIndicators = value; OnPropertyChanged(nameof(NumericalIndicators)); } }

        async void GetNumericalIndicatorsForDiagnosis(int diagnosis_id)
        {
            try
            {
                NumericalIndicators = new(await NumericalIndicatorInDiagnosisList.GetNumericalIndicatorsForDiagnosis("GameContext/NumericalIndicators/", diagnosis_id));
            }
            catch (Exception exc)
            {
                MessageBoxResult result = MessageBox.Show(exc.Message + ". Попробовать снова?", "Не удалось загрузить числовые индикаторы", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        GetNumericalIndicatorsForDiagnosis(diagnosis_id);
                        break;
                }
            }
        }
        #endregion initialising table with numerical indicators for diagnosis

        #region numerical indicator for diagnosis listview buttons handling
        ICommand editNICommand;
        public ICommand EditNICommand
        {
            get
            {
                editNICommand = new RelayCommand(obj =>
                {
                    NumericalIndicatorInDiagnosis current_indicator_id_diagnosis = obj as NumericalIndicatorInDiagnosis;
                    if (current_indicator_id_diagnosis != null)
                    {
                        NumericalIndicatorUpdateContext updateContext = new(DiagnosisDataContext.SelectedItem.id,
                                                                            current_indicator_id_diagnosis.indicator,
                                                                            current_indicator_id_diagnosis.value_min,
                                                                            current_indicator_id_diagnosis.value_max);

                        NumericalIndicatorUpdateWindow updateWindow = new()
                        {
                            DataContext = updateContext
                        };
                        bool? resultDialog = updateWindow.ShowDialog();
                        if (resultDialog.HasValue && resultDialog.Value)
                            GetNumericalIndicatorsForDiagnosis(DiagnosisDataContext.SelectedItem.id);
                    }
                });
                return editNICommand;
            }
        }

        ICommand deleteNICommand;
        public ICommand DeleteNICommand
        {
            get
            {
                deleteNICommand = new RelayCommand(obj =>
                {
                    NumericalIndicatorInDiagnosis current_indicator_id_diagnosis = obj as NumericalIndicatorInDiagnosis;
                    MessageBoxResult dialog_result = MessageBox.Show("Вы уверены, что хотите удалить следующую запись:\nИндикатор: \""
                        + current_indicator_id_diagnosis.indicator + "\"\nМинимальное значение: \"" + current_indicator_id_diagnosis.value_min + "\"\nМаксимальное значение: \"" + current_indicator_id_diagnosis.value_max
                        + "\"\nдля диагноза \"" + DiagnosisDataContext.SelectedItem + "\"", "Удаление записи", MessageBoxButton.YesNo) ;
                    if (dialog_result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            NumericalIndicatorInDiagnosis.DeleteNumericalInducatorDiagnosis("GameContext/NumericalIndicatorsDelete/",
                                                                                    DiagnosisDataContext.SelectedItem.id,
                                                                                    current_indicator_id_diagnosis.indicator.indicator_id);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Не удалось удалить. Ошибка: " + e.Message);
                        }
                        numericalIndicators.Remove(current_indicator_id_diagnosis);
                    }
                });
                return deleteNICommand;
            }
        }
        #endregion

        #endregion numerical indicators tabitem

        #region enumerated indicators tabitem
        #region add buttons for enumerated indicators
        ICommand addEnumeratedIndicalorCommand;
        public ICommand AddEnumeratedIndicalorCommand
        {
            get
            {
                addEnumeratedIndicalorCommand = new RelayCommand(obj =>
                {
                    MessageBox.Show("Буп!");
                });
                return addEnumeratedIndicalorCommand;
            }
        }
        ICommand addEnumeratedValueCommand;
        public ICommand AddEnumeratedValueCommand
        {
            get
            {
                addEnumeratedValueCommand = new RelayCommand(obj =>
                {
                    CreateAddAQCommand(AddedType.EnumValue, EnumeratedValueContext.Combobox_text);
                });
                return addEnumeratedValueCommand;
            }
        }

        ICommand addEnumeratedIndicatorToDiagnosis;
        public ICommand AddEnumeratedIndicatorToDiagnosis
        {
            get
            {
                addEnumeratedIndicatorToDiagnosis = new RelayCommand(obj =>
                {
                    AddingEnumeratedIndicatorToDiagnosis();
                });
                return addEnumeratedIndicatorToDiagnosis;
            }
        }

        async void AddingEnumeratedIndicatorToDiagnosis()
        {
            Diagnosis current_diagnosis = DiagnosisDataContext.SelectedItem;
            EnumeratedIndicator current_indicator = EnumeratedIndicatorContext.SelectedItem;
            EnumeratedValue current_value = EnumeratedValueContext.SelectedItem;
            if (current_diagnosis != null
                && current_indicator != null
                && current_value != null)
            {
                bool isExists = true;
                try
                {
                    isExists = await EnumeratedIndicatorInDiagnosisList.EnumeratedIndicatorValidation("GameContext/DiagnosisEnumeratedIndicatorValidation/",
                                                                                      current_diagnosis.id,
                                                                                      current_indicator.indicator_id,
                                                                                      current_value.value_id);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Не удалось проверить уникальность тройки диагноз-индикатор-значение. Попробуйте ещё раз. Ошибка: " + e.Message);
                }
                if (isExists)
                {
                    MessageBox.Show("Не удалось добавить значение перечислимого индикатора к диагнозу. Это значение индикатора уже указано для диагноза.");
                    return;
                }
                try
                {
                    int result = await EnumeratedIndicatorInDiagnosisList.PostEnumeratedIndicatorForDiagnosis("GameContext/EnumeratedIndicators/",
                                                                           current_diagnosis.id,
                                                                           current_indicator.indicator_id,
                                                                           current_value.value_id);
                    if (result > 0)
                        enumeratedIndicators.Add(new(current_diagnosis.id, current_indicator, current_value));
                    else
                        MessageBox.Show("Не удалось добавить значение индикатора к диагнозу. Попробуйте ещё раз.");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Не удалось добавить значение индикатора к диагнозу. Попробуйте ещё раз. Ошибка: " + e.Message);
                }
            }
            else MessageBox.Show
                    ((current_diagnosis == null ? "Диагноз не выбран.\n" : "")
                   + (current_indicator == null ? "Индикатор не выбран.\n" : "")
                   + (current_value == null ? "Значение не выбрано.\n" : ""));
        }
        #endregion add buttons for enumerated indicators
        #region initialising table with enumerated indicators for diagnosis
        ObservableCollection<EnumeratedIndicatorInDiagnosis> enumeratedIndicators = new();
        public ObservableCollection<EnumeratedIndicatorInDiagnosis> EnumeratedIndicators { get => enumeratedIndicators; set { enumeratedIndicators = value; OnPropertyChanged(nameof(EnumeratedIndicators)); } }
        async void GetEnumeratedIndicatorsForDiagnosis(int diagnosis_id)
        {
            try
            {
                EnumeratedIndicators = new(await EnumeratedIndicatorInDiagnosisList.GetEnumeratedIndicatorsForDiagnosis("GameContext/EnumeratedIndicators/", diagnosis_id));
            }
            catch (Exception exc)
            {
                MessageBoxResult result = MessageBox.Show(exc.Message + ". Попробовать снова?", "Не удалось загрузить перечислимые индикаторы", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        GetEnumeratedIndicatorsForDiagnosis(diagnosis_id);
                        break;
                }
            }
        }
        #endregion initialising table with enumerated indicators for diagnosis
        #endregion enumerated indicators tabitem
    }
}
