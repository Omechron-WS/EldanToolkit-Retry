using Godot;
using LibNexus.Core;
using LibNexus.Files;
using System;
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
        Progress progress;

        public ArchiveFilePair(string index, bool hasArchive, CancellationToken token)
        {
            progress = new Progress();
            task = FileSystem.Create(progress, index, hasArchive, null, token);
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

        public string projectFilesPath { get { return Path.Combine(projectPath, "ProjectFiles"); } }

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
                    if (!Engine.IsEditorHint())
                    {
                        ReadArchive();
                    }
                }
            }
        }

        private string archivePath;
        private List<ArchiveFilePair> archiveFiles = new List<ArchiveFilePair>();
        CancellationTokenSource cts = new CancellationTokenSource();

        private void ReadArchive()
        {
            archiveFiles.Clear();
            archiveFiles.Add(new ArchiveFilePair(Path.Combine(archivePath, "ClientData"), true, cts.Token));
            archiveFiles.Add(new ArchiveFilePair(Path.Combine(archivePath, "ClientDataEN"), true, cts.Token));
            archiveFiles.Add(new ArchiveFilePair(Path.Combine(archivePath, "Path"), false, cts.Token));
            archiveFiles.Add(new ArchiveFilePair(Path.Combine(archivePath, "Client64"), false, cts.Token));
        }

        public SortedSet<string> GetFilesInFolder(string path)
        {
            SortedSet<string> files = new SortedSet<string>();
            if (projectFilesPath != null)
            {
                string fullPath = Path.Combine(projectFilesPath, path);
                if (Directory.Exists(fullPath))
                {
                    files.UnionWith(Directory.GetFiles(Path.Combine(projectFilesPath, path)).Select((p) => Path.GetFileName(p)));
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
            if (projectFilesPath != null)
            {
                string fullPath = Path.Combine(projectFilesPath, path);
                if (Directory.Exists(fullPath))
                {
                    folders.UnionWith(Directory.GetDirectories(Path.Combine(projectFilesPath, path)).Select((p) => Path.GetFileName(p)));
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
            return Path.Exists(Path.Combine(projectFilesPath, path));
        }

        public bool IsInArchive(string path)
        {
            foreach(var a in archiveFiles)
            {
                if(a.GetFileSystem().GetFile(path) != null)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddtoProject(string path)
        {
            foreach(var a in archiveFiles)
            {
                byte[] file = a.GetFileSystem().Read(path);
                if(file != null)
                {
                    string dir = Path.GetDirectoryName(path);
                    Directory.CreateDirectory(Path.Combine(projectFilesPath, dir));
                    FileStream fs = new FileStream(Path.Combine(projectFilesPath, path), FileMode.CreateNew, System.IO.FileAccess.Write);
                    fs.Write(file);
                    fs.Flush();
                    fs.Dispose();
                    return;
                }
            }
        }

        public void RemoveFromProject(string path)
        {
            File.Delete(Path.Combine(projectFilesPath, path));
        }

        public async void CompileProject()
        {
            Progress p = new Progress();
            FileSystem fs = await FileSystem.Create(p, projectPath + "/custom", true, null, cts.Token);
            var files = GetAllProjectFiles(projectFilesPath);
            foreach(var file in files)
            {
                string relativePath = Path.GetRelativePath(file, projectFilesPath).Replace('\\', '/');
                byte[] bytes = File.ReadAllBytes(file);
                fs.Write(relativePath, bytes, DateTime.Now);
            }
            fs.Dispose();
        }

        public List<string> GetAllProjectFiles(string path)
        {
            List<string> files = new List<string>();
            foreach(var dir in Directory.EnumerateDirectories(path))
            {
                files.AddRange(GetAllProjectFiles(dir));
            }
            files.AddRange(Directory.EnumerateFiles(path));
            return files;
        }
    }
}
