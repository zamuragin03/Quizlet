using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Quizlet
{
    class PairGame
    {
        private string QuestionField;
        private readonly int ModuleId;
        private SQLiteConnection db;
        private List<string> WordNames;
        private List<string> WordMeanings;

        public PairGame(int ModuleId)
        {
            this.ModuleId = ModuleId;
            db = new SQLiteConnection("Data Source = data.db;");
            db.Open();
        }

        public (List<string>,List<string>) StartGame()
        {
            WordMeanings = new();
            WordNames = new();
            SQLiteDataReader reader;
            SQLiteCommand command;
            command = new SQLiteCommand($"select Word,Meaning from Words where Module_id='{ModuleId}'", db);
            reader = command.ExecuteReader();
            foreach (DbDataRecord el in reader)
            {
                WordNames.Add(el["Word"].ToString());
                WordMeanings.Add(el["Meaning"].ToString());
            }

            return (WordNames, WordMeanings);
        }

    }

}
