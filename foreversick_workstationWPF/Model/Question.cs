using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace foreversick_workstationWPF.Model
{
    [Serializable]
    public class QuestionList
    {
        public List<Question> questions { get; set; }
        public QuestionList() { questions = new List<Question>(); }
        public QuestionList(List<Question> questions)
        {
            this.questions = questions;
        }
        public void Add(Question question)
        {
            questions.Add(question);
        }

        public static async Task<List<Question>> GetQuestionsListAsync(string path)
        {
            List<Question> questions = new();
            WebRequest request = WebRequest.Create(App.HOST_URL + path);
            WebResponse response = await request.GetResponseAsync();
            string quesJSON = string.Empty;
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new(stream))
                quesJSON = reader.ReadToEnd();
            questions = JsonSerializer.Deserialize<QuestionList>(quesJSON).questions;
            response.Close();
            return questions;
        }
    }
    [Serializable]
    public class Question : IEquatable<Question>
    {
        public int id { get; set; }
        public string question_text { get; set; }
        public Question() { }
        public Question(int id, string question_text)
        {
            this.id = id;
            this.question_text = question_text;
        }
        public override string ToString()
        {
            return question_text;
        }

        public bool Equals(Question other)
        {
            return (other != null) && id == other.id;
        }

        /// <summary>
        /// Добавляет новый вопрос
        /// </summary>
        /// <param name="text">Текст вопроса</param>
        /// <param name="path">Адрес, например, GameContext/Question/</param>
        /// <returns></returns>
        public static async Task<int> PostQuestion(string text, string path)
        {
            int result = -1;
            Question new_question = new(0, text);
            WebRequest request = WebRequest.Create(App.HOST_URL + path);
            request.Method = "POST";
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
            string result_stringJSON = JsonSerializer.Serialize<Question>(new_question, options);
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

    [Serializable]
    public class AnswerList
    {
        public List<Answer> answers { get; set; }
        public AnswerList() { answers = new List<Answer>(); }
        public AnswerList(List<Answer> answers)
        {
            this.answers = answers;
        }
        public void Add(Answer answer)
        {
            answers.Add(answer);
        }
        public static async Task<List<Answer>> GetAnswersListAsync(string path)
        {
            List<Answer> answers = new();
            WebRequest request = WebRequest.Create(App.HOST_URL + path);
            WebResponse response = await request.GetResponseAsync();
            string answJSON = string.Empty;
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new(stream))
                answJSON = reader.ReadToEnd();
            answers = JsonSerializer.Deserialize<AnswerList>(answJSON).answers;
            response.Close();
            return answers;
        }
    }
    [Serializable]
    public class Answer : IEquatable<Answer>
    {
        public int id { get; set; }
        public string answer_text { get; set; }
        public Answer() { }
        public Answer(int id, string answer_text)
        {
            this.id = id;
            this.answer_text = answer_text;
        }
        public override string ToString()
        {
            return answer_text;
        }

        public bool Equals(Answer other)
        {
            return (other != null) && id == other.id;
        }

        /// <summary>
        /// Добавляет новый ответ
        /// </summary>
        /// <param name="text">Текст ответ</param>
        /// <param name="path">Адрес, например, GameContext/Answer/</param>
        /// <returns></returns>
        public static async Task<int> PostAnswer(string text, string path)
        {
            int result = -1;
            Answer new_answer = new(0, text);
            WebRequest request = WebRequest.Create(App.HOST_URL + path);
            request.Method = "POST";
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
            string result_stringJSON = JsonSerializer.Serialize<Answer>(new_answer, options);
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

    [Serializable]
    public class QuestionOnAnswer
    {
        public int question_id { get; set; }
        public string question_text { get; set; }
        public int answer_id { get; set; }
        public string answer_text { get; set; }
        public QuestionOnAnswer() { }
        public QuestionOnAnswer(int question_id, string question_text, int answer_id, string answer_text)
        {
            this.question_id = question_id;
            this.question_text = question_text;
            this.answer_id = answer_id;
            this.answer_text = answer_text;
        }
    }
    [Serializable]
    public class QuestionOnAnswerList
    {
        public List<QuestionOnAnswer> questionsOnAnswer { get; set; }
        public QuestionOnAnswerList() { questionsOnAnswer = new List<QuestionOnAnswer>(); }
        public QuestionOnAnswerList(List<QuestionOnAnswer> questionsOnAnswer)
        {
            this.questionsOnAnswer = questionsOnAnswer;
        }
        public void Add(QuestionOnAnswer questionOnAnswer)
        {
            questionsOnAnswer.Add(questionOnAnswer);
        }

        public static async Task<List<QuestionOnAnswer>> GetAswersAndQuestionsForDiagnosis(string path, int diagnosis_id)
        {
            List<QuestionOnAnswer> AswersAndQuestions = new();
            WebRequest request = WebRequest.Create(App.HOST_URL + path + diagnosis_id);
            WebResponse response = await request.GetResponseAsync();
            string AswersAndQuestionsJSON = string.Empty;
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new(stream))
                AswersAndQuestionsJSON = reader.ReadToEnd();
            AswersAndQuestions = JsonSerializer.Deserialize<QuestionOnAnswerList>(AswersAndQuestionsJSON).questionsOnAnswer;
            response.Close();
            return AswersAndQuestions;
        }

        public static async void UpdateAnswerOnQuestionForDiagnosis(string path, int diagnosis_id, int question_id, int new_question_id, int new_answer_id)
        {
            WebRequest request = WebRequest.Create(App.HOST_URL + path
                                                    + diagnosis_id + '-'
                                                    + question_id + '-'
                                                    + new_question_id + '-'
                                                    + new_answer_id);
            request.Method = "PUT";
            await request.GetResponseAsync();
        }
        public static async void DeleteAnswerOnQuestionForDiagnosis(string path, int diagnosis_id, int question_id)
        {
            WebRequest request = WebRequest.Create(App.HOST_URL + path
                                                    + diagnosis_id + '-'
                                                    + question_id);
            request.Method = "DELETE";
            await request.GetResponseAsync();
        }
        public static async Task<int> PostAnswerOnQuestionForDiagnosis(string path, int diagnosis_id, int answer_id, int question_id)
        {
            int result = -1;
            answers_questions_for_diagnosesString new_answer = new()
            {
                diagnosis_id = diagnosis_id,
                answer_id = answer_id,
                question_id = question_id
            };
            WebRequest request = WebRequest.Create(App.HOST_URL + path);
            request.Method = "POST";
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
            string result_stringJSON = JsonSerializer.Serialize<answers_questions_for_diagnosesString>(new_answer, options);
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

        /// <summary>
        /// Проверка существования пары диагноз-вопрос
        /// </summary>
        /// <param name="path">Адрес</param>
        /// <returns>Возвращает true, если пара диагноз-вопрос есть, false, если ещё нет</returns>
        public static async Task<bool> DiagnosisQuestionValidation(string path, int diagnosis_id, int question_id)
        {
            int res = 1;
            WebRequest request = WebRequest.Create(App.HOST_URL + path + diagnosis_id + "-" + question_id);
            WebResponse response = await request.GetResponseAsync();
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new(stream))
                int.TryParse(reader.ReadToEnd(), out res);
            response.Close();
            return res > 0;
        }
    }
    [Serializable]
    public class answers_questions_for_diagnosesString
    {
        public int diagnosis_id { get; set; }
        public int question_id { get; set; }
        public int answer_id { get; set; }
    }
}
