using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using TestIT.DB;
using TestIT.Models;

namespace TestIT
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public ObservableCollection<FolderM> Folders { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ClassDB.Init();

            MainWindowVM mwvm = new MainWindowVM();
            mwvm.Init();
            DataContext = mwvm;
            //treeView1.ItemsSource = Folders;
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void MaximizeClick(object sender, RoutedEventArgs e)
        {
            ChangeSize();
        }

        private void ContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ChangeSize();
        }

        private void ChangeSize()
        {
            App.Current.MainWindow.WindowState = App.Current.MainWindow.WindowState == WindowState.Maximized ?
                WindowState.Normal : WindowState.Maximized;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            App.Current.MainWindow.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        //private void CreateFolder_Click(object sender, RoutedEventArgs e)
        //{
        //    NameWindow nw = new NameWindow();
        //    nw.ShowDialog();
        //}

    }


    //public class Node
    //{
    //    public string Name { get; set; }
    //    public ObservableCollection<Node> Nodes { get; set; }
    //    //public ObservableCollection<FileFolder> Files { get; set; }
    //}

    //public class FileFolder
    //{
    //    public string Name { get; set; }
    //}

}
