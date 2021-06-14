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
using foreversick_workstationWPF.ViewModel;

namespace foreversick_workstationWPF
{
    public class DataContext : INotifyPropertyChanged
    {
        public ComboBoxDataContext<Question> QuestionsDataContext { get; set; } = new(QuestionList.GetQuestionsListAsync,
                                                                "GameContext/QuestionsBySubstring/",
                                                                "Не удалось загрузить список вопросов. Проверьте подключение к интернету и повторите попытку.");
        public ComboBoxDataContext<Answer> AnswersDataContext { get; set; } = new(AnswerList.GetAnswersListAsync,
                                                                "GameContext/AnswersBySubstring/",
                                                                "Не удалось загрузить список ответов. Проверьте подключение к интернету и повторите попытку.");
        public ComboBoxDataContext<Diagnosis> DiagnosisDataContext { get; set; } = new(DiagnosisList.GetDiagnosisListAsync,
                                                                "GameContext/DiagnosesBySubstring/",
                                                                "Не удалось загрузить список диагнозов. Проверьте подключение к интернету и повторите попытку.");


        public DataContext()
        {
            DiagnosisDataContext.SelectedItemChanged += (sender, e)=> SelectedDiagnosisChanged(sender, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        ICommand addQuestionCommand;
        public ICommand AddQuestionCommand
        {
            get
            {
                addQuestionCommand = new RelayCommand(obj =>
                    {
                        AddingQuesOrAnsContext<Question> addingFormContext = new(AddedType.Question, QuestionsDataContext.Combobox_text);
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
                        AddingQuesOrAnsContext<Question> addingFormContext = new(AddedType.Answer, AnswersDataContext.Combobox_text);
                        AddingQuestion addingQuestionForm = new()
                        {
                            DataContext = addingFormContext
                        };
                        bool? result = addingQuestionForm.ShowDialog();
                    });
                return addAnswerCommand;
            }
        }
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

        void SelectedQuestionChanged()
        {

        }

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


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
