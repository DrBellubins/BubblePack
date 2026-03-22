using Godot;
using System;

public partial class CameraControls : Camera2D
{
	[Export] public float ZoomSpeed = 1.5f;
	
	private Vector2 mouseDelta = Vector2.Zero;
	
	private float zoom = 1.0f;
	
	public override void _Ready()
	{
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion eventMouseMotion)
			mouseDelta = eventMouseMotion.ScreenRelative;
	}

	public override void _Process(double delta)
	{
		var isDragging = Input.IsActionPressed("left_click");

		if (Input.IsActionJustReleased("mouse_wheel_up"))
			zoom += ZoomSpeed;
		
		if (Input.IsActionJustReleased("mouse_wheel_down"))
			zoom -= ZoomSpeed;

		zoom = Mathf.Clamp(zoom, 0.5f, 100f);
		
		SetZoom(new Vector2(zoom, zoom));
		
		GD.Print($"Zoom: {zoom}");
		
		if (isDragging)
		{
			Position -= mouseDelta;
		}
	}
}
