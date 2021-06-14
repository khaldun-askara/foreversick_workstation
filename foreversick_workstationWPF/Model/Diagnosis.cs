using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace foreversick_workstationWPF
{
    [Serializable]
    public class DiagnosisList
    {
        public List<Diagnosis> diagnoses { get; set; }
        public DiagnosisList() { diagnoses = new List<Diagnosis>(); }
        public DiagnosisList(List<Diagnosis> diagnoses)
        {
            this.diagnoses = diagnoses;
        }
        public void Add(Diagnosis diagnosis)
        {
            diagnoses.Add(diagnosis);
        }

        public static int CurrentDiagnosis { get; set; }
        //public static List<Diagnosis> DiagnosesWithSuggestions { get; set; }
        public static async Task<List<Diagnosis>> GetDiagnosisListAsync(string path)
        {
            List<Diagnosis> DiagnosesWithSuggestions = new();
            WebRequest request = WebRequest.Create(App.HOST_URL + path);
            WebResponse response = await request.GetResponseAsync();
            string diagJSON = string.Empty;
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new(stream))
                diagJSON = reader.ReadToEnd();
            DiagnosesWithSuggestions = JsonSerializer.Deserialize<DiagnosisList>(diagJSON).diagnoses;
            response.Close();
            return DiagnosesWithSuggestions;
        }
        public static async Task<List<Diagnosis>> Test()
        {
            DiagnosisList result = new DiagnosisList();
            //WebRequest request = WebRequest.Create(MainWindow.host_url + path);
            WebRequest request = WebRequest.Create("https://localhost:44320/GameContext/Diagnoses/suggestions");
            try
            {
                MessageBox.Show("перед авейтом");
                WebResponse response = await request.GetResponseAsync();
                MessageBox.Show("после авейта");
                string diagJSON = string.Empty;
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                    diagJSON = reader.ReadToEnd();
                MessageBox.Show("diagJSON = reader.ReadToEnd(); ");
                result = JsonSerializer.Deserialize<DiagnosisList>(diagJSON);
                response.Close();
                MessageBox.Show("response.Close();");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            MessageBox.Show("конец");
            return result.diagnoses;
        }
    }
    [Serializable]
    public class Diagnosis : IEquatable<Diagnosis>
    {
        public int id { get; set; }
        public string diagnosis_text { get; set; }
        public string mcb_code { get; set; }
        public Diagnosis() { }
        public Diagnosis(int id, string diagnosis_text, string mcb_code)
        {
            this.id = id;
            this.diagnosis_text = diagnosis_text;
            this.mcb_code = mcb_code;
        }
        public override string ToString() 
        {
            return diagnosis_text;
        }

        public bool Equals(Diagnosis other)
        {
            return (other != null) && id == other.id;
        }
    }
}
