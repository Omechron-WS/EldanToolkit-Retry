using Godot;
using LibNexus.Core;
using LibNexus.Files;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                return archivePath;
            }
            set
            {
                if (archivePath != value)
                {
                    archivePath = value;
                    ReadArchive();
                }
            }
        }

        private string archivePath;
        private List<ArchiveFilePair> archiveFiles = new List<ArchiveFilePair>();

        private void ReadArchive()
        {
            archiveFiles.Clear();
            archiveFiles.Add(new ArchiveFilePair(Path.Combine(archivePath, "ClientData"), true));
            archiveFiles.Add(new ArchiveFilePair(Path.Combine(archivePath, "ClientDataEN"), true));
            archiveFiles.Add(new ArchiveFilePair(Path.Combine(archivePath, "Path"), false));
            archiveFiles.Add(new ArchiveFilePair(Path.Combine(archivePath, "Client64"), false));
        }

        public SortedSet<string> GetFilesInFolder(string path)
        {
            SortedSet<string> files = new SortedSet<string>();
            if (projectPath != null)
            {
                string fullPath = Path.Combine(projectPath, path);
                if (Directory.Exists(fullPath))
                {
                    files.UnionWith(Directory.GetFiles(Path.Combine(projectPath, path)).Select((p) => Path.GetFileName(p)));
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
                    folders.UnionWith(Directory.GetDirectories(Path.Combine(projectPath, path)).Select((p) => Path.GetFileName(p)));
                }
            }
            foreach (var a in archiveFiles)
            {
                folders.UnionWith(a.GetFileSystem().ListDirectories(path));
            }
            return folders;
        }

        public bool IsInProject(string path)
        {
            return Path.Exists(Path.Combine(projectPath, path));
        }

        public void AddtoProject(string path)
        {
            foreach(var a in archiveFiles)
            {
                byte[] file = a.GetFileSystem().Read(path);
                if(file != null)
                {
                    string dir = Path.GetDirectoryName(path);
                    Directory.CreateDirectory(Path.Combine(projectPath, dir));
                    FileStream fs = new FileStream(Path.Combine(projectPath, path), FileMode.CreateNew, System.IO.FileAccess.Write);
                    fs.Write(file);
                    fs.Flush();
                    fs.Dispose();
                    return;
                }
            }
        }

        public void RemoveFromProject(string path)
        {
            File.Delete(Path.Combine(projectPath, path));
        }
    }
}
