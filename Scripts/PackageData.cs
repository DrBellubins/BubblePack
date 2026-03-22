using Godot;
using System;
using System.Collections.Generic;

namespace BubblePack.Scripts;

public struct PackageData
{
	public string Name;
	public string Description;
	public string Version;
	public string Author;
	public float SizeInMB;
	public List<string> Dependencies;
}
