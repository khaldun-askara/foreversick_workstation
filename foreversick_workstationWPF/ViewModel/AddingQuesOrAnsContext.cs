using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foreversick_workstationWPF.ViewModel
{
    enum AddedType
    {
        Question,
        Answer
    }
    class AddingQuesOrAnsContext<T> : INotifyPropertyChanged
    {
        public AddingQuesOrAnsContext(AddedType current_type, string text)
        {
            this.current_type = current_type;
            this.Name = text;

            switch(current_type)
            {
                case AddedType.Question:
                    Label_text = "вопроса";
                    break;
                case AddedType.Answer:
                    Label_text = "ответа";
                    break;
            }

            Title_text += Label_text;
        }

        AddedType current_type;
        public string Label_text { get; set; }
        public string Title_text { get; set; } = "Добавление ";

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; set; }
    }
}
