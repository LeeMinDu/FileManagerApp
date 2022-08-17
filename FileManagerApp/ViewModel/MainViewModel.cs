using FileManagerApp.Component;
using FileManagerApp.Shared;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileManagerApp.ViewModel
{
    // xu li cho  view co ten la MainView.xaml
    public class MainViewModel : BaseViewModel
    {
        private IList<DirectoryInfo> _directories;

        public IList<DirectoryInfo> Directories
        {
            get => _directories;
            set
            {
                _directories = value;
                OnPropertyChanged(nameof(Directories));
            }
        }

        public IList<DirectoryInfo> CurrentStack { get; set; } = new List<DirectoryInfo>();
        public IList<DirectoryInfo> SecondaryStack { get; set; } = new List<DirectoryInfo>();

        private string _currentPath;
        public string CurrentPath 
        {
            get { return _currentPath; }
            set
            {
                _currentPath = value;
                OnPropertyChanged(nameof(CurrentPath));
            }
        }

        private int _itemCount;
        public int ItemCount
        {
            get => _itemCount;
            set
            {
                _itemCount = value;
                OnPropertyChanged(nameof(ItemCount));
            }
        }

        public IList<DirectoryTree> DirectoryTrees { get; set; } = new List<DirectoryTree>();

        public IList<MenuItem> DirectoryHistory { get; set; } = new List<MenuItem>();

        private void OnTreeViewLoaded()
        {
            var disks = new Dictionary<string, string>
            {
                {
                    @"C:\",
                    "/Asset/icons8-c-drive-16.png"
                },
                {
                    @"D:\",
                    "/Asset/icons8-disk-partition-48.png"
                },
                {
                    @"E:\",
                    "/Asset/icons8-disk-partition-48.png"
                },
            };
            foreach (var disk in disks)
            {
                DirectoryTrees.Add(new DirectoryTree()
                {
                    Title = $"New Volume ({disk.Key.Replace(@"\", "")})",
                    ImageSource = disk.Value,
                    Directories = new DirectoryInfo(disk.Key).GetDirectories(),
                });
            }
        }

        public MainViewModel()
        {
            // Init
            var di = new DirectoryInfo(@"E:\");
            Directories = di.GetDirectories();
            CurrentPath = di.FullName;
            ItemCount = Directories.Count;
            CurrentStack.Add(di);
            OnTreeViewLoaded();
            DirectoryHistory.Add(new MenuItem() { Header = "v" });
            // Events
            OnBackClicked = new CommandHandler((_) => 
            {
                // Add current path to [SecondaryStack]
                SecondaryStack.Insert(0, CurrentStack[0]);
                // Remove current path from [CurrentStack]
                CurrentStack.RemoveAt(0);
                // Update current path to [Directiories]
                var directoryInfo = CurrentStack[0];
                OnDirectoriesChanged(directoryInfo);
            }, () => CurrentStack.Count > 1);

            OnForwardClicked = new CommandHandler((_) =>
            {
                CurrentStack.Insert(0, SecondaryStack[0]);
                SecondaryStack.RemoveAt(0);
                var directoryInfo = CurrentStack[0];
                OnDirectoriesChanged(directoryInfo);
            }, () => SecondaryStack.Count > 0);

            OnUpClicked = new CommandHandler((_) =>
            {
                var directoryInfo = new DirectoryInfo(CurrentPath);
                OnDirectoriesChanged(directoryInfo.Parent);
            }, () => new DirectoryInfo(CurrentPath).Parent != null);

            OnItemClicked = new CommandHandler((sender) =>
            {
                if (sender is DirectoryInfo directoryInfo)
                {
                    OnDirectoriesChanged(directoryInfo);
                    // đồng thời thêm đường dẫn đó vào trong backstack để dùng cho việc back lại
                    CurrentStack.Insert(0, directoryInfo);
                    DirectoryHistory[0].Items.Add(
                        new MenuItem() { Header = directoryInfo.Name});
                }
                
            }, () => true);

            OnItemCutted = new CommandHandler((sender) => 
            {
                if (sender is DirectoryInfo directoryInfo)
                {
                    _directoryCommand = (directoryInfo, true);
                }
            }, () => true);

            OnItemCopied = new CommandHandler((sender) =>
            {
                if (sender is DirectoryInfo directoryInfo)
                {
                    _directoryCommand = (directoryInfo, false);
                }
            }, () => true);

            OnItemPasted = new CommandHandler((sender) => 
            {
                var newPath = Path.Combine(CurrentPath, _directoryCommand.Item1.Name);
                if (_directoryCommand.Item2)
                {
                    try
                    {
                        Directory.Move(_directoryCommand.Item1.FullName, newPath);
                    } 
                    catch (Exception e)
                    {
                            MessageBox.Show(e.Message, "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    Directory.CreateDirectory(newPath);
                    foreach (var file in Directory.GetFiles(_directoryCommand.Item1.FullName))
                    {                                                                                          
                        var fileName = Path.GetFileName(file);
                        var destFile = Path.Combine(newPath, fileName);
                        File.Copy(file, destFile, true);
                    }
                }
                OnDirectoriesChanged(new DirectoryInfo(CurrentPath));
            }, () => _directoryCommand.Item1 != null);

            OnItemDeleted = new CommandHandler((sender) =>
            {
                if (sender is DirectoryInfo directoryInfo)
                {
                    // Show my custom dialog
                    var result = MessageBox.Show(
                        "Are you sure you want to delete this folder?",
                        directoryInfo.FullName,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (result == MessageBoxResult.No) return;
                    // Delete this folder
                    FileSystem.DeleteDirectory(
                        directoryInfo.FullName, 
                        UIOption.OnlyErrorDialogs,
                        // live folder RecycleBin
                        RecycleOption.SendToRecycleBin);
                    OnDirectoriesChanged(new DirectoryInfo(CurrentPath));
                }
            }, () => true);
        }

        private (DirectoryInfo, bool) _directoryCommand;

        void OnDirectoriesChanged(DirectoryInfo directoryInfo)
        {
            try
            {
                CurrentPath = directoryInfo.FullName;
                Directories = directoryInfo.GetDirectories();
            }
            catch (UnauthorizedAccessException e)
            {
                MessageBox.Show(e.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        } 

        public ICommand OnBackClicked { get; }
        public ICommand OnForwardClicked { get; }
        public ICommand OnUpClicked { get; }
        public ICommand OnItemClicked { get; }
        public ICommand OnItemSearch { get;  }
        public ICommand OnItemCutted { get; }
        public ICommand OnItemCopied { get; }
        public ICommand OnItemPasted { get; }
        public ICommand OnItemDeleted { get; }
    }
}
        



    

