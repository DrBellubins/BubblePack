using Godot;
using System;

public partial class TreeLines : Node2D
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public override void _Draw()
	{
		// Placeholder:
		DrawLine(Vector2.Zero, new Vector2(), Colors.White);
	}
}
