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
    public class FileM : INotifyPropertyChanged, IStorage
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

        private string description;
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnPropertyChanged();
            }
        }

        private int fileExtentionID;
        [ForeignKey(typeof(FileExtensionM))]
        public int FileExtentionID
        {
            get { return fileExtentionID; }
            set
            {
                fileExtentionID = value;
                OnPropertyChanged();
            }
        }

        private int folderID;
        [ForeignKey(typeof(FolderM))]
        public int FolderID
        {
            get { return folderID; }
            set
            {
                folderID = value;
                OnPropertyChanged();
            }
        }

        private string content;
        public string Content
        {
            get { return content; }
            set
            {
                content = value;
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
