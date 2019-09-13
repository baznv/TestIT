using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public void Init()
        {
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

            var foldersAndFiles = ClassDB.GetData(parentId);
            foreach (var item in foldersAndFiles)
            {
                Node node;
                if (item is FolderM)
                    node = new Node() { IsSelected = false, Storage = (item as FolderM) };
                else if (item is FileM)
                    node = new Node() { IsSelected = false, Storage = (item as FileM) };
                else node = new Node();
                temp.Add(node);
            }

            return temp;
        }

        private string nameNewFolder;
        public string NameNewFolder
        {
            get { return nameNewFolder; }
            set
            {
                nameNewFolder = value;
                OnPropertyChanged();
            }
        }

        private Node selectedNode;
        public Node SelectedNode
        {
            get { return selectedNode; }
            set
            {
                selectedNode = value;
                OnPropertyChanged();
            }
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
                        p => true
                       )
                    );
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

        //public bool IsSelectedItemFolder
        //{
        //    get
        //    {
        //        return (Nodes.FirstOrDefault(x => x.IsSelected == true)).Storage is FolderM;
        //    }

        //                set
        //    {
        //        selectedNode = value;
        //        OnPropertyChanged();
        //    }

        //}

        private ICommand deleteFolderCommand;
        public ICommand DeleteFolderCommand
        {
            get
            {
                return deleteFolderCommand ??
                    (
                    deleteFolderCommand = new Commands.RelayCommand(
                        p => DeleteFolder(),
                        p => CanDeleteFolder()
                       )
                    );
            }
        }

        private bool CanDeleteFolder()
        {
            if (SelectedItem?.Storage is FolderM)
                return true;
            else
                return false;
        }

        private void DeleteFolder()
        {
            //FolderM tmp = SelectedItem as FolderM;
            //Node node = Nodes.FirstOrDefault(x => x.Storage.Equals(SelectedItem));
            var t = SelectedItem;
            ClassDB.DeleteObject(SelectedItem.Storage as FolderM);
            Nodes.Remove(SelectedItem);
            SelectedItem = null;
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
                        p => true
                       )
                    );
            }
        }

        private void TreeView(object p)
        {
            SelectedItem = p as Node;
        }

        private ICommand saveNameCommand;
        public ICommand SaveNameCommand
        {
            get
            {
                return saveNameCommand ??
                    (
                    saveNameCommand = new Commands.RelayCommand(
                        p => SaveName(p),
                        p => true
                       )
                    );
            }
        }

        private void SaveName(object p)
        {
            (p as Window).DialogResult = true;
            Node node = SelectedItem;
            FolderM folder = new FolderM() { Name = NameNewFolder };

            if (node == null)
            {
                folder.ParentFolderID = 0;
                Nodes.Add(new Node() { IsSelected = false, Storage = (IStorage)folder });
            }
            else
            {
                folder.ParentFolderID = (node.Storage as FolderM).ID;
                Node tmp = FindNode(Nodes, folder);
                tmp.Nodes.Add(new Node() { IsSelected = false, Storage = (IStorage)folder });
            }

            NameNewFolder = null;
            ClassDB.SaveToDB(folder);
        }

        public Node FindNode(ObservableCollection<Node> nodes, IStorage storage)
        {
            foreach (Node node in nodes)
            {
                Node highScore;
                if (storage is FolderM)
                    highScore = (node.Storage as FolderM).ID == (storage as FolderM).ParentFolderID ? node : FindNode(node.Nodes, storage);
                //else if (storage is FileM)
                //    return (node.Storage as FileM).ID == (node.Storage as FileM).ParentFolderID ? node : FindNode(node.Nodes);
                else
                    highScore = null;

                if (highScore != null)
                {
                    return highScore;
                }
            }
            return null;
        }

        private void CreateFolder()
        {
            AddFolderWindow nw = new AddFolderWindow();
            nw.DataContext = this;
            nw.ShowDialog();
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
        public bool IsSelected { get; set; }
        public ObservableCollection<Node> Nodes { get; set; } = new ObservableCollection<Node>();
    }

}
