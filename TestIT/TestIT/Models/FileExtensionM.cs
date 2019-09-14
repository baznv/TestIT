using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TestIT.DB;

namespace TestIT.Models
{
    [Table]
    public class FileExtensionM : INotifyPropertyChanged
    {
        private int id;
        [PrimaryKey]
        public int ID
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        private string typeFile;
        [Unique]
        public string TypeFile
        {
            get { return typeFile; }
            set
            {
                typeFile = value;
                OnPropertyChanged();
            }
        }

        private string icon;
        public string Icon
        {
            get { return icon; }
            set
            {
                icon = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

    }
}
