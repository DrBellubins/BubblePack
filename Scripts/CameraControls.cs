using Godot;
using System;

public partial class CameraControls : Camera2D
{
    [Export] public float ZoomSpeed = 1.5f;
    [Export] public float StartZoom = 50f;

    private bool _isDragging;
    private Vector2 _dragStartMouseScreen;
    private Vector2 _dragStartCameraPosition;

    private float _zoom = 1.0f;

    public override void _Ready()
    {
        SetZoomAmount(StartZoom);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            _isDragging = mb.Pressed;

            if (_isDragging)
            {
                _dragStartMouseScreen = mb.Position;
                _dragStartCameraPosition = Position;
            }

            return;
        }

        if (_isDragging && @event is InputEventMouseMotion mm)
        {
            Vector2 screenDelta = mm.Position - _dragStartMouseScreen;

            Vector2 worldDelta = new Vector2(
                screenDelta.X / Zoom.X,
                screenDelta.Y / Zoom.Y
            );

            Position = _dragStartCameraPosition - worldDelta;
            return;
        }

        if (@event.IsActionPressed("mouse_wheel_up"))
        {
            _zoom *= 1.1f;
            _zoom = Mathf.Clamp(_zoom, 0.1f, 5.0f);
            SetZoomAmount(_zoom);
        }

        if (@event.IsActionPressed("mouse_wheel_down"))
        {
            _zoom /= 1.1f;
            _zoom = Mathf.Clamp(_zoom, 0.1f, 5.0f);
            SetZoomAmount(_zoom);
        }
    }

    public override void _Process(double delta)
    {
        if (!_isDragging)
        {
            return;
        }

        Vector2 mouseNowScreen = GetViewport().GetMousePosition();

        // Screen delta -> world delta.
        // When zoom is larger, moving the mouse should move the camera more in world units.
        Vector2 screenDelta = mouseNowScreen - _dragStartMouseScreen;
        Vector2 worldDelta = screenDelta * Zoom;

        // Subtract so the content "grabs" and follows the mouse.
        Position = _dragStartCameraPosition - worldDelta;
    }

    private void SetZoomAmount(float zoomAmount)
    {
        _zoom = zoomAmount;
        Zoom = new Vector2(_zoom, _zoom);
    }
}