﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Data.SQLite;


namespace Quizlet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        delegate void InfoHandler(string text);
        private string data;
        SQLiteConnection db;
        SQLiteCommand command;

        enum State
        {
            Stated,
            Unstated,
        }

        private State state;
        private InfoHandler messageBoxHandler;
        private YesNoGame yesnogame;
        public MainWindow()
        {
            messageBoxHandler = PrintInfo;
            InitializeComponent();

            db = new SQLiteConnection("Data Source = data.db;");
            db.Open();

            state = State.Unstated;
            SQLiteDataReader reader;
            command = new SQLiteCommand("select Module_Name from Modules", db);
            reader = command.ExecuteReader();
            ModuleNamesColumn.Items.Clear();
            foreach (DbDataRecord item in reader)
            {
                ModuleNamesColumn.Items.Add(item["Module_Name"]);
            }
        }

        void PrintInfo(string message) =>
            MessageBox.Show(message, "Quizlet", MessageBoxButton.OK, MessageBoxImage.Information);

        void Logging(string message)
        {
            DateTime todayDateTime = DateTime.Today.ToLocalTime();
            //TODO дохуя всего
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string moduleNameText = ModuleName.Text;
            if (string.IsNullOrEmpty(moduleNameText.Trim()))
            {
                messageBoxHandler.Invoke("Не вводите пустые");
                return;
            }
            command = new SQLiteCommand("insert into Modules values (@Module_id,@Module_Name)", db);
            command.Parameters.AddWithValue("@Module_Name", moduleNameText.TrimStart().TrimEnd());
            command.Parameters.AddWithValue("@Module_id", null);
            command.ExecuteNonQuery();
            command.Dispose();
            ModuleName.Text = "";
            Update_Modules_List();
        }

        private void Add_New_Word(object sender, RoutedEventArgs e)
        {
            string WordnNameText = WordName.Text;
            string WordMeaningText = WordMeaning.Text;
            string ModuleName = ModuleNames.Text;

            if (String.IsNullOrEmpty( WordMeaningText.Trim()) ||string.IsNullOrEmpty(WordnNameText.Trim()))
            {
                messageBoxHandler.Invoke("Не вводите пустые");
                return;
            }
            SQLiteDataReader reader;
            SQLiteCommand command2 = new SQLiteCommand($"SELECT Module_id from Modules where Module_Name = '{ModuleName}'", db);
            reader = command2.ExecuteReader();
            string ModuleNameId = "";
            foreach (DbDataRecord el in reader)
            {
                ModuleNameId = el["Module_id"].ToString();
            }

            command = new SQLiteCommand("insert into Words values (@Word,@Meaning, @Module_id)", db);
            command.Parameters.AddWithValue("@Module_id", ModuleNameId);
            command.Parameters.AddWithValue("@Word", WordnNameText.TrimStart().TrimEnd());
            command.Parameters.AddWithValue("@Meaning", WordMeaningText.TrimStart().TrimEnd());

            command.ExecuteNonQuery();
            command.Dispose();
            command2.Dispose();
            WordName.Text = "";
            WordMeaning.Text = "";
        }
        private void AddWordTabItem()
        {
            SQLiteDataReader reader;
            command = new SQLiteCommand("select Module_Name from Modules", db);
            reader = command.ExecuteReader();
            ModuleNames.Items.Clear();
            foreach (DbDataRecord item in reader)
            {
                ModuleNames.Items.Add(item["Module_Name"]);
            }
        }

        private void MyWordsTabItem()
        {
            SQLiteDataReader reader;
            command = new SQLiteCommand("select Word, Meaning, Module_Name from Words inner join Modules on Words.Module_id = Modules.Module_id", db);
            reader = command.ExecuteReader();
            Word_List.ItemsSource = null;
            List<Word> words = new List<Word>();
            foreach (DbDataRecord item in reader)
            {
                Word temp = new Word()
                {
                    Module_Name = item["Module_Name"].ToString(),
                    Word_Meaning = item["Meaning"].ToString(),
                    Word_Text = item["Word"].ToString(),
                };
                words.Add(temp);

            }

            Word_List.ItemsSource = words.OrderBy(x => x.Module_Name);
            command.Dispose();
        }

        void Update_Modules_List()
        {
            SQLiteDataReader reader;
            command = new SQLiteCommand("select Module_Name from Modules", db);
            reader = command.ExecuteReader();
            Module_List.Items.Clear();
            ModuleNamesColumn.Items.Clear();
            foreach (DbDataRecord item in reader)
            {
                Word temp = new Word()
                {
                    Module_Name = item["Module_Name"].ToString(),
                    Word_Meaning = null,
                    Word_Text = null,
                };
                Module_List.Items.Add(temp);
                ModuleNamesColumn.Items.Add(temp);
            }
            command.Dispose();
        }


        private void OnDelete(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить все данные?", "Quizlet", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    command = new SQLiteCommand("delete from Words", db);
                    command.ExecuteNonQuery();
                    command.Dispose();
                    command = new SQLiteCommand("delete from Modules", db);
                    command.ExecuteNonQuery();
                    command.Dispose();
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        private void LearnTabIem()
        {
            yesnogame = new YesNoGame(GetCurrentModuleId());
            var pair = yesnogame.StartGame();
            YesNoQuestionField.Content = pair.Item1;
            YesNoAnswerField.Content = pair.Item2;
        }


        private void LearnModule(object sender, RoutedEventArgs e)
        {
            if (state == State.Unstated)
            {
                LearnItem.IsSelected = true;

            }
        }

        private void AddWord(object sender, RoutedEventArgs e)
        {
            if (state == State.Unstated)
            {
                AddWords.IsSelected = true;
                AddWordTabItem();
            }
            
        }

        private void AddModule(object sender, RoutedEventArgs e)
        {

            if (state == State.Unstated)
            {
                Add_Module.IsSelected = true;
                Update_Modules_List();
            }
            
        }

        private void MyWord(object sender, RoutedEventArgs e)
        {
            if (state == State.Unstated)
            {
                MyWords.IsSelected = true;
                MyWordsTabItem();
            }
        }

        private void SpellingModule(object sender, RoutedEventArgs e)
        {
            if (state== State.Stated)
            {
                Spelling.IsSelected = true;

            }
        }

        private void PairModule(object sender, RoutedEventArgs e)
        {
        }


        private void SpellingModuleButton(object sender, RoutedEventArgs e)
        {
        }

        private void StartSpellingModule(object sender, RoutedEventArgs e)
        {
        }

        private void StartLearnModuleButton(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(ModuleNamesColumn.Text.Trim()))
            {
                messageBoxHandler.Invoke("Выберите модуль");
                return;
            }
            state = State.Stated;
            Title = $"Изучение модуля — {ModuleNamesColumn.Text}";
            LearnTabIem();
        }

        private int GetCurrentModuleId()
        {
            int CurrentModule_Id=0; 
            string ModuleName = ModuleNamesColumn.Text;
            SQLiteDataReader reader;
            SQLiteCommand command2 = new SQLiteCommand($"SELECT Module_id from Modules where Module_Name = '{ModuleName}'", db);
            reader = command2.ExecuteReader();
            foreach (DbDataRecord el in reader)
            {
                CurrentModule_Id = int.Parse(el["Module_id"].ToString());
            }

            return CurrentModule_Id;
        }

        private void AnswerYesNo(object sender, RoutedEventArgs e)
        {

            bool result = yesnogame.GetResult();
            string s = (sender as Button).Content.ToString();
            if ((result && s=="Yes" )|| (!result && s == "No"))
            {
                yesnogame.ChangeAccuracy();
                messageBoxHandler.Invoke($"правильно. Точность {yesnogame.Accuracy}");
            }
            else
            {
                yesnogame.ChangeAccuracy(false);
                messageBoxHandler.Invoke($"Неправильно. Точность {yesnogame.Accuracy}");
            }
            (string,string) pair = yesnogame.StartGame();
            YesNoQuestionField.Content = pair.Item1;
            YesNoAnswerField.Content = pair.Item2;
        }

    }

    class YesNoGame
    {
        
        public string QuestionField { get; set; }
        public string AnswerField { get; set; }
        private readonly int ModuleId;
        public double Accuracy { get; set; }
        private int WordsAmount = 0;
        private double AccuracyStep;
        private SQLiteConnection db;
        public YesNoGame(int moduleId)
        {
            ModuleId = moduleId;
            db = new SQLiteConnection("Data Source = data.db;");
            db.Open();
        }

        public (string,string) StartGame()
        {
            List<string> questList = new List<string>();
            List<string> ansList = new List<string>();

            Random rnd = new();
            SQLiteDataReader reader;
            SQLiteCommand command = new SQLiteCommand($"select Word, Meaning from Words where Module_id='{ModuleId}'", db);
            reader = command.ExecuteReader();

            foreach (DbDataRecord el in reader)
            {
                questList.Add(el["Word"].ToString());
                ansList.Add(el["Meaning"].ToString());
            }

            QuestionField = questList[rnd.Next(0, questList.Count)];
            AnswerField = ansList[rnd.Next(0, questList.Count)];
            WordsAmount = ansList.Count;
            AccuracyStep = (double) 1 / WordsAmount;
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

        public void ChangeAccuracy(bool plus=true)
        {
            if (Accuracy<0)
            {
                return;
            }
            if (plus)
            {
                Accuracy += AccuracyStep;
                return;
            }
            Accuracy -= AccuracyStep;

        }
    }

    class SpellingModule
    {
        
    }
    class Word
    {
        public string Word_Meaning { get; set; }
        public string Module_Name { get; set; }
        public string Word_Text { get; set; }

        public override string ToString()
        {
            return Module_Name + " " + Word_Text + " " + Word_Meaning;

        }
    }
}
