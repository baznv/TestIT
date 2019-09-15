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

        private ObservableCollection<Node> openedFiles = new ObservableCollection<Node>();
        public ObservableCollection<Node> OpenedFiles
        {
            get { return openedFiles; }
            set
            {
                openedFiles = value;
                OnPropertyChanged();
            }
        }

        ObservableCollection<FileExtensionM> Extentions { get; set; }

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
                var temp = next.Folder;
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

            var folders = ClassDB.GetFolders(parentId);
            foreach (var item in folders)
            {
                Node node = new Node() { Folder = item };
                temp.Add(node);
            }

            var files = ClassDB.GetFiles(parentId);
            foreach (var item in files)
            {
                Node node = new Node() { File = item, Extension = Extentions.FirstOrDefault(x => x.ID == item.FileExtentionID) };
                temp.Add(node);
            }
            return temp;
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
                if (selectedItem?.File != null)
                {
                    var file = OpenedFiles.FirstOrDefault(x => x.File.ID == selectedItem.File.ID);
                    if (file == null)
                        OpenedFiles.Add(selectedItem);
                }
                OnPropertyChanged();
            }
        }

        private int selectedIndexOpenedFiles;
        public int SelectedIndexOpenedFiles
        {
            get
            {
                return selectedIndexOpenedFiles;
            }
            set
            {
                selectedIndexOpenedFiles = value;
                OnPropertyChanged();
            }
        }

        private Node newNode;
        public Node NewNode
        {
            get
            {
                return newNode;
            }
            set
            {
                newNode = value;
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

            if (SelectedItem.File != null)
                SelectedIndexOpenedFiles = OpenedFiles.IndexOf(SelectedItem);
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
            NewNode = new Node();
            NewNode.Folder = new FolderM();

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
                        p => IsCanSaveFolder() )
                    );
            }
        }

        private bool IsCanSaveFolder()
        {
            if (!String.IsNullOrEmpty(NewNode?.Folder?.Name))
                return true;
            else return false;
        }

        private void SaveFolder(object p)
        {
            (p as Window).DialogResult = true;
            Node node = SelectedItem;

            if (IsSaveToRoot || node == null)
            {
                NewNode.Folder.ParentFolderID = 0;
                Nodes.Add(NewNode);
            }
            else
            {
                NewNode.Folder.ParentFolderID = node.Folder.ID;
                Node tmp = FindParentNode(Nodes, NewNode.Folder.ParentFolderID);
                tmp.Nodes.Add(NewNode);
            }
            ClassDB.SaveToDB(NewNode.Folder);
            NewNode = null;
        }

        public Node FindParentNode(ObservableCollection<Node> nodes, int parentFolderID)
        {
            foreach (Node node in nodes)
            {
                Node findNode = node.Folder?.ID == parentFolderID ? node : FindParentNode(node.Nodes, parentFolderID);
                if (findNode != null)
                {
                    return findNode;
                }
            }
            return null;
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
                        p => true )
                    );
            }
        }

        private void AddFile()
        {
            NewNode = new Node();
            NewNode.File = new FileM();
            NewNode.Extension = new FileExtensionM();

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
                NewNode.Extension.Icon = Convert.ToBase64String(fileBytes);
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
                    NewNode.Extension = new FileExtensionM() { TypeFile = extension };
                else
                    NewNode.Extension = fem;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFilePath);

                NewNode.File.Name = fileNameWithoutExtension;
                NewNode.File.Content = File.ReadAllText(fullFilePath, Encoding.Unicode);
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
                NewNode.File.FolderID = 0;
                Nodes.Add(NewNode);
            }
            else
            {
                NewNode.File.FolderID = node.Folder.ID;
                Node tmp = FindParentNode(Nodes, NewNode.File.FolderID);
                tmp.Nodes.Add(NewNode);
            }
            if (NewNode.Extension.ID == 0)
                ClassDB.SaveToDB(NewNode.Extension);
            NewNode.File.FileExtentionID = NewNode.Extension.ID;
            ClassDB.SaveToDB(NewNode.File);
            NewNode = null;
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
            return SelectedItem?.Folder != null;
        }

        private void DeleteFolder()
        {
            List<Node> lst = new List<Node>();
            lst.Add(SelectedItem);

            var stack = new Stack<Node>(SelectedItem.Nodes);

            while (stack.Any())
            {
                var next = stack.Pop();
                if (next != null)
                {
                    lst.Add(next);
                    foreach (var item in next?.Nodes)
                        stack.Push(item);
                }
            }

            lst.Reverse();
            foreach (var item in lst)
            {
                if (item.Folder != null)
                    ClassDB.DeleteObject(item.Folder);
                else if (item.File != null)
                    ClassDB.DeleteObject(item.File);
            }

            Node node = FindParentNode(Nodes, SelectedItem.Folder.ParentFolderID);
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
            return SelectedItem?.File != null;
        }

        private void DeleteFile()
        {
            //var t = SelectedItem;
            ClassDB.DeleteObject(SelectedItem.File);
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
            sfd.FileName = SelectedItem.File.Name;
            sfd.DefaultExt = SelectedItem.Extension.TypeFile;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(sfd.OpenFile(), System.Text.Encoding.Default))
                {
                    sw.Write(SelectedItem.File.Content);
                    sw.Close();
                }
            }
        }

        private ICommand renameCommand;
        public ICommand RenameCommand
        {
            get
            {
                return renameCommand ??
                    (
                    renameCommand = new Commands.RelayCommand(
                        p => Rename(),
                        p => true)
                    );
            }
        }

        private void Rename()
        {
            RenameWindow nw = new RenameWindow();
            nw.DataContext = this;
            nw.ShowDialog();
        }

        private ICommand saveRenameCommand;
        public ICommand SaveRenameCommand
        {
            get
            {
                return saveRenameCommand ??
                    (
                    saveRenameCommand = new Commands.RelayCommand(
                        p => SaveRename(p),
                        p => true)
                    );
            }
        }

        private void SaveRename(object p)
        {
            (p as Window).DialogResult = true;
            if (SelectedItem.Folder != null)
                ClassDB.UpdateObject(SelectedItem.Folder);
            else if (SelectedItem.File != null)
                ClassDB.UpdateObject(SelectedItem.File);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    public class Node : INotifyPropertyChanged
    {
        //public IStorage Storage { get; set; }
        public FolderM Folder { get; set; }
        public FileM File { get; set; }

        private FileExtensionM extension;
        public FileExtensionM Extension
        {
            get { return extension; }
            set
            {
                extension = value;
                OnPropertyChanged();
            }
        }

        //public bool IsSelected { get; set; }
        public ObservableCollection<Node> Nodes { get; set; } = new ObservableCollection<Node>();


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

    }

}
