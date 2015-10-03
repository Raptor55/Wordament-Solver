using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Wordament_Solver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WordList = readWordList(@"dictionary.txt");
        }

        string[,] letters;
        bool[,] board;
        List<string> words = new List<string>();
        string[] WordList;

        IEnumerable<string> lengthSort(IEnumerable<string> e)
        {
            var sorted = from s in e
                         orderby s.Length descending
                         select s;
            return sorted;
        }

        bool checkSquare(string input)
        {
            List<string> inputBoard = new List<string>(input.Length);

            int startIndex = 0;
            int count = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]))
                {
                    if (i == 0)
                    {
                        startIndex = i;
                        count++;
                        inputBoard.Add("" + char.ToLower(input[i]));
                    }

                    else
                    {
                        if (char.IsUpper(input[i - 1]))
                        {
                            inputBoard[startIndex - count + 1] += char.ToLower(input[i]);
                        }

                        else
                        {
                            startIndex = i;
                            count++;
                            inputBoard.Add("" + char.ToLower(input[i]));
                        }
                    }
                }

                else
                {
                    inputBoard.Add("" + input[i]);
                }
            }

            if (inputBoard.Count != (Math.Sqrt(inputBoard.Count) * Math.Sqrt(inputBoard.Count)))
            {
                MessageBox.Show("Number of tiles is not a perfect square");
                return false;
            }

            else
            {
                int sqrtLength = (int)Math.Sqrt(inputBoard.Count);
                letters = new string[sqrtLength, sqrtLength];

                for (int i = 0; i < sqrtLength; i++)
                {
                    for (int j = 0; j < sqrtLength; j++)
                    {
                        letters[i, j] = "" + inputBoard[i * sqrtLength + j];
                    }
                }

                board = new bool[letters.GetLength(0), letters.GetLength(0)];
                resetBoard();
                return true;
            }
        }

        string getSquare()
        {
            if (letters == null)
            {
                return "no letters";
            }

            else
            {
                string retString = "";

                for (int i = 0; i < letters.GetLength(0); i++)
                {
                    for (int j = 0; j < letters.GetLength(1); j++)
                    {
                        if ((j+1) == letters.GetLength(1))
                        {
                            retString += letters[i, j];
                        }
                        else
                        {
                            retString += letters[i, j] + "\t";
                        }
                    }

                    if((i+1) != letters.GetLength(0))
                    {
                        retString += "\n\n";
                    }
                }
                return retString;
            }
        }

        void resetBoard()
        {
            for (int i = 0; i < letters.GetLength(0); i++)
            {
                for (int j = 0; j < letters.GetLength(1); j++)
                {
                    board[i, j] = false;
                }
            }
        }

        void getWords()
        {
            for (int xStart = 0; xStart < letters.GetLength(0); xStart++)
            {
                for (int yStart = 0; yStart < letters.GetLength(0); yStart++)
                {
                    for (int xEnd = 0; xEnd < letters.GetLength(0); xEnd++)
                    {
                        for (int yEnd = 0; yEnd < letters.GetLength(0); yEnd++)
                        {
                            paths(xStart, yStart, xEnd, yEnd, "");
                            resetBoard();
                        }
                    }
                }
            }
        }

        void paths(int xStart, int yStart, int xEnd, int yEnd, string str)
        {
            if (xStart >= 0 && yStart >= 0 && xStart < (letters.GetLength(0)) && yStart < (letters.GetLength(0)))
            {
                if (!board[xStart, yStart])
                {
                    board[xStart, yStart] = true;
                    str += letters[xStart, yStart];
                    WordPrefix prefix = new WordPrefix(WordList);
                    prefix.add(str);

                    if (xStart == xEnd && yStart == yEnd && prefix.isWord())
                    {
                        words.Add(str);
                    }

                    else if (prefix.isPrefix())
                    {
                        paths(xStart + 1, yStart, xEnd, yEnd, str);
                        paths(xStart, yStart + 1, xEnd, yEnd, str);
                        paths(xStart + 1, yStart + 1, xEnd, yEnd, str);
                        paths(xStart, yStart - 1, xEnd, yEnd, str);
                        paths(xStart + 1, yStart - 1, xEnd, yEnd, str);
                        paths(xStart - 1, yStart, xEnd, yEnd, str);
                        paths(xStart - 1, yStart + 1, xEnd, yEnd, str);
                        paths(xStart - 1, yStart - 1, xEnd, yEnd, str);
                    }

                    board[xStart, yStart] = false;
                }
            }
        }

        string[] readWordList(string filename)
        {
            List<String> list = new List<String>();

            using (StreamReader file = new StreamReader(filename))
            {
                while (!file.EndOfStream)
                {
                    list.Add(file.ReadLine());
                }
            }

            return list.ToArray();
        }

        private void solve_Click(object sender, RoutedEventArgs e)
        {
            words.Clear();
            results.Clear();
            if (checkSquare(letterBox.Text))
            {
                boardText.Text = getSquare();
                getWords();
                words = words.Distinct().ToList();
                words.Sort();
                words = lengthSort(words).ToList();

                int minLength = 0;

                if (!int.TryParse(lengthBox.Text, out minLength))
                {
                    MessageBox.Show("Invalid Length Input");
                }

                else
                {
                    for (int i = 0; i < words.Count; i++)
                    {
                        if (words[i].Length >= minLength)
                        {
                            if ((i + 1) != words.Count)
                            {
                                results.Text += words[i] + "\n";
                            }

                            else 
                            { 
                                results.Text += words[i];
                            }
                        }
                    }
                }
            }
        }
    }

    class WordPrefix
    {
        private string prefix;
        private string[] WordList;

        public WordPrefix(string[] WordList)
        {
            this.WordList = WordList;
            prefix = "";
        }

        public WordPrefix(WordPrefix src)
        {
            this.WordList = src.WordList;
            prefix = "";
        }

        public bool isWord()
        {
            int min = 0, max = WordList.Length - 1;
            for (int i = 0; i < (Math.Log(WordList.Length) / Math.Log(2)); i++)
            {
                if (prefix.CompareTo(WordList[(min + max) / 2]) > 0)
                {
                    min = (min + max) / 2;
                }

                else if (prefix.CompareTo(WordList[(min + max) / 2]) < 0)
                {
                    max = (min + max) / 2;
                }

                else if (prefix.CompareTo(WordList[(min + max) / 2]) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool isPrefix()
        {
            int min = 0, max = WordList.Length - 1;
            for (int i = 0; i < (Math.Log(WordList.Length) / Math.Log(2)); i++)
            {
                if (WordList[(min + max) / 2].Length > prefix.Length)
                {
                    if (prefix.CompareTo(WordList[(min + max) / 2].Substring(0, prefix.Length)) > 0)
                    {
                        min = (min + max) / 2;
                    }

                    else if (prefix.CompareTo(WordList[(min + max) / 2].Substring(0, prefix.Length)) < 0)
                    {
                        max = (min + max) / 2;
                    }

                    else if (prefix.CompareTo(WordList[(min + max) / 2].Substring(0, prefix.Length)) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void add(string str)
        {
            prefix += str;
        }

        public int length()
        {
            return prefix.Length;
        }

        public string getText()
        {
            return prefix;
        }
    }
}