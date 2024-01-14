using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;
namespace TextFilm.Model
{
    public class FileWorker
    {
        private List<string> ignoreWord = new List<string>()
        {
            "Fuck", "Fucked", "Fucking", "Whore", "Slut", "Idiot", "Shit",
            "Nigger", "Bitch", "Bastard", "Prick", "Dick", "Cunt", "Pussy", "Sucker", "Suck",
            "Sucking", "Asshole", "Gay", "Faggot", "Douchebag", "Ass", "Motherfucker", "Pissed",
            "Piss", "Dickhead", "Fucko", "Scumbag", "Wanker", "Retard", "Numbnuts", "Prat",
            "Moron", "Skullfuck", "Felch", "Beaver", "Punani"
        };

        private List<string> repcaseWordWithDot = new List<string>()
        {
            "a.m.", "a. m.","p.m.","p. m.","Mr.","Mrs.","Ms.","Dr.","St.","vs.",
        };

        public FileWorker()
        {
        }

        public ObservableCollection<UserFile> GetUserFiles()
        {
            ObservableCollection<UserFile> temp = new ObservableCollection<UserFile>();
            string[] allfiles = Directory.GetFiles($"{Directory.GetCurrentDirectory()}/Files");
            allfiles.ToList().ForEach(x =>
            {
                temp.Add(new UserFile()
                {
                    Name = GetShortName(x),
                    Content = new ObservableCollection<string>(File.ReadAllLines(x).ToList())
                });
            });
            return temp;
        }
        private string GetShortName(string path)
        {
            string output = "";
            for (int i = path.Length - 1; i >= 0; i--)
                if (path[i] != '\\')
                    output = path[i] + output;
                else break;
            return output.Replace(".txt", "");
        }
        public UserFile AddFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Txt file (*.txt)|*.txt";
            if(openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;
                string temp = File.ReadAllText(path);
                List<string> row = FilterRow(temp);
                List<string> content = ConnectSepparateString(row);
                for(int i = 0; i < content.Count; i++)
                {
                    content[i] = content[i].Replace("#", ".");
                }

                string name = GetShortName(path);

                File.WriteAllLines($"{Directory.GetCurrentDirectory()}/Files/{name}.txt", content);

                UserFile userFile = new UserFile()
                {
                    Name = name,
                    Content = new ObservableCollection<string>(content)
                };

                return userFile;
            }

            return null;
        }
        private List<string> FilterRow(string rows)
        {
            List<string> row = new List<string>();
            List<string> finalRow = new List<string>();

            Regex regex = new Regex(@".{0,1}\d{1,5}.{0,2}.\d\d:\d\d:\d\d,\d\d\d --> \d\d:\d\d:\d\d,\d\d\d.", RegexOptions.Singleline);
            Regex dot = new Regex(@"(\p{Lu}\.){2,10}");
            Regex number = new Regex(@"(\d){1,10}\.\d{1,10}");
            MatchCollection matches = regex.Matches(rows);         

            foreach(Match m in matches)
            {
                rows = ReplaceDot(rows.Replace(m.Value.ToString(), ""));
            }

            MatchCollection matchesDot = dot.Matches(rows);

            foreach (Match m in matchesDot)
            {
                rows = rows.Replace(m.Value.ToString(), m.Value.ToString().Replace(".", "#"));
            }

            MatchCollection matchesNumber = number.Matches(rows);

            foreach (Match m in matchesNumber)
            {
                rows = rows.Replace(m.Value.ToString(), m.Value.ToString().Replace(".", "#"));
            }

            rows = rows.Replace("\r\r", "\n");

            string[] temp2 = rows.Split('\n');

            temp2 = temp2.ToList().Where(x => x != "\r" && x != "").ToArray();

            return temp2.ToList();
        }
        private List<string> ConnectSepparateString(List<string> row)
        {
            row = ReplaceSympol(row).Where(x => x != "").ToList();

            for (int i = 0; i < row.Count(); i++) 
            {
                int index = row[i].IndexOfAny(new char[3] { '.', '!', '?' });

                if (i != row.Count - 1)
                {
                    if (Char.IsLower(row[i + 1][0]) || index == -1)
                    {
                        row[i] = row[i] + " " + row[i + 1];
                        row[i] = row[i].Replace("\n", "").Replace("\r", "").Replace("\r\n", "").Replace("\n\r", "");
                        row.RemoveAt(i + 1);

                        i--;
                    }
                }
            }

            List<string> temp = new List<string>();


            for (int i = 0; i < row.Count(); i++)
            {
                if (row[i].Contains("...") && i != row.Count - 1)
                {
                    row[i] = row[i].Replace("...", "") + " " + row[i + 1];
                    row.Remove(row[i + 1]);
                    i--;
                }
            }


            for (int i = 0; i < row.Count(); i++)
            {
                GetNewStrings(row[i].Replace("\n", "").Replace("\r", "").Replace("\r\n", "").Replace("\n\r", "")).ForEach(x => temp.Add(x));
            }

            return ReplaceSympol(temp.Where(x => x != ".").ToList());
        }
        private List<string> GetNewStrings(string old)
        {
            Debug.WriteLine(old);
            int index = old.IndexOfAny(new char[3] { '.', '!', '?' });

            if (index == old.Length - 1 || index == old.Length - 2 || index == -1)
            {
                return new List<string>() { old.Replace("\"", "").Replace("''", "") };
            }
            else
            {
                List<string> output = new List<string>
                {
                    old.Substring(0, index + 1).Replace("\"", "").Replace("''", "")
                };

                GetNewStrings(old.Substring(index + 1, (old.Length - 1 - index)).Replace("\"", "").Replace("''", "")).ForEach(x => output.Add(x));

                return output;
            }
        }
        private List<string> ReplaceSympol(List<string> row)
        {
            for (int i = 0; i < row.Count; i++)
            {
                for (int j = 0; j < row[i].Length; j++)
                {
                    if (row[i][j] == ' ' || row[i][j] == '"' || row[i][j] == '-' || row[i][j] == '\'' || row[i][j] == '[' || row[i][j] == ']' || row[i][j] == '{' || row[i][j] == '}' || row[i][j] == '.' || row[i][j] == '\n' || row[i][j] == '\r' || row[i][j] == '\t')
                    {
                        row[i] = row[i].Substring(j + 1);
                        j--;
                    }
                    else break;
                }
            }

            return row; 
        }

        public void Print(List<UserFile> row, int count, int start, int end, string word, bool searchAccuracy)
        {
            List<string> output = new List<string>();

            count = row.Count < count ? row.Count : count;
            Random r = new Random();
            row = row.OrderBy(x => r.Next()).ToList();

           

            if (!searchAccuracy)
                for (int i = 0; i < row.Count; i++) 
                {
                    List<string> temp = row[i].Content.Where(x => x.Split(' ').Count() >= start && x.Split(' ').Count() <= end).ToList().Where(x => IsContainIgnoreWord(x) == false).ToList();

                    for (int j = 0; j < temp.Count; j++)
                    {
                        if (!temp[j].ToLower().Contains(word.ToLower()))
                        {
                            temp.RemoveAt(j);
                            j--;
                        }
                    }
                    temp = temp.OrderBy(x => r.Next()).ToList();


                    if (output.Count == count)
                        break;

                    if (temp.Count != 0)
                        for(int j =0; j < temp.Count; j++)
                            if(output.Where(x => x.Contains(temp[j])).ToList().Count() == 0)
                            {
                                output.Add(temp[j] + $" – {row[i].Name}\n");
                                break;
                            }

                }
            else
                for (int i = 0; i < row.Count; i++)
                {
                    List<string> temp = row[i].Content.Where(x => x.Split(' ').Count() >= start && x.Split(' ').Count() <= end).ToList().Where(x => IsContainIgnoreWord(x) == false).ToList();

                    for (int j = 0; j < temp.Count; j++)
                    {
                        string[] words = temp[j].ToLower().Replace(",", "").Replace(".", "").Replace(":", "").Replace("!", "").Replace("?", "").Split(' ');

                        if (!words.Contains(word.ToLower()))
                        {
                            temp.RemoveAt(j);
                            j--;
                        }
                    }

                    temp = temp.OrderBy(x => r.Next()).ToList();


                    if (output.Count == count)
                        break;

                    if (temp.Count != 0)
                        for (int j = 0; j < temp.Count; j++)
                            if (output.Where(x => x.Contains(temp[j])).ToList().Count() == 0)
                            {
                                output.Add(temp[j] + $" – {row[i].Name}\n");
                                break;
                            }
                            
                }

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Txt file (*.txt)|*.txt";

            if(save.ShowDialog() == true)
            {
                string path = save.FileName;

                File.WriteAllLines(path, output);
            }
        }

        private bool IsContainIgnoreWord(string s)
        {

            for(int i = 0; i < ignoreWord.Count();  i++) 
            {
                if (s.ToLower().Contains(ignoreWord[i].ToLower()))
                    return true;
            }

            return false;
        }

        public string ReplaceDot(string s)
        {
            for(int i=0; i< repcaseWordWithDot.Count(); i++)
            {
                if (s.Contains(repcaseWordWithDot[i]))
                {
                    s = s.Replace(repcaseWordWithDot[i], repcaseWordWithDot[i].Replace(".", "#"));
                }
            }

            return s;
        }
    }
}
