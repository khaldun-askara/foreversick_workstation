using foreversick_workstationWPF.Model;
using System.Windows;

namespace foreversick_workstationWPF
{
    /// <summary>
    /// Логика взаимодействия для AddingEntitiesWindow.xaml
    /// </summary>
    public partial class AddingEntitiesWindow : Window
    {
        public AddingEntitiesWindow()
        {
            InitializeComponent();
        }

        public AddingEntitiesWindow(Diagnosis current_duagnosis)
        {
            InitializeComponent();
            diagnosisComboBox.Text = current_duagnosis.diagnosis_text;
            diagnosisComboBox.SelectedItem = current_duagnosis;
        }
    }
}
