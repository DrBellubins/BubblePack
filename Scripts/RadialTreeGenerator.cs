using Godot;
using System;

public partial class RadialTreeGenerator : Node
{
	private LPU.PackageManagerType packageManagerType;
	
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
