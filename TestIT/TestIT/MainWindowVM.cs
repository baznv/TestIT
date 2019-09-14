using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using TestIT.DB;
using TestIT.Interfaces;
using TestIT.Models;

namespace TestIT
{
    class MainWindowVM : INotifyPropertyChanged
    {
        private ObservableCollection<Node> nodes;
        public ObservableCollection<Node> Nodes
        {
            get { return nodes; }
            set
            {
                nodes = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<FileExtensionM> extentions;
        public ObservableCollection<FileExtensionM> Extentions
        {
            get { return extentions; }
            set
            {
                extentions = value;
                OnPropertyChanged();
            }
        }

        public void Init()
        {
            Extentions = ClassDB.GetExtentions();
            Nodes = GetLevel(0);
            Load();
        }

        async public void Load()
        {
            var stack = new Stack<Node>(Nodes);

            while (stack.Any())
            {
                var next = stack.Pop();
                var temp = next.Storage as FolderM;
                if (temp != null)
                {
                    next.Nodes = GetLevel(temp.ID);
                    foreach (var item in next.Nodes)
                        stack.Push(item);
                }
            }
        }

        private ObservableCollection<Node> GetLevel(int parentId)
        {
            ObservableCollection<Node> temp = new ObservableCollection<Node>();

            var foldersAndFiles = ClassDB.GetIStorages(parentId);
            foreach (var item in foldersAndFiles)
            {
                Node node;
                if (item is FolderM)
                    node = new Node() { IsSelected = false, Storage = (item as FolderM) };
                else if (item is FileM)
                    node = new Node() { IsSelected = false, Storage = (item as FileM), Extension=Extentions.FirstOrDefault(x => x.ID == (item as FileM).FileExtentionID) };
                else node = new Node();
                temp.Add(node);
            }
            return temp;
        }

        private FolderM newFolder = new FolderM();
        public FolderM NewFolder
        {
            get { return newFolder; }
            set
            {
                newFolder = value;
                OnPropertyChanged();
            }
        }

        private FileM newFile = new FileM();
        public FileM NewFile
        {
            get { return newFile; }
            set
            {
                newFile = value;
                OnPropertyChanged();
            }
        }

        private FileExtensionM newFileExtension = new FileExtensionM();
        public FileExtensionM NewFileExtension
        {
            get { return newFileExtension; }
            set
            {
                newFileExtension = value;
                OnPropertyChanged();
            }
        }

        private Node selectedItem;
        public Node SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                selectedItem = value;
                OnPropertyChanged();
            }
        }

        private bool isSaveToRoot;
        public bool IsSaveToRoot
        {
            get
            {
                return isSaveToRoot;
            }
            set
            {
                isSaveToRoot = value;
                OnPropertyChanged();
            }
        }

        private ICommand treeViewCommand;
        public ICommand TreeViewCommand
        {
            get
            {
                return treeViewCommand ??
                    (
                    treeViewCommand = new Commands.RelayCommand(
                        p => TreeView(p),
                        p => true)
                    );
            }
        }

        private void TreeView(object p)
        {
            SelectedItem = p as Node;
        }

        private ICommand createFolderCommand;
        public ICommand CreateFolderCommand
        {
            get
            {
                return createFolderCommand ??
                    (
                    createFolderCommand = new Commands.RelayCommand(
                        p => CreateFolder(),
                        p => true )
                    );
            }
        }

        private void CreateFolder()
        {
            AddFolderWindow nw = new AddFolderWindow();
            nw.DataContext = this;
            nw.ShowDialog();
        }

        private ICommand saveFolderCommand;
        public ICommand SaveFolderCommand
        {
            get
            {
                return saveFolderCommand ??
                    (
                    saveFolderCommand = new Commands.RelayCommand(
                        p => SaveFolder(p),
                        p => true )
                    );
            }
        }

        private void SaveFolder(object p)
        {
            (p as Window).DialogResult = true;
            Node node = SelectedItem;

            if (IsSaveToRoot || node == null)
            {
                NewFolder.ParentFolderID = 0;
                Nodes.Add(new Node() { Storage = NewFolder });
            }
            else
            {
                NewFolder.ParentFolderID = (node.Storage as FolderM).ID;
                Node tmp = FindParentNode(Nodes, NewFolder);
                tmp.Nodes.Add(new Node() { Storage = NewFolder });
            }
            ClassDB.SaveToDB(NewFolder);
            NewFolder = null;
        }

        public Node FindParentNode(ObservableCollection<Node> nodes, IStorage storage)
        {
            Node findNode = null;
            foreach (Node node in nodes)
            {
                if (storage is FolderM)
                    findNode = (node.Storage as FolderM)?.ID == (storage as FolderM).ParentFolderID ? node : FindParentNode(node.Nodes, storage);
                else if (storage is FileM)
                    findNode = (node.Storage as FolderM)?.ID == (storage as FileM).FolderID ? node : FindParentNode(node.Nodes, storage);
            }
            return findNode;
        }


        private ICommand addFileCommand;
        public ICommand AddFileCommand
        {
            get
            {
                return addFileCommand ??
                    (
                    addFileCommand = new Commands.RelayCommand(
                        p => AddFile(),
                        p => true
                       )
                    );
            }
        }

        private void AddFile()
        {
            AddFileWindow afw = new AddFileWindow();
            afw.DataContext = this;
            afw.ShowDialog();
        }

        private ICommand loadIconCommand;
        public ICommand LoadIconCommand
        {
            get
            {
                return loadIconCommand ??
                    (
                    loadIconCommand = new Commands.RelayCommand(
                        p => LoadIcon(),
                        p => true )
                    );
            }
        }

        private void LoadIcon()
        {
            string fullFilePath;
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                fullFilePath = ofd.FileName;
                byte[] fileBytes = File.ReadAllBytes(fullFilePath);
                NewFileExtension.Icon = Convert.ToBase64String(fileBytes);
            }
        }

        private ICommand loadFileCommand;
        public ICommand LoadFileCommand
        {
            get
            {
                return loadFileCommand ??
                    (
                    loadFileCommand = new Commands.RelayCommand(
                        p => LoadFile(),
                        p => true )
                    );
            }
        }

        private void LoadFile()
        {
            string fullFilePath;
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                fullFilePath = ofd.FileName;
                string extension = Path.GetExtension(fullFilePath);
                FileExtensionM fem = Extentions.FirstOrDefault(x => x.TypeFile == extension);
                if (fem == null)
                    NewFileExtension = new FileExtensionM() { TypeFile = extension };
                else
                    NewFileExtension = fem;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFilePath);

                NewFile.Name = fileNameWithoutExtension;
                NewFile.Content = File.ReadAllText(fullFilePath, Encoding.Default);
            }
        }

        private ICommand saveFileCommand;
        public ICommand SaveFileCommand
        {
            get
            {
                return saveFileCommand ??
                    (
                    saveFileCommand = new Commands.RelayCommand(
                        p => SaveFile(p),
                        p => true)
                    );
            }
        }

        private void SaveFile(object p)
        {
            (p as Window).DialogResult = true;

            Node node = SelectedItem;

            if (IsSaveToRoot || node == null)
            {
                NewFile.FolderID = 0;
                Nodes.Add(new Node() { Storage = NewFile });
            }
            else
            {
                NewFile.FolderID = (node.Storage as FolderM).ID;
                Node tmp = FindParentNode(Nodes, NewFile as FileM);
                tmp.Nodes.Add(new Node() { Storage = NewFile });
            }
            if (NewFileExtension.ID == 0)
                ClassDB.SaveToDB(NewFileExtension);
            NewFile.FileExtentionID = NewFileExtension.ID;
            ClassDB.SaveToDB(NewFile);
            NewFile = null;
            NewFileExtension = null;
        }


        private ICommand deleteFolderCommand;
        public ICommand DeleteFolderCommand
        {
            get
            {
                return deleteFolderCommand ??
                    (
                    deleteFolderCommand = new Commands.RelayCommand(
                        p => DeleteFolder(),
                        p => IsSelectedItemFolder() )
                    );
            }
        }

        private bool IsSelectedItemFolder()
        {
            return SelectedItem?.Storage is FolderM;
        }

        private void DeleteFolder()
        {
            List<Node> lst = new List<Node>();


            ClassDB.DeleteObject(SelectedItem.Storage as FolderM);

            Node node = FindParentNode(Nodes, SelectedItem.Storage as FolderM);
            if (node == null)
                Nodes.Remove(SelectedItem);
            else
                node.Nodes.Remove(SelectedItem);
            SelectedItem = null;
        }

        private ICommand deleteFileCommand;
        public ICommand DeleteFileCommand
        {
            get
            {
                return deleteFileCommand ??
                    (
                    deleteFileCommand = new Commands.RelayCommand(
                        p => DeleteFile(),
                        p => IsSelectedItemFile())
                    );
            }
        }

        private bool IsSelectedItemFile()
        {
            return SelectedItem?.Storage is FileM;
        }

        private void DeleteFile()
        {
            //var t = SelectedItem;
            ClassDB.DeleteObject(SelectedItem.Storage as FileM);
            Nodes.Remove(SelectedItem);
            SelectedItem = null;
        }

        
        private ICommand downloadFileCommand;
        public ICommand DownloadFileCommand
        {
            get
            {
                return downloadFileCommand ??
                    (
                    downloadFileCommand = new Commands.RelayCommand(
                        p => DownloadFile(),
                        p => IsSelectedItemFile())
                    );
            }
        }

        private void DownloadFile()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = SelectedItem.Storage.Name;
            sfd.DefaultExt = SelectedItem.Extension.TypeFile;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(sfd.OpenFile(), System.Text.Encoding.Default))
                {
                    sw.Write((SelectedItem.Storage as FileM).Content);
                    sw.Close();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    public class Node
    {
        public IStorage Storage { get; set; }
        public FileExtensionM Extension { get; set; }
        public bool IsSelected { get; set; }
        public ObservableCollection<Node> Nodes { get; set; } = new ObservableCollection<Node>();
    }

}
