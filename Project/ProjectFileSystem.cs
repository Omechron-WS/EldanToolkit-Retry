using Godot;
using LibNexus.Core;
using LibNexus.Files;
using LibNexus.Files.ModelFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EldanToolkit.Project
{
    [GlobalClass]
    public partial class ProjectFileSystem : Node
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
                    if(String.IsNullOrWhiteSpace(value))
                    {
                        archivePath = null;
                    }
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

        DateTime dummyTime = new DateTime(2017, 1, 1);

        private void ReadArchive()
        {
            archiveFiles.Clear();
            if(archivePath == null) { return; }
            archiveFiles.Add(new ArchiveFilePair(Path.Combine(archivePath, "ClientData"), true, cts.Token));
            archiveFiles.Add(new ArchiveFilePair(Path.Combine(archivePath, "ClientDataEN"), true, cts.Token));
            //archiveFiles.Add(new ArchiveFilePair(Path.Combine(archivePath, "Patch"), false, cts.Token)); // Tells the client which index files to use
            //archiveFiles.Add(new ArchiveFilePair(Path.Combine(archivePath, "Client64"), false, cts.Token));
            //archiveFiles.Add(new ArchiveFilePair(Path.Combine(archivePath, "../Bootstrap/Bootstrap"), false, cts.Token));
            //archiveFiles.Add(new ArchiveFilePair(Path.Combine(archivePath, "../Bootstrap/Launcher"), false, cts.Token));
            //archiveFiles.Add(new ArchiveFilePair(Path.Combine(archivePath, "../Bootstrap/LauncherData"), true, cts.Token)); // Actual launcher files: CSS, HTML, etc.
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
                if (a.IsReady())
                {
                    files.UnionWith(a.GetFileSystem().ListFiles(path));
                }
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
                if (a.IsReady())
                {
                    folders.UnionWith(a.GetFileSystem().ListDirectories(path));
                }
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
                if (!a.IsReady()) continue; // File was seen in another archive, probably.
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

        public void CompileProject()
        {
            File.Delete(Path.Combine(projectPath, "custom.index"));
            File.Delete(Path.Combine(projectPath, "custom.archive"));

            Progress p = new Progress();
            FileSystem fs = FileSystem.Create(p, projectPath + "/custom", true, null, cts.Token).Result;
            var files = GetAllProjectFiles(projectFilesPath);
            foreach(var file in files)
            {
                string relativePath = Path.GetRelativePath(projectFilesPath, file).Replace('\\', '/');
                byte[] bytes = File.ReadAllBytes(file);
                fs.Write(relativePath, bytes, dummyTime);
            }
            fs.Dispose();

            ChangePatchIndex();
        }

        public void ChangePatchIndex()
        {
            File.Delete(Path.Combine(projectPath, "Patch.index"));
            File.Copy(Path.Combine(projectPath, "Patch-Original.index"), Path.Combine(projectPath, "Patch.index"));

            Progress p = new Progress();
            FileSystem fs = FileSystem.Create(p, projectPath + "/Patch", false, null, cts.Token).Result;
            fs.Write("custom.index", File.ReadAllBytes(projectPath + "/custom.index"), dummyTime);
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

        public void LoadModel(string path)
        {
            Stream stream = File.OpenRead(Path.Combine(projectFilesPath, path));
            Model mdl = new Model(stream);
        }
    }
    public class ArchiveFilePair
    {
        FileSystem fs = null;
        Task<FileSystem> task = null;
        Progress progress;

        public ArchiveFilePair(string index, bool hasArchive, CancellationToken token)
        {
            ThreadPool.QueueUserWorkItem((o) => GD.Print(""));
            progress = new Progress();
            task = Task.Run(() => FileSystem.Create(progress, index, hasArchive, null, token));
            task.ContinueWith((t) =>
            {
                Callable.From(() => ProjectHolder.FileSystemLoadEvent()).CallDeferred();
            });
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

        public bool IsReady()
        {
            return task == null || task.IsCompleted;
        }
    }
}
