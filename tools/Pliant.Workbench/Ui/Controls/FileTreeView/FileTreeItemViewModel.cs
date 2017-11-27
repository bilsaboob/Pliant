using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Demo.ViewModel;

namespace Pliant.Workbench.Ui.Controls.FileTreeView
{
    public abstract class FileSystemTreeItemViewModel : TreeItemViewModel
    {
        private FileSystemInfo info;

        protected FileSystemTreeItemViewModel(TreeItemViewModel parent, FileSystemInfo info, bool lazyLoadChildren)
            : base(parent, lazyLoadChildren)
        {
            this.info = info;
            this.DisplayName = info.Name;
        }

        public virtual string Name => info.Name;
        public virtual string FullPath => info.FullName;

        public override string ToString()
        {
            return FullPath;
        }
    }

    public class FileTreeItemViewModel : FileSystemTreeItemViewModel
    {
        private FileInfo info;

        public FileTreeItemViewModel(TreeItemViewModel parent, FileInfo info)
            : base(parent, info, false)
        {
            this.info = info;
        }
        
        protected override void LoadChildren()
        {
            //No children available for a file
        }
    }

    public class FolderTreeItemViewModel : FileSystemTreeItemViewModel
    {
        private DirectoryInfo info;

        public FolderTreeItemViewModel(TreeItemViewModel parent, DirectoryInfo info)
            : base(parent, info, true)
        {
            this.info = info;
        }

        protected override void LoadChildren()
        {
            //Load the sub folder
            var subInfos = info.GetFileSystemInfos().ToList();
            foreach (var subInfo in subInfos)
            {
                if (subInfo is FileInfo)
                {
                    Children.Add(new FileTreeItemViewModel(this, (FileInfo) subInfo));
                }
                else if (subInfo is DirectoryInfo)
                {
                    Children.Add(new FolderTreeItemViewModel(this, (DirectoryInfo) subInfo));
                }
            }
        }
    }
}
