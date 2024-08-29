using Godot;
using LibNexus.Core;
using LibNexus.Files;
using SharpCompress;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace EldanToolkit.Project
{
    public class ArchiveFilePair
    {
        FileSystem fs = null;
        Task<FileSystem> task = null;
        CancellationTokenSource cts;
        Progress progress;

        public ArchiveFilePair(string index, bool hasArchive)
        {
            cts = new CancellationTokenSource();
            progress = new Progress();
            task = FileSystem.Create(progress, index, hasArchive, null, cts.Token);
        }

        public FileSystem GetFileSystem()
        {
            if (fs == null && task != null)
            {
                task.Wait();
                fs = task.Result;
                task = null;
            }
            return fs;
        }
    }

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

        private string _archivePath;
        private List<ArchiveFilePair> archiveFiles = new List<ArchiveFilePair>();

        private void ReadArchive()
        {
            archiveFiles.Clear();
            archiveFiles.Add(new ArchiveFilePair(Path.Combine(ArchivePath, "ClientData"), true));
            archiveFiles.Add(new ArchiveFilePair(Path.Combine(ArchivePath, "ClientDataEN"), true));
            archiveFiles.Add(new ArchiveFilePair(Path.Combine(ArchivePath, "Path"), false));
            archiveFiles.Add(new ArchiveFilePair(Path.Combine(ArchivePath, "Client64"), false));
        }

        public SortedSet<string> GetFilesInFolder(string path)
        {
            SortedSet<string> files = new SortedSet<string>();
            if (projectPath != null)
            {
                string fullPath = Path.Combine(projectPath, path);
                if (Directory.Exists(fullPath))
                {
                    files.UnionWith(Directory.GetFiles(Path.Combine(projectPath, path)));
                }
            }
            foreach(var a in archiveFiles)
            {
                files.UnionWith(a.GetFileSystem().ListFiles(path));
            }
            return files;
        }

        public SortedSet<string> GetFoldersInFolder(string path)
        {
            SortedSet<string> folders = new SortedSet<string>();
            if (projectPath != null)
            {
                string fullPath = Path.Combine(projectPath, path);
                if (Directory.Exists(fullPath))
                {
                    folders.UnionWith(Directory.GetDirectories(Path.Combine(projectPath, path)));
                }
            }
            foreach (var a in archiveFiles)
            {
                folders.UnionWith(a.GetFileSystem().ListDirectories(path));
            }
            return folders;
        }
    }
}
