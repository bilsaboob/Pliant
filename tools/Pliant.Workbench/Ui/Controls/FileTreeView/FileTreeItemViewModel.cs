using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Demo.ViewModel;
using MahApps.Metro.IconPacks;
using RapidFx.Ui.Controls.Icon;

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
            this.IconType = 0;
            this.IconSize = 15;
            this.IconColor = new SolidColorBrush(Color.FromRgb(173, 173, 173));

            SetupStyle();
        }

        private void SetupStyle()
        {
            Padding = new Thickness(0);
            Margin = new Thickness(0);
            BorderThickness = new Thickness(0);
        }

        public virtual string Name => info.Name;
        public virtual string FullPath => info.FullName;
        public virtual object IconType { get; set; }
        public virtual int IconSize { get; set; }
        public virtual SolidColorBrush IconColor { get; set; }

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
            this.IconSize = 15;
            SetIconTypeByFileExtension();
        }

        private void SetIconTypeByFileExtension()
        {
            if(info == null) return;

            var ext = info.Extension.ToLower().Trim('.');
            switch (ext)
            {
                case "cs":
                    IconType = MfixxIconKind.CSharp;
                    break;
                case "txt":
                    IconType = PackIconOcticonsKind.FileText;
                    break;
                case "js":
                    IconType = MfixxIconKind.JavaScript;
                    break;
                case "pdf":
                    IconType = PackIconFontAwesomeKind.FilePdfOutline;
                    break;
                case "html":
                    IconType = MfixxIconKind.HTML;
                    break;
                case "xml":
                    IconType = FileIconKind.YAML;
                    break;
                default: break;
            }
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
            this.IconSize = 15;
            IconType = PackIconOcticonsKind.FileDirectory;
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
