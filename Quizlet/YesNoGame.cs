using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;

namespace Quizlet
{
    class YesNoGame
    {

        public string QuestionField { get; set; }
        public string AnswerField { get; set; }
        private readonly int ModuleId;
        public double Accuracy { get; set; }
        private int WordsAmount = 0;
        private double AccuracyStep;
        private SQLiteConnection db;
        private List<string> ansList;
        private List<string> questList;
        public YesNoGame(int moduleId)
        {
            ModuleId = moduleId;
            db = new SQLiteConnection("Data Source = data.db;");
            db.Open();
            questList = new List<string>();
            ansList = new List<string>();

            SQLiteDataReader reader;
            SQLiteCommand command = new SQLiteCommand($"select Word, Meaning from Words where Module_id='{ModuleId}'", db);
            reader = command.ExecuteReader();

            foreach (DbDataRecord el in reader)
            {
                questList.Add(el["Word"].ToString());
                ansList.Add(el["Meaning"].ToString());
            }

        }

        public (string, string) StartGame()
        {
            Random rnd = new();
            QuestionField = questList[rnd.Next(0, questList.Count)];
            AnswerField = ansList[rnd.Next(0, questList.Count)];
            WordsAmount = ansList.Count;
            AccuracyStep = (double)1 / (WordsAmount * 4);

            return (QuestionField, AnswerField);
        }

        public bool GetResult()
        {
            SQLiteDataReader reader;
            SQLiteCommand command2 = new SQLiteCommand($"SELECT Meaning from Words where Word = '{QuestionField}'", db);
            reader = command2.ExecuteReader();
            string word = "";
            foreach (DbDataRecord el in reader)
            {
                word = el["Meaning"].ToString();
            }
            return AnswerField.Equals(word);
        }

        public void ChangeAccuracy(bool plus = true)
        {
            if (plus)
            {
                Accuracy += AccuracyStep;
                return;

            }
            if (Accuracy < 0 || Accuracy-AccuracyStep<0 )
            {
                return;
            }
            
            Accuracy -= AccuracyStep;

        }
    }
}
