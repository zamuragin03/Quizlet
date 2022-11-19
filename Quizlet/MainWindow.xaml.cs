using System;
using System.Collections;
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
using System.Diagnostics.SymbolStore;
using System.Threading;
using System.Windows.Media.Animation;
using NLog;


namespace Quizlet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        delegate void InfoHandler(string text);

        readonly SQLiteConnection db;
        SQLiteCommand command;

        private Label currentLabel;
        enum State
        {
            Stated,
            Unstated,
        }
        private State state;

        private List<Word> WordList;
        private int CurrentWord;

        private (List<string>, List<string>) currendWords;

        
        private readonly InfoHandler messageBoxHandler;
        private YesNoGame yesnogame;
        private SpellingGame spellinggame;
        private PairGame pairgame;
        public MainWindow()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "file.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;

            messageBoxHandler = PrintInfo;
            InitializeComponent();

            db = new SQLiteConnection("Data Source = data.db;");
            db.Open();

            state = State.Unstated;
            Update_Modules_List();
        }
        void PrintInfo(string message)
        {

            MessageBox.Show(message, "Quizlet", MessageBoxButton.OK, MessageBoxImage.Information);
            Log.Warn(message);
        }
        private void AddModule_Button(object sender, RoutedEventArgs e)
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

            if (String.IsNullOrEmpty(WordMeaningText.Trim()) || string.IsNullOrEmpty(WordnNameText.Trim()) || string.IsNullOrEmpty(ModuleName))
            {
                messageBoxHandler.Invoke("Не вводите пустые");
                return;
            }
            SQLiteDataReader reader;
            SQLiteCommand command2 = new($"SELECT Module_id from Modules where Module_Name = '{ModuleName}'", db);
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
            List<Word> words = new();
            foreach (DbDataRecord item in reader)
            {
                Word temp = new()
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
                Word temp = new()
                {
                    Module_Name = item["Module_Name"].ToString(),
                    Word_Meaning = null,
                    Word_Text = null,
                };
                ModuleNamesColumn.Items.Add(temp.Module_Name);
                Module_List.Items.Add(temp);


            }
            command.Dispose();
        }
        void LearnWordTabItem()
        {
            SQLiteDataReader reader;
            command = new SQLiteCommand("select Word, Meaning from Words", db);
            WordList = new List<Word>();
            reader = command.ExecuteReader();
            foreach (DbDataRecord item in reader)
            {
                WordList.Add(new Word()
                {
                    Word_Meaning = item["Word"].ToString(),
                    Word_Text = item["Meaning"].ToString(),
                    Module_Name = null
                });
            }
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
            try
            {
                var pair = yesnogame.StartGame();
                YesNoQuestionField.Content = pair.Item1;
                YesNoAnswerField.Content = pair.Item2;
            }
            catch (Exception e)
            {
                Log.Error(e.Message, "Не добавлены слова");
                messageBoxHandler.Invoke("Не добавлены слова");
            }

        }
        private void LearnModule(object sender, RoutedEventArgs e)
        {
            Button el = sender as Button;

            if (state == State.Unstated)
            {
                LearnItem.IsSelected = true;
                YesButton.IsEnabled = true;
                NoButton.IsEnabled = true;
                ChangeSelectedButtonBackground(el);

            }
        }
        private void AddWord(object sender, RoutedEventArgs e)
        {
            Button el = sender as Button;

            if (state == State.Unstated)
            {
                AddWords.IsSelected = true;
                ChangeSelectedButtonBackground(el);

                AddWordTabItem();
            }

        }
        private void AddModule(object sender, RoutedEventArgs e)
        {

            Button el = sender as Button;

            if (state == State.Unstated)
            {
                Add_Module.IsSelected = true;
                ChangeSelectedButtonBackground(el);
                Update_Modules_List();

            }

        }
        private void MyWord(object sender, RoutedEventArgs e)
        {
            Button el = sender as Button;

            if (state == State.Unstated)
            {
                MyWords.IsSelected = true;
                ChangeSelectedButtonBackground(el);

                MyWordsTabItem();

            }
        }
        private void SpellingModule(object sender, RoutedEventArgs e)
        {
            Button el = sender as Button;

            if (state == State.Unstated)
            {
                Spelling.IsSelected = true;
                ChangeSelectedButtonBackground(el);

            }
        }
        private void LearnWord(object sender, RoutedEventArgs e)
        {
            Button el = sender as Button;

            if (state == State.Unstated)
            {
                LearWord.IsSelected = true;
                ChangeSelectedButtonBackground(el);

                LearnWordTabItem();


            }
        }
        private void PairModule(object sender, RoutedEventArgs e)
        {
            Button el = sender as Button;

            if (state == State.Unstated)
            {
                Pair.IsSelected = true;
                ChangeSelectedButtonBackground(el);
            }
        }
        private void AnswerButtonSpelling(object sender, RoutedEventArgs e)
        {
            string answer = AnswerBox.Text;
            int result = spellinggame.CheckAnswer(answer, isModeReverse.IsChecked.Value);
            var answers = spellinggame.GetResult();
            if (result <= 2)
                Spelling_();
            else
            {
                messageBoxHandler.Invoke("Ваш ответ " + answers.Item1 + " \n" + "Правильный ответ " + answers.Item2);
            }



        }
        private void StartSpellingModule(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(ModuleNamesColumn.Text.Trim()))
            {
                messageBoxHandler.Invoke("Выберите модуль");
                return;
            }

            
            SpellingStartButton.IsEnabled = true;
            Spelling_();
            state = State.Stated;
            Title = $"Изучение модуля — {ModuleNamesColumn.Text}";
        }
        void Spelling_()
        {
            spellinggame = new SpellingGame(GetCurrentModuleId());
            SpellingLabel.Content = spellinggame.StartGame(isModeReverse.IsChecked.Value);

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
            int CurrentModule_Id = 0;
            string ModuleName = ModuleNamesColumn.Text;
            SQLiteDataReader reader;
            SQLiteCommand command2 = new($"SELECT Module_id from Modules where Module_Name = '{ModuleName}'", db);
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
            if ((result && s == "Yes") || (!result && s == "No"))
            {
                yesnogame.ChangeAccuracy();
            }
            else
            {
                yesnogame.ChangeAccuracy(false);
            }

            YesNoGameResult.Content = "Прогресс: " + $"{yesnogame.Accuracy:F}%";
            (string, string) pair = yesnogame.StartGame();
            YesNoQuestionField.Content = pair.Item1;
            YesNoAnswerField.Content = pair.Item2;

            if (yesnogame.Accuracy > 1)
            {
                messageBoxHandler.Invoke("Модуль завершен");
            }

            if (yesnogame.Accuracy > 1)
            {
                state = State.Unstated;
                messageBoxHandler.Invoke("Изучение модуля завершено");
            }
        }
        private void PreviousWord(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentWord--;
                LearnWord_Word_Label.Content = WordList[CurrentWord].Word_Text;
                LeanWord_Meaning_Label.Content = WordList[CurrentWord].Word_Meaning;
            }
            catch (Exception exception)
            {
                messageBoxHandler.Invoke("Некуда назад двигаться");
                CurrentWord++;
            }
        }
        private void NextWord(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentWord++;
                LearnWord_Word_Label.Content = WordList[CurrentWord].Word_Text;
                LeanWord_Meaning_Label.Content = WordList[CurrentWord].Word_Meaning;
            }
            catch (Exception exception)
            {
                messageBoxHandler.Invoke("Некуда вперед двигаться");
                CurrentWord--;
            }

        }
        void ChangeSelectedButtonBackground(Button button, bool flag = true)
        {

            foreach (UIElement btn in Buttons.Children)
            {
                if (btn is Button btn_)
                {
                    btn_.Foreground = new SolidColorBrush(Colors.Black);
                    btn_.Background = new SolidColorBrush(Colors.DeepSkyBlue);

                }
            }
            if (flag)
            {
                button.Foreground = new SolidColorBrush(Colors.White);
                button.Background = new SolidColorBrush(Colors.DarkSlateBlue);
            }

        }
        private void StopSpellingModule(object sender, RoutedEventArgs e)
        {
            state = State.Unstated;
            SpellingStartButton.IsEnabled = false;
        }

        void PairGame()
        {
            pairgame = new PairGame(GetCurrentModuleId(), (int)WordsCounter.Value);
            currendWords = pairgame.StartGame();
            GridCanvas.Children.Clear();
            Random rnd = new();
            for (int i = 1; i <= currendWords.Item1.Count; i++)
            {
                Label lbl = new()
                {
                    Content = currendWords.Item1[i - 1]
                };
                lbl.MouseMove += Handler;
                Canvas.SetLeft(lbl, rnd.Next(700));
                Canvas.SetTop(lbl, rnd.Next(500));
                GridCanvas.Children.Add(lbl);
            }
            for (int i = 1; i <= currendWords.Item2.Count; i++)
            {
                Label lbl = new()
                {
                    Content = currendWords.Item2[i - 1]
                };
                lbl.MouseMove += Handler;
                Canvas.SetLeft(lbl, rnd.Next(800));
                Canvas.SetTop(lbl, rnd.Next(500));
                GridCanvas.Children.Add(lbl);
            }
        }


        private void Handler(object sender, MouseEventArgs e)
        {
            Label el = sender as Label;
            currentLabel = el;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(el, el, DragDropEffects.Move);
            }
        }

        private void GridButtons_OnDrop(object sender, DragEventArgs e) { }

        private void GridCanvas_OnDragOver(object sender, DragEventArgs e)
        {
            Point p = e.GetPosition(GridCanvas);
            Canvas.SetLeft(currentLabel, p.X);
            Canvas.SetTop(currentLabel, p.Y);
        }

        private void PairGameCheckResult(object sender, RoutedEventArgs e)
        {
            var WordNames = currendWords.Item1;
            var WordMeanings = currendWords.Item2;
            List<Label> listtoremove = new();

            foreach (Label element in GridCanvas.Children)
            {
                Label l = Quizlet.PairGame.FindNearLabel(GridCanvas, element);
                if (l is null)
                {
                    //messageBoxHandler.Invoke(element.Content.ToString() + "без пары");
                    element.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    for (int i = 0; i < WordNames.Count; i++)
                    {
                        if (WordNames[i].Equals(l.Content) && WordMeanings[i].Equals(element.Content))
                        {
                            //s += l.Content.ToString() + " " + element.Content + " Верно" + "\n";
                            listtoremove.Add(element);
                            listtoremove.Add(l);
                        }
                        else
                        {
                            element.Foreground = new SolidColorBrush(Colors.Red);
                            l.Foreground = new SolidColorBrush(Colors.Red);

                        }
                    }

                    
                }
            }
            foreach (var label in listtoremove)
            {
                GridCanvas.Children.Remove(label);
            }

            if (listtoremove.Distinct().Count()==WordMeanings.Count)
            {
                messageBoxHandler.Invoke($"Модуль {ModuleNamesColumn.Text} Завершен");
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Pair.IsSelected )
            {
                PairGame();
            }
        }
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
