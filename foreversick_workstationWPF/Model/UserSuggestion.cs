using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace foreversick_workstationWPF.Model
{
    [Serializable]
    public class UserSuggestionList
    {
        public List<UserSuggestion> userSuggestions { get; set; }
        public UserSuggestionList() { userSuggestions = new List<UserSuggestion>(); }
        public UserSuggestionList(List<UserSuggestion> userSuggestions)
        {
            this.userSuggestions = userSuggestions;
        }
        public void Add(UserSuggestion userSuggestion)
        {
            userSuggestions.Add(userSuggestion);
        }

        public static async Task<List<UserSuggestion>> GetSuggestionsForDiagnosis(string path, int diagnosis_id)
        {
            List<UserSuggestion> Suggestions = new();
            WebRequest request = WebRequest.Create(App.HOST_URL + path + diagnosis_id);
            WebResponse response = await request.GetResponseAsync();
            string suggestJSON = string.Empty;
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new(stream))
                suggestJSON = reader.ReadToEnd();
            Suggestions = JsonSerializer.Deserialize<UserSuggestionList>(suggestJSON).userSuggestions;
            response.Close();
            return Suggestions;
        }
        public static async void DeleteSuggestion(string path, int id)
        {
            WebRequest request = WebRequest.Create(App.HOST_URL + path
                                                    + id);
            request.Method = "DELETE";
            await request.GetResponseAsync();
        }
        public static async Task<List<Inline>> GetSuggestionsForDiagnosis(int diagnosis_id)
        {
            List<UserSuggestion> suggestions = await Task.Run(() => UserSuggestionList.GetSuggestionsForDiagnosis("GameContext/Suggestions/", diagnosis_id));
            List<Inline> result = new();
            List<Inline> symptomsList = new();
            List<Inline> visualsList = new();
            List<Inline> quesansList = new();
            foreach (UserSuggestion suggestion in suggestions)
            {
                if (!string.IsNullOrWhiteSpace(suggestion.symptoms))
                {
                    symptomsList.Add(new Run()
                    {
                        Text = suggestion.symptoms,
                        Style = Application.Current.FindResource("baseRun") as Style
                    });
                    symptomsList.Add(new LineBreak());
                }

                if (!string.IsNullOrWhiteSpace(suggestion.visible_signs))
                {
                    visualsList.Add(new Run()
                    {
                        Text = suggestion.visible_signs,
                        Style = Application.Current.FindResource("baseRun") as Style
                    });
                    visualsList.Add(new LineBreak());
                }
                if (!string.IsNullOrWhiteSpace(suggestion.questions_and_answers))
                {
                    quesansList.Add(new Run()
                    {
                        Text = suggestion.questions_and_answers,
                        Style = Application.Current.FindResource("baseRun") as Style
                    });
                    quesansList.Add(new LineBreak());
                }
            }
            if (symptomsList.Count > 0)
            {
                result.Add(new Run()
                {
                    Text = "Симптомы:",
                    Style = Application.Current.FindResource("boldRun") as Style
                });
                result.Add(new LineBreak());
            }
            result.AddRange(symptomsList);
            if (visualsList.Count > 0)
            {
                result.Add(new Run()
                {
                    Text = "Визуальные проявления:",
                    Style = Application.Current.FindResource("boldRun") as Style
                });
                result.Add(new LineBreak());
            }
            result.AddRange(visualsList);
            if (quesansList.Count > 0)
            {
                result.Add(new Run()
                {
                    Text = "Вопросы и ответы:",
                    Style = Application.Current.FindResource("boldRun") as Style
                });
                result.Add(new LineBreak());
            }
            result.AddRange(quesansList);
            return result;
        }
    }
    [Serializable]
    public class UserSuggestion
    {
        public UserSuggestion()
        {
        }

        public UserSuggestion(int id, int diagnosis_id, string symptoms, string visible_signs, string questions_and_answers)
        {
            this.id = id;
            this.diagnosis_id = diagnosis_id;
            this.symptoms = symptoms;
            this.visible_signs = visible_signs;
            this.questions_and_answers = questions_and_answers;
        }

        public int id { get; set; }
        public int diagnosis_id { get; set; }
        public string symptoms { get; set; }
        public string visible_signs { get; set; }
        public string questions_and_answers { get; set; }
    }
}
