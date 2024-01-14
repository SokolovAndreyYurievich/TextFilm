using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextFilm.Model
{
    public class UserFile
    {
        public string Name { get; set; }
        public ObservableCollection<string> Content { get; set; }
    }
}
