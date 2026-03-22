using Godot;
using System;

public partial class RadialTreeGenerator : Node
{
	private LinuxPackageUtils.PackageManagerType packageManagerType;
	private string[] installedPackages = new string[] {};
	
	public override void _Ready()
	{
		packageManagerType = LinuxPackageUtils.DetectPackageManager();
		installedPackages = LinuxPackageUtils.GetInstalledPackages();
	}

	private bool hasRun = false;
	public override void _Process(double delta)
	{
		if (!hasRun)
		{
			foreach (var package in installedPackages)
				GD.Print(package);
			
			hasRun = true;
		}
	}
}
