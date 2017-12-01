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

        private object _iconType;
        public virtual object IconType
        {
            get => _iconType;
            set
            {
                _iconType = value;
                OnPropertyChanged(nameof(IconType));
            }
        }

        private int _iconSize;
        public virtual int IconSize
        {
            get => _iconSize;
            set
            {
                _iconSize = value;
                OnPropertyChanged(nameof(IconSize));
            }
        }

        private SolidColorBrush _iconColor;
        public virtual SolidColorBrush IconColor
        {
            get => _iconColor;
            set
            {
                _iconColor = value;
                OnPropertyChanged(nameof(IconColor));
            }
        }

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
                    IconType = PackIconMaterialKind.LanguageCsharp;
                    break;
                case "txt":
                    IconType = PackIconMaterialKind.FileDocument;
                    break;
                case "js":
                    IconType = PackIconMaterialKind.LanguageJavascript;
                    break;
                case "pdf":
                    IconType = PackIconMaterialKind.FilePdf;
                    break;
                case "html":
                    IconType = PackIconMaterialKind.LanguageHtml5;
                    break;
                case "xml":
                    IconType = PackIconMaterialKind.FileXml;
                    break;
                case "xaml":
                    IconType = PackIconMaterialKind.Xaml;
                    break;
                case "sln":
                    IconType = PackIconMaterialKind.Visualstudio;
                    break;
                case "gitignore":
                    IconType = PackIconMaterialKind.Git;
                    break;
                case "config":
                    IconType = PackIconOcticonsKind.Gear;
                    break;
                case "dll":
                    IconType = PackIconOcticonsKind.Gear;
                    break;
                case "pdb":
                    IconType = PackIconMaterialKind.Database;
                    break;
                default:
                    IconType = PackIconMaterialKind.FileOutline;
                    break;
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
            IconType = PackIconFontAwesomeKind.Folder;
        }

        public override bool IsExpanded
        {
            get { return base.IsExpanded; }
            set
            {
                base.IsExpanded = value;

                if (IsExpanded)
                {
                    IconType = PackIconFontAwesomeKind.FolderOpen;
                }
                else
                {
                    IconType = PackIconFontAwesomeKind.Folder;
                }
            }
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
