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
    private PopupMenu BuildMenu;
    private List<Action> BuildMenuCallbacks;

    private List<Control> tabs = new();
    private TabBar tabBar;
    private Control tabHolder;
    private Control fileExplorerTab;

	[Export]
	public PackedScene ProjectSettingsUI;

	[Export]
    public PackedScene DecorColorShiftWizard;

    private List<PackedScene> wizards = new();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SetupMenuBar();
        SetupTabs();
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

        /*FileMenu.AddItem("Recent Projects");
        FileMenuCallbacks.Add(null);*/

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

        FileMenu.AddItem("Program Settings");
        FileMenuCallbacks.Add(() =>
        {
            Control settingsUI = ProjectSettingsUI.Instantiate<Control>();
            GetTree().Root.AddChild(settingsUI);
            settingsUI.MoveToFront();
        });

        /*if (FileMenu != null && RecentProjects != null)
        {
            FileMenu.SetItemSubmenuNode(RecentProjectsItemIndex, RecentProjects);
        }*/

        SetupWizardList(menuBar);


        BuildMenu = new PopupMenu();
        BuildMenuCallbacks = new();
        BuildMenu.Name = "Build";
        menuBar.AddChild(BuildMenu);
        BuildMenu.Connect("index_pressed", Callable.From((int index) => {
            if (BuildMenuCallbacks[index] != null)
            {
                BuildMenuCallbacks[index]();
            }
        }));

        BuildMenu.AddItem("Convert Files");
        BuildMenuCallbacks.Add(() =>
        {
            ProjectHolder.project.ConvertFiles();
        });

        BuildMenu.AddItem("Convert & Compile Project");
        BuildMenuCallbacks.Add(() =>
        {
            ProjectHolder.project.ConvertAndCompile();
        });
    }

    public void SetupWizardList(MenuBar menu)
    {
        PopupMenu pm = new PopupMenu();
        pm.Name = "Wizards";
        menu.AddChild(pm);
        AddWizard(pm, "Decor color-shift", DecorColorShiftWizard);

        pm.Connect("index_pressed", Callable.From((int i) =>
        {
            Control wiz = wizards[i].Instantiate<Control>();
            AddTab(wiz);
        }));
    }

    public void AddWizard(PopupMenu menu, string name, PackedScene scene)
    {
        menu.AddItem(name);
        wizards.Add(scene);
    }

    public void SetupTabs()
    {
        tabBar = GetNode<TabBar>("%TabBar");
        tabHolder = GetNode<Control>("%TabHolder");

        tabBar.Connect("tab_changed", Callable.From((int index) => ChangeTab(index)));

        fileExplorerTab = tabHolder.GetChild<Control>(0);
        AddTab(fileExplorerTab);
        ChangeTab(0);
    }

    public void ChangeTab(int index)
    {
        foreach(Control child in tabHolder.GetChildren())
        {
            child.Visible = false;
        }
        Control ctrl = tabBar.GetTabMetadata(index).As<Control>();
        if (ctrl != null)
        {
            ctrl.Visible = true;
            tabBar.TabCloseDisplayPolicy = (ctrl == fileExplorerTab) ? TabBar.CloseButtonDisplayPolicy.ShowNever : TabBar.CloseButtonDisplayPolicy.ShowActiveOnly;
        }
    }

    public void CloseTab(int index)
    {
        Control ctrl = tabHolder.GetChild<Control>(index);
        if(ctrl != fileExplorerTab)
        {
            tabHolder.RemoveChild(ctrl);
            tabs.Remove(ctrl);
            ctrl.QueueFree();
        }
    }

    public void AddTab(Control tab)
    {
        if(!tabHolder.IsAncestorOf(tab))
        {
            tabHolder.AddChild(tab);
        }
        tabBar.AddTab(tab.Name);
        tabBar.SetTabMetadata(tabBar.TabCount - 1, tab);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}