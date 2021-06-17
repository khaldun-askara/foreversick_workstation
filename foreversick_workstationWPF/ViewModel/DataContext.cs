using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using foreversick_workstationWPF.Model;

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
       
        public DataContext()
        {
            DiagnosisDataContext.SelectedItemChanged += (sender, e)=> SelectedDiagnosisChanged(sender, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region add buttons for questions and answers
        ICommand addQuestionCommand;
        public ICommand AddQuestionCommand
        {
            get
            {
                addQuestionCommand = new RelayCommand(obj =>
                    {
                        AddingQuesOrAnsContext<Question> addingFormContext = new(AddedType.Question, 
                                                                                 QuestionsDataContext.Combobox_text,
                                                                                 Question.PostQuestion,
                                                                                 "GameContext/Question/",
                                                                                 QuestionList.GetQuestionsListAsync,
                                                                                 "GameContext/QuestionsBySubstring/");
                        AddingQuestion addingQuestionForm = new()
                        {
                            DataContext = addingFormContext
                        };
                        bool? result = addingQuestionForm.ShowDialog();
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
                        AddingQuesOrAnsContext<Answer> addingFormContext = new(AddedType.Answer, 
                                                                                 AnswersDataContext.Combobox_text, 
                                                                                 Answer.PostAnswer,
                                                                                 "GameContext/Answer/",
                                                                                 AnswerList.GetAnswersListAsync,
                                                                                 "GameContext/AnswersBySubstring/");
                        AddingQuestion addingQuestionForm = new()
                        {
                            DataContext = addingFormContext
                        };
                        bool? result = addingQuestionForm.ShowDialog();
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
                catch(Exception e)
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
                catch(Exception e)
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
            try {
                UserSuggestions = new(await UserSuggestionList.GetSuggestionsForDiagnosis("GameContext/Suggestions/", diagnosis_id));

            }
            catch(Exception exc)
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
