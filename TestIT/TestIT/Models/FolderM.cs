using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TestIT.DB;
using TestIT.Interfaces;

namespace TestIT.Models
{
    [Table]
    public class FolderM : INotifyPropertyChanged, IStorage
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

        private string name;
        [NotNull]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private int parentFolderID;
        [NotNull]
        public int ParentFolderID
        {
            get { return parentFolderID; }
            set
            {
                parentFolderID = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public override string ToString()
        {
            return String.Format($"{ID} {Name} {ParentFolderID}");
        }
    }
}
