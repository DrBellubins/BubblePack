using Godot;
using System;
using System.Collections.Generic;
using BubblePack.Scripts;

public partial class RadialTreeGenerator : Node
{
	private LPU.PackageManagerType packageManagerType;
	private Dictionary<string, PackageData> packages = new();
	
	public override void _Ready()
	{
		packageManagerType = LPU.DetectPackageManager();
	}

	private bool hasRun = false;
	public override void _Process(double delta)
	{
		if (!hasRun)
		{
			hasRun = true;
		}
	}
}
