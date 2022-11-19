using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Quizlet
{
    class PairGame
    {
        private readonly int ModuleId;
        private SQLiteConnection db;
        private List<string> WordNames;
        private List<string> WordMeanings;
        private int WordsCount;
        public PairGame(int ModuleId, int WordsCount)
        {
            this.ModuleId = ModuleId;
            this.WordsCount = WordsCount;
            db = new SQLiteConnection("Data Source = data.db;");
            db.Open();
        }

        public (List<string>, List<string>) StartGame()
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
            return (WordNames.Take(WordsCount).ToList(), WordMeanings.Take(WordsCount).ToList());
            //return (WordNames.OrderBy(x=>x.Take(rnd.Next(WordsCount)).ToList(), WordMeanings.OrderBy(x => x.Take(rnd.Next(WordsCount)).ToList());
        }

        public static Label FindNearLabel(Canvas canvas, Label l)
        {
            foreach (Label el in canvas.Children)
            {
                if (GetResult(el, l) & !el.Equals(l))
                {
                    return el;
                }
            }

            return null;
        }

        static bool GetResult(Label l1, Label l2)
        {
            if (Math.Abs(Canvas.GetLeft(l1) - Canvas.GetLeft(l2)) < 50 &&
                Math.Abs(Canvas.GetTop(l1) - Canvas.GetTop(l2)) < 50)
                return true;
            return false;

        }

    }

}
