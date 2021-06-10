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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace foreversick_workstationWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // тут происходит загрузка списка диагнозов с предложениями,
            // а потом этот список засовывается в листвью
            InitialiseListViewWithUserSuggestions();
        }

        private async void InitialiseListViewWithUserSuggestions()
        {
            //var diagnosestemp = new Diagnosis[]
            //{ new Diagnosis(1, "diagnosis1diagnosis1diagnosis1diagnosis1diagnosis1diagnosis1diagnosis1diagnosis1diagnosis1diagnosis1diagnosis1diagnosis1diagnosis1diagnosis1diagnosis1diagnosis1diagnosis1diagnosis1diagnosis1", "mkb-1"),
            //    new Diagnosis(1, "diagnosis1", "mkb-1"),
            //    new Diagnosis(1, "diagnosis1", "mkb-1") };
            //listOfSuggestedDiagnoses.ItemsSource = new List<Diagnosis>(diagnosestemp);
            try
            {
                listOfSuggestedDiagnoses.ItemsSource = await DiagnosisList.GetDiagnosisListAsync("GameContext/Diagnoses/suggestions");
            }
            catch (Exception e)
            {
                MessageBoxResult result = MessageBox.Show(e.Message + ". Попробовать снова?", "Что-то не так", MessageBoxButton.YesNo);
                switch(result)
                {
                    case MessageBoxResult.Yes:
                        InitialiseListViewWithUserSuggestions();
                        break;
                    case MessageBoxResult.No:
                        this.Close();
                        break;
                }
            }

        }

        private async void listOfSuggestedDiagnoses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            diagnosisSuggestion.Inlines.Clear();
            diagnosisSuggestion.Inlines.Add(new Run() { Text = "Загрузка...", Style = Application.Current.FindResource("baseRun") as Style });
            //List<Run> temp = await Task.Run(()=> UserSuggestionList.GetSuggestionsForDiagnosis(115));
            try
            {
                List<Inline> temp = await UserSuggestionList.GetSuggestionsForDiagnosis((listOfSuggestedDiagnoses.SelectedItem as Diagnosis).id);
                diagnosisSuggestion.Inlines.Clear();
                foreach (var t in temp)
                    diagnosisSuggestion.Inlines.Add(t);
            }
            catch(Exception exc)
            {
                MessageBoxResult result = MessageBox.Show(exc.Message + ". Попробовать снова?", "Что-то не так", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        listOfSuggestedDiagnoses_SelectionChanged(sender, e);
                        break;
                    //case MessageBoxResult.No:
                    //    this.Close();
                    //    break;
                }
            }
        }
    }
}
