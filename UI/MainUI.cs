using Godot;
using System;

public partial class MainUI : Control
{
	[Export]
	public PopupMenu FileMenu;
	[Export]
	public int RecentProjectsItemIndex;
	[Export]
	public PopupMenu RecentProjects;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (FileMenu != null && RecentProjects != null)
		{
			FileMenu.SetItemSubmenuNode(RecentProjectsItemIndex, RecentProjects);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
