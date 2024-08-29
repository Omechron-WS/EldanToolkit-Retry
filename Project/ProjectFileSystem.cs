using Godot;
using Nexus.Archive;
using System.Collections.Generic;
using System.IO;

namespace EldanToolkit.Project
{
    [GlobalClass]
    public partial class ProjectFileSystem : Resource
    {
        [Export(PropertyHint.GlobalDir)]
        public string projectPath;

        [Export(PropertyHint.GlobalDir)]
        public string ArchivePath
        {
            get
            {
                return _archivePath;
            }
            set
            {
                if (_archivePath != value)
                {
                    _archivePath = value;
                    ReadArchive();
                }
            }
        }

        public string _archivePath;

        private void ReadArchive()
        {
        }

        public SortedSet<string> GetFilesInFolder(string path)
        {
            SortedSet<string> files = new SortedSet<string>();
            if (projectPath != null)
            {
                files.UnionWith(Directory.GetFiles(Path.Combine(projectPath, path)));
            }
            return files;
        }

        public SortedSet<string> GetFoldersInFolder(string path)
        {
            SortedSet<string> folders = new SortedSet<string>();
            if (projectPath != null)
            {
                folders.UnionWith(Directory.GetDirectories(Path.Combine(projectPath, path)));
            }
            return folders;
        }
    }
}
