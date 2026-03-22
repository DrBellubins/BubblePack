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
		
		if (packageManagerType == LPU.PackageManagerType.Pacman)
		{
			packages = Pacman.GetInstalledPackages();
			GD.Print($"Loaded {packages.Count} installed packages from pacman -Qi.");
		}
		else
			GD.PushWarning($"Package manager {packageManagerType} not implemented yet.");
	}

	private bool hasRun = false;
	public override void _Process(double delta)
	{
		if (!hasRun)
		{
			// Quick sanity check output
			int printed = 0;
			foreach (var kvp in packages)
			{
				var pkg = kvp.Value;
				GD.Print($"{pkg.Name} {pkg.Version} {pkg.SizeInMB}MB deps={pkg.Dependencies?.Count ?? 0}");

				printed++;
				
				if (printed >= 10)
					break;
			}

			hasRun = true;
		}
	}
}
