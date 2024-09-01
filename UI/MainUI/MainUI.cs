using EldanToolkit.Project;
using Godot;
using System;
using System.Collections.Generic;

public partial class MainUI : Control
{
    private MenuBar menuBar;
	private PopupMenu FileMenu;
    private List<Action> FileMenuCallbacks;
    private PopupMenu RecentProjects;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SetupMenuBar();
    }

    public void SetupMenuBar()
    {
        menuBar = GetNode<MenuBar>("Panel/VBoxContainer/MenuBar");
        FileMenu = new PopupMenu();
        FileMenuCallbacks = new();
        FileMenu.Name = "File";
        menuBar.AddChild(FileMenu);
        FileMenu.Connect("index_pressed", Callable.From((int index) => {
            if (FileMenuCallbacks[index] != null)
            {
                FileMenuCallbacks[index]();
            }
        }));

        FileMenu.AddItem("New Project");
        FileMenuCallbacks.Add(() =>
        {
            FileDialog fd = new FileDialog();
            fd.FileMode = FileDialog.FileModeEnum.OpenDir;
            fd.Access = FileDialog.AccessEnum.Filesystem;
            fd.UseNativeDialog = true;
            fd.CurrentDir = ProgramSettings.GetLastProject() ?? "C:/";
            fd.Show();
            string path = fd.CurrentFile;
            if (path != null)
            {
                if (ProjectHolder.Instance.NewProject(path))
                {
                    ProgramSettings.NoteProjectLoaded(path);
                }
            }
        });

        FileMenu.AddItem("Recent Projects");
        FileMenuCallbacks.Add(null);

        FileMenu.AddItem("Load Project");
        FileMenuCallbacks.Add(() =>
        {
            FileDialog fd = new FileDialog();
            fd.FileMode = FileDialog.FileModeEnum.OpenDir;
            fd.Access = FileDialog.AccessEnum.Filesystem;
            fd.UseNativeDialog = true;
            fd.CurrentDir = ProgramSettings.GetLastProject() ?? "C:/";
            fd.Show();
            string path = fd.CurrentFile;
            if(path != null)
            {
                if(ProjectHolder.Instance.LoadProject(path))
                {
                    ProgramSettings.NoteProjectLoaded(path);
                }
            }
        });

        FileMenu.AddItem("Set Archive Path");
        FileMenuCallbacks.Add(() =>
        {
            if (ProjectHolder.project?.FileSystem == null) { return; }
            FileDialog fd = new FileDialog();
            fd.FileMode = FileDialog.FileModeEnum.OpenDir;
            fd.Access = FileDialog.AccessEnum.Filesystem;
            fd.UseNativeDialog = true;
            fd.CurrentDir = ProjectHolder.project?.FileSystem?.ArchivePath ?? "C:/";
            fd.Show();
            string path = fd.CurrentFile;
            if (path != null)
            {
                ProjectHolder.project.FileSystem.ArchivePath = path;
                ProjectHolder.project.Save();
            }
        });

        /*if (FileMenu != null && RecentProjects != null)
        {
            FileMenu.SetItemSubmenuNode(RecentProjectsItemIndex, RecentProjects);
        }*/
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
