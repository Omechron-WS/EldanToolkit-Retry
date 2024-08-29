using EldanToolkit.Project;
using Godot;
using System.Linq;
using System.IO;

public partial class ProjectFileSystemView : Control
{
	[Export]
	public ProjectFileSystem fileSystem;

	[Export]
	public VBoxContainer fileList;

	[Export]
	public PackedScene fileEntryScene;

	[Signal]
	public delegate void SelectedFileChangedEventHandler(string filename, string filepath);

	public string selectedFile = null;

	protected string currentFolder = "";

	private bool needsRefresh = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(needsRefresh)
		{
            populate();
			needsRefresh = false;
        }
	}

	public void populate()
    {
		foreach(var child in fileList.GetChildren())
		{
			child.QueueFree();
            fileList.RemoveChild(child);
		}

		if (currentFolder != "")
		{
			FileEntry fe = fileEntryScene.Instantiate<FileEntry>();
			fe.SetData("<==", FileType.Directory, () => SetFolder(GetParentFolder(currentFolder)));
			fileList.AddChild(fe);
		}

        var list = fileSystem.GetFoldersInFolder(currentFolder);
        foreach (var file in list)
        {
			string fileName = Path.GetFileName(file);
            FileEntry fe = fileEntryScene.Instantiate<FileEntry>();
            fe.SetData(fileName, FileType.Directory, () => SetFolder(Path.Combine(currentFolder, fileName)));
            fileList.AddChild(fe);
        }

        list = fileSystem.GetFilesInFolder(currentFolder);
        foreach (var file in list)
        {
            string fileName = Path.GetFileName(file);
            FileEntry fe = fileEntryScene.Instantiate<FileEntry>();
			fe.SetData(fileName, GetFileType(file), () => selectedFile = fileName);
            fileList.AddChild(fe);
        }
    }

	public FileType GetFileType(string fileName)
	{
		string extension = Path.GetExtension(fileName).ToLower();
		switch(extension)
		{
			case "tex":
				return FileType.Image;
			case "tbl":
				return FileType.Table;
		}
		return FileType.UnknownFile;
	}

	protected string GetParentFolder(string path)
	{
		string[] split = path.Split('\\', '/');
		if (split.Length > 0)
		{
			return Path.Combine(split.SkipLast(1).ToArray());
		}
		return "";
	}

	protected void SetFolder(string folder)
	{
		currentFolder = folder;
		SetFile(null);
        Refresh();
	}

	protected void SetFile(string filename)
	{
		selectedFile = filename;
		EmitSignal(SignalName.SelectedFileChanged, filename, Path.Combine(currentFolder, filename));
	}

	public void Refresh()
	{
		needsRefresh = true;
	}
}
