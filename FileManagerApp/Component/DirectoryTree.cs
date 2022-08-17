using System.Collections.Generic;
using System.IO;

namespace FileManagerApp.Component
{
    /// <summary>
    ///  A model class for binding on [TreeView]
    /// </summary>
    public class DirectoryTree
    {
        public string Title { get; set; }

        public string ImageSource { get; set; }

        public IList<DirectoryInfo> Directories { get; set; } = new List<DirectoryInfo>();
    }
}
