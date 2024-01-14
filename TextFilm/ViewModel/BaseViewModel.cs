using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TextFilm.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public Action Close { get; set; }

        public void Message(string message)
        {
            MessageBox.Show(message);
        }

        #region Event
        public virtual event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnProperty(params string[] propertyNames)
        {
            if (PropertyChanged != null)
            {
                foreach (string propertyName in propertyNames) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                PropertyChanged(this, new PropertyChangedEventArgs("HasError"));
            }
        }
        #endregion
    }
}
