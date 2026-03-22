using Godot;
using System.Collections.Generic;

public partial class TreeLines : Node2D
{
    public struct Segment
    {
        public Vector2 A;
        public Vector2 B;
        public Color Color;
    }

    [Export] public float LineWidth { get; set; } = 1.0f;

    private readonly List<Segment> _segments = new();

    public void Clear()
    {
        _segments.Clear();
        QueueRedraw();
    }

    public void SetSegments(List<Segment> segments)
    {
        _segments.Clear();
        _segments.AddRange(segments);
        QueueRedraw();
    }

    public override void _Draw()
    {
        foreach (Segment s in _segments)
            DrawLine(s.A, s.B, s.Color, LineWidth);
    }
}