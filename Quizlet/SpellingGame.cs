using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;

namespace Quizlet
{
    class SpellingGame
    {
        private string QuestionField;
        private readonly int ModuleId;
        private SQLiteConnection db;
        private List<string> wordList;

        private string Answer;
        private string RightAnswer;
        public SpellingGame(int ModuleId)
        {
            wordList = new List<string>();
            this.ModuleId = ModuleId;
            db = new SQLiteConnection("Data Source = data.db;");
            db.Open();
        }

        public string StartGame(bool flag = false)
        {
            SQLiteDataReader reader;
            SQLiteCommand command;
            Random rnd = new();
            if (!flag)
            {
                command = new SQLiteCommand($"select Word from Words where Module_id='{ModuleId}'", db);
                reader = command.ExecuteReader();
                foreach (DbDataRecord el in reader)
                {
                    wordList.Add(el["Word"].ToString());
                }

                QuestionField = wordList[rnd.Next(wordList.Count)];
                return QuestionField;
            }

            command = new SQLiteCommand($"select Meaning from Words where Module_id='{ModuleId}'", db);
            reader = command.ExecuteReader();

            foreach (DbDataRecord el in reader)
            {
                wordList.Add(el["Meaning"].ToString());
            }

            QuestionField = wordList[rnd.Next(wordList.Count)];
            return QuestionField;
        }


        public int CheckAnswer(string answer, bool flag = false)
        {
            Answer = answer;
            var result = "";
            SQLiteDataReader reader;
            SQLiteCommand command;
            if (!flag)
            {

                command = new SQLiteCommand($"select Meaning from Words where Word='{QuestionField}'", db);
                reader = command.ExecuteReader();
                foreach (DbDataRecord el in reader)
                    result = el["Meaning"].ToString();

                RightAnswer = result;
                return Levinshtein.LevenshteinDistance(answer.Trim().ToLower(), result.Trim().ToLower());

            }
            command = new SQLiteCommand($"select Word from Words where Meaning='{QuestionField}'", db);
            reader = command.ExecuteReader();
            foreach (DbDataRecord el in reader)
                result = el["Word"].ToString();
            RightAnswer = result;

            return Levinshtein.LevenshteinDistance(answer?.ToLower(), result?.ToLower());
        }

        public (string,string) GetResult()
        {
            return (Answer, RightAnswer);
        }
    }
}
