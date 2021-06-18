﻿using foreversick_workstationWPF.Model;
using foreversick_workstationWPF.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace foreversick_workstationWPF.ViewModel
{
    public class DataContext : INotifyPropertyChanged
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

        #endregion

        // дурашка, это конструктор
        public DataContext()
        {
            // эта строчка нужна, чтобы подписаться на изменение диагноза и обрабатывать это в SelectedDiagnosisChanged
            DiagnosisDataContext.SelectedItemChanged += (sender, e) => SelectedDiagnosisChanged(sender, e);
        }
        /// <summary>
        /// Вызывается при изменении диагноза. Тут происходит инициализация табличек сущностей и предложений пользователей
        /// </summary>
        void SelectedDiagnosisChanged(object sender, EventArgs e)
        {
            //MessageBox.Show("SelectedDiagnosisChanged говорит, что DiagnosisDataContext.SelectedItem сменилось на "
            //    + ((DiagnosisDataContext.SelectedItem != null) ? DiagnosisDataContext.SelectedItem.ToString() : "null"));
            if (DiagnosisDataContext.SelectedItem != null)
            {
                GetSuggestionsForDiagnosis(DiagnosisDataContext.SelectedItem.id);
                GetAswersAndQuestionsForDiagnosis(DiagnosisDataContext.SelectedItem.id);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
            AddingQuestion addingAQForm = new()
            {
                DataContext = (type == AddedType.Question) ? addingFormContextQ : addingFormContextA
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

        #endregion

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
    }
}
