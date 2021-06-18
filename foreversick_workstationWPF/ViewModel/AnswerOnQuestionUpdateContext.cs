using foreversick_workstationWPF.Model;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace foreversick_workstationWPF.ViewModel
{
    class AnswerOnQuestionUpdateContext : INotifyPropertyChanged, ICloseWindow
    {
        #region comboBox context
        public ComboBoxDataContext<Question> QuestionsDataContext { get; set; } = new(QuestionList.GetQuestionsListAsync,
                                                                "GameContext/QuestionsBySubstring/",
                                                                "Не удалось загрузить список вопросов. Проверьте подключение к интернету и повторите попытку.");
        public ComboBoxDataContext<Answer> AnswersDataContext { get; set; } = new(AnswerList.GetAnswersListAsync,
                                                                "GameContext/AnswersBySubstring/",
                                                                "Не удалось загрузить список ответов. Проверьте подключение к интернету и повторите попытку.");
        #endregion

        #region AQ adding buttons
        ICommand addQuestionCommand;
        public ICommand AddQuestionCommand
        {
            get
            {
                addQuestionCommand = new RelayCommand(obj =>
                {
                    DataContext.CreateAddAQCommand(AddedType.Question, QuestionsDataContext.Combobox_text);
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
                    DataContext.CreateAddAQCommand(AddedType.Answer, AnswersDataContext.Combobox_text);
                });
                return addAnswerCommand;
            }
        }
        #endregion


        public AnswerOnQuestionUpdateContext(int diagnosis_id, Question old_question, Answer old_answer)
        {
            this.diagnosis_id = diagnosis_id;
            QuestionsDataContext.Combobox_text = old_question.question_text;
            QuestionsDataContext.SelectedItem = old_question;
            this.old_question_id = old_question.id;
            this.old_question_text = old_question.question_text;
            AnswersDataContext.Combobox_text = old_answer.answer_text;
            AnswersDataContext.SelectedItem = old_answer;
            this.old_answer_id = old_answer.id;
            this.old_answer_text = old_answer.answer_text;
        }

        int diagnosis_id;
        int old_question_id;
        int old_answer_id;
        string old_question_text;
        string old_answer_text;

        ICommand updateAQButtonCommand;
        public ICommand UpdateAQButtonCommand
        {
            get
            {
                updateAQButtonCommand = new RelayCommand(obj =>
                {
                    Update();
                });
                return updateAQButtonCommand;
            }
        }

        private bool? dialogResult;
        public bool? DialogResult { get => dialogResult; set { dialogResult = value; OnPropertyChanged(nameof(DialogResult)); } }

        private async void Update()
        {
            MessageBox.Show("ты видишь заглушку из Update");
            Question current_question = QuestionsDataContext.SelectedItem;
            Answer current_answer = AnswersDataContext.SelectedItem;

            // проверить, выбраны ли в комбобоксах вопрос и ответ
            if (current_answer == null || current_question == null)
            {
                // если нет, выдать ошибку
                MessageBox.Show
                    ((current_answer == null ? "Ответ не выбран.\n" : "")
                   + (current_question == null ? "Вопрос не выбран.\n" : ""));
                return;
            }
            // проверить существование такого вопроса
            if (current_question.id != old_question_id)
            {
                bool isExists = true;
                try
                {
                    isExists = await QuestionOnAnswerList.DiagnosisQuestionValidation("GameContext/DiagnosisQuestionValidation/",
                                                                                      diagnosis_id,
                                                                                      current_question.id);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Не удалось проверить уникальность пары диагноз-вопрос. Попробуйте ещё раз. Ошибка: " + e.Message);
                }
                // если есть, выдать ошибку "всё хуйня давай по новой"
                if (isExists)
                {
                    MessageBox.Show("Не удалось обновить ответ на вопрос к диагнозу. Выбранный вопрос уже указан для этого диагноза.");
                    return;
                }
            }
            // если обновлять нечего, так и сказать
            if (old_question_id == current_question.id && old_answer_id == current_answer.id)
            {
                MessageBox.Show("Измените вопрос или ответ, либо нажмите \"Отмена\".");
                return;
            }
            // подтверждение ОЙ А ВЫ УВЕРЕНЫ
            if (MessageBox.Show("Вы действительно ходите выполнить изменение?" +
                                "\nСтарый вопрос: " + old_question_text +
                                "\nНовый вопрос: " + current_question.question_text +
                                "\nСтарый ответ: " + old_answer_text +
                                "\nНовый ответ: " + current_answer.answer_text, "Вы уверены?", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;
            // попытка апдейта
            try
            {
                QuestionOnAnswerList.UpdateAnswerOnQuestionForDiagnosis("GameContext/AnswerOnQuestionUpdate/",
                    diagnosis_id, old_question_id, current_question.id, current_answer.id);
            }
            // если эксепшн, выдать ошибку
            catch (Exception e)
            {
                MessageBox.Show("Не удалось обновить ответ на вопрос к диагнозу. Попробуйте ещё раз. Ошибка: " + e.Message);
            }
            // иначе сообщение об успешном добавлении
            MessageBox.Show("Вопрос и ответ изменены.");
            // закрытие формы с дайлог резалт тру
            DialogResult = true;
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
    }
}
