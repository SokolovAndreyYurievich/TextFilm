using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using TextFilm.Model;

namespace TextFilm.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private ObservableCollection<UserFile> _userFile;
        private UserFile _selectUserFile;
        private FileWorker _file;
        private string _selectSentence;
        private string _findWord;
        private int _numberOffers;
        private int _startWord;
        private int _endWord;
        private bool _searchAccuracy;

        public ObservableCollection<UserFile> UserFile
        {
            get => _userFile;
            set 
            {
                _userFile = value;
                OnProperty("UserFile");
            }
        }
        public string SelectSentence
        {
            get => _selectSentence;
            set
            {
                _selectSentence = value;
                OnProperty("SelectSentence");
            }
        }
        public UserFile SelectUserFile
        {
            get => _selectUserFile;
            set
            {
                _selectUserFile = value;
                OnProperty("SelectUserFile");
            }
        }
        public string FindWord
        {
            get => _findWord;
            set
            {
                _findWord = value;
                OnProperty("FindWord");
            }
        }
        public int NumberOffers
        {
            get => _numberOffers;
            set
            {
                _numberOffers = value;
                OnProperty("NumberOffers");
            }
        }
        public int StartWord
        {
            get => _startWord;
            set
            {
                _startWord = value;
                OnProperty("StartWord");
            }
        }
        public int EndWord
        { 
            get => _endWord;
            set
            {
                _endWord = value;
                OnProperty("EndWord");
            }
        }
        public bool SearchAccuracy
        {
            get => _searchAccuracy;
            set
            {
                _searchAccuracy = value;
                OnProperty("SearchAccuracy");
            }
        }

        public MainViewModel() 
        {
            _file = new FileWorker();
            UserFile = _file.GetUserFiles();
            if (UserFile.Count > 0)
                SelectUserFile = UserFile[0];

            NumberOffers = 50;
            StartWord = 2;
            EndWord = 8;
            SearchAccuracy = true;

        }

        private void AddFile()
        {
            UserFile user = _file.AddFile();

            if(user != null)
                UserFile.Add(user);

            OnProperty("UserFile");
        }

        private void PrintString()
        {
            if(EndWord < StartWord)
            {
                Message("Некорректный диапозон слов в предложении");
                return;
            }

            if(FindWord == "")
            {
                Message("Слово для поиска, не должно быть пустой строкой");
                return;
            }

            _file.Print(_userFile.ToList(), NumberOffers, StartWord, EndWord, FindWord, SearchAccuracy);
        }

        public void Copy()
        {
            Clipboard.SetText(SelectSentence);
        }

        public void Update()
        {

        }

        public void Remove()
        {
            File.Delete($"{Directory.GetCurrentDirectory()}/Files/{SelectUserFile.Name}.txt");
            UserFile = _file.GetUserFiles();
            OnProperty("UserFile");
        }

        public RelayCommand Add => new RelayCommand(AddFile);
        public RelayCommand Prind => new RelayCommand(PrintString);
    }
}
