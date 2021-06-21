using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace foreversick_workstationWPF.Model
{
    [Serializable]
    public class AnalysisList
    {
        public List<Analysis> analyses { get; set; }
        public AnalysisList() { analyses = new List<Analysis>(); }
        public AnalysisList(List<Analysis> questions)
        {
            this.analyses = questions;
        }
        public void Add(Analysis analysis)
        {
            analyses.Add(analysis);
        }
    }
    [Serializable]
    public class Analysis
    {
        public int id { get; set; }
        public string analysis_text { get; set; }
        public bool is_enum { get; set; }

        public Analysis() { }

        public Analysis(int id, string analysis_text, bool is_enum)
        {
            this.id = id;
            this.analysis_text = analysis_text;
            this.is_enum = is_enum;
        }
    }

    [Serializable]
    public class NumericalIndicatorList
    {
        public List<NumericalIndicator> numericalIndicators { get; set; }
        public NumericalIndicatorList() { numericalIndicators = new List<NumericalIndicator>(); }
        public NumericalIndicatorList(List<NumericalIndicator> numericalIndicators)
        {
            this.numericalIndicators = numericalIndicators;
        }
        public void Add(NumericalIndicator indicator)
        {
            numericalIndicators.Add(indicator);
        }

        public static async Task<List<NumericalIndicator>> GetNumericalIndicatorListAsync(string path)
        {
            List<NumericalIndicator> NumericalIndicators = new();
            WebRequest request = WebRequest.Create(App.HOST_URL + path);
            WebResponse response = await request.GetResponseAsync();
            string numindJSON = string.Empty;
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new(stream))
                numindJSON = reader.ReadToEnd();
            NumericalIndicators = JsonSerializer.Deserialize<NumericalIndicatorList>(numindJSON).numericalIndicators;
            response.Close();
            return NumericalIndicators;
        }

        /// <summary>
        /// Проверка существования пары диагноз-индикатор
        /// </summary>
        /// <param name="path">Адрес</param>
        /// <returns>Возвращает true, если пара диагноз-индикатор есть, false, если ещё нет</returns>
        public static async Task<bool> DiagnosisNumericalIndicatorValidation(string path, int diagnosis_id, int indicator_id)
        {
            int res = 1;
            WebRequest request = WebRequest.Create(App.HOST_URL + path + diagnosis_id + "-" + indicator_id);
            WebResponse response = await request.GetResponseAsync();
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new(stream))
                int.TryParse(reader.ReadToEnd(), out res);
            response.Close();
            return res > 0;
        }
    }
    [Serializable]
    public class NumericalIndicator : IEquatable<NumericalIndicator>
    {
        public int indicator_id { get; set; }
        public string name { get; set; }
        public double min_value { get; set; }
        public double max_value { get; set; }

        public double normal_min { get; set; }
        public double normal_max { get; set; }
        public string units_name { get; set; }
        public int accuracy { get; set; }

        public NumericalIndicator() { }

        public NumericalIndicator(int indicator_id, string name, double min_value, double max_value, double normal_min, double normal_max, string units_name, int accuracy)
        {
            this.indicator_id = indicator_id;
            this.name = name;
            this.min_value = min_value;
            this.max_value = max_value;
            this.normal_min = normal_min;
            this.normal_max = normal_max;
            this.units_name = units_name;
            this.accuracy = accuracy;
        }

        public bool Equals(NumericalIndicator other)
        {
            return other != null && this.indicator_id == other.indicator_id;
        }

        public override string ToString()
        {
            return this.name;
        }

        public string Tooltip
        {
            get { return "Допустимый промежуток: от " + min_value + " до " + max_value + " " + units_name; }
        }
    }

    public class NumericalIndicatorInDiagnosis
    {
        public int diagnosis_id { get; set; }
        public NumericalIndicator indicator { get; set; }
        public double value_min { get; set; }
        public double value_max { get; set; }
        public NumericalIndicatorInDiagnosis() { }
        public NumericalIndicatorInDiagnosis(int diagnosis_id, NumericalIndicator indicator, double value_min, double value_max)
        {
            this.diagnosis_id = diagnosis_id;
            this.indicator = indicator;
            this.value_min = value_min;
            this.value_max = value_max;
        }

        public override string ToString()
        {
            return indicator.name;
        }
    }

    public class NumericalIndicatorInDiagnosisList
    {
        public List<NumericalIndicatorInDiagnosis> numericalIndicators { get; set; }
        public NumericalIndicatorInDiagnosisList() { numericalIndicators = new List<NumericalIndicatorInDiagnosis>(); }
        public NumericalIndicatorInDiagnosisList(List<NumericalIndicatorInDiagnosis> numericalIndicators)
        {
            this.numericalIndicators = numericalIndicators;
        }
        public void Add(NumericalIndicatorInDiagnosis indicator)
        {
            numericalIndicators.Add(indicator);
        }

        public static async Task<List<NumericalIndicatorInDiagnosis>> GetNumericalIndicatorsForDiagnosis(string path, int diagnosis_id)
        {
            List<NumericalIndicatorInDiagnosis> NumericalIndicators = new();
            WebRequest request = WebRequest.Create(App.HOST_URL + path + diagnosis_id);
            WebResponse response = await request.GetResponseAsync();
            string NumericalIndicatorsJSON = string.Empty;
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new(stream))
                NumericalIndicatorsJSON = reader.ReadToEnd();
            NumericalIndicators = JsonSerializer.Deserialize<NumericalIndicatorInDiagnosisList>(NumericalIndicatorsJSON).numericalIndicators;
            response.Close();
            return NumericalIndicators;
        }

        public static async Task<int> PostNumericalIndicatorForDiagnosis(string path, int diagnosis_id, int indicator_id, double min_value, double max_value)
        {
            int result = -1;
            num_indicator_in_diagnosis new_answer = new()
            {
                diagnosis_id = diagnosis_id,
                indicator_id = indicator_id,
                value_min = min_value,
                value_max = max_value
            };
            WebRequest request = WebRequest.Create(App.HOST_URL + path);
            request.Method = "POST";
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
            string result_stringJSON = JsonSerializer.Serialize<num_indicator_in_diagnosis>(new_answer, options);
            byte[] byteArray = Encoding.UTF8.GetBytes(result_stringJSON);
            request.ContentLength = byteArray.Length;
            request.ContentType = "application/json; charset = UTF-8";
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = await request.GetResponseAsync();
            using Stream dataStreamResponse = response.GetResponseStream();
            StreamReader reader = new(dataStreamResponse);
            if (!int.TryParse(reader.ReadToEnd(), out result))
                result = -1;
            reader.Close();
            dataStreamResponse.Close();
            return result;
        }
    }

    public class num_indicator_in_diagnosis
    {
        public int diagnosis_id { get; set; }
        public int indicator_id { get; set; }
        public double value_min { get; set; }
        public double value_max { get; set; }
    }
}
