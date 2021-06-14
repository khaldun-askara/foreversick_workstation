using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
