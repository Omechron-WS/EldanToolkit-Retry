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

	public string selectedFilePath { get { return selectedFile == null ? null : $"{currentFolder}{selectedFile}"; } }

	protected string currentFolder = ""; // in the format "abc/def/"

	private bool needsRefresh = true;

    private Button AddToProject = null;
    private Button RemoveFromProject = null;

	private Color FileInProjectColor = new Color(0.5f, 0.7f, 0.6f);
    private Color DefaultFileColor = new Color(0.5f, 0.5f, 0.5f);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AddToProject = GetNode<Button>("%AddToProject");
		AddToProject.Connect("pressed", Callable.From(() => {
			if (selectedFile != null)
			{
				fileSystem.AddtoProject(selectedFilePath);
				Refresh();
			}
		}));
        RemoveFromProject = GetNode<Button>("%RemoveFromProject");
        RemoveFromProject.Connect("pressed", Callable.From(() => {
            if (selectedFile != null)
            {
                fileSystem.RemoveFromProject(selectedFilePath);
                Refresh();
            }
        }));
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
            fe.SetData(fileName, FileType.Directory, () => SetFolder($"{currentFolder}{fileName}/"));
            fileList.AddChild(fe);
        }

        list = fileSystem.GetFilesInFolder(currentFolder);
        foreach (var file in list)
        {
            string fileName = Path.GetFileName(file);
            FileEntry fe = fileEntryScene.Instantiate<FileEntry>();
			fe.SetData(fileName, GetFileType(file), () => SetFile(fileName));
			fe.AddThemeColorOverride("font_color", fileSystem.IsInProject($"{currentFolder}{fileName}") ? FileInProjectColor : DefaultFileColor);
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
		int pos = path.Replace('\\', '/').RFind("/", path.Length - 2);
		if(pos > 0)
		{
			return path.Substring(0, pos + 1);
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
		string fullName = filename == null ? null : Path.Combine(currentFolder, filename);
		EmitSignal(SignalName.SelectedFileChanged, filename, fullName);
		AddToProject.Visible = false;
		RemoveFromProject.Visible = false;
		if(selectedFile != null)
		{
			if (fileSystem.IsInProject(fullName))
			{
				RemoveFromProject.Visible = true;
			} else
			{
				AddToProject.Visible = true;
			}
		}
	}

	public void Refresh()
	{
		needsRefresh = true;
	}
}
