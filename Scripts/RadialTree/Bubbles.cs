using Godot;
using System;

public partial class Bubbles : MultiMeshInstance2D
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public void AddBubble(Vector2 position, float size)
	{
		// Called from RadialTreeGenerator
		// Adds a circle to the multi-mesh mesh instance at given position and size
	}
}
