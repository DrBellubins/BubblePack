using Godot;
using System;
using System.Collections.Generic;
using BubblePack.Scripts;

public partial class RadialTreeGenerator : Node2D
{
	[Export] public Bubbles Bubbles;
	[Export] public TreeLines Branches;
	
	private LPU.PackageManagerType packageManagerType;
	private Dictionary<string, PackageData> packages = new();
	
	public override void _Ready()
	{
		packageManagerType = LPU.DetectPackageManager();
		
		if (packageManagerType == LPU.PackageManagerType.Pacman)
		{
			packages = Pacman.GetInstalledPackages();
			GD.Print($"Loaded {packages.Count} installed packages from pacman -Qi.");
		}
		else
			GD.PushWarning($"Package manager {packageManagerType} not implemented yet.");
	}

	public override void _Process(double delta)
	{
		
	}
}
