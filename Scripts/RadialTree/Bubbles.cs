using Godot;
using System.Collections.Generic;

public partial class Bubbles : MultiMeshInstance2D
{
    private int _count;

    public override void _Ready()
    {
        if (Multimesh == null)
            GD.PushError("Bubbles: MultiMesh is null. Assign it in the scene.");
    }

    public void Clear()
    {
        _count = 0;

        if (Multimesh != null)
            Multimesh.InstanceCount = 0;
    }

    public int AddBubble(Vector2 position, float radius)
    {
        if (Multimesh == null)
            return -1;

        int index = _count;
        _count++;

        Multimesh.InstanceCount = _count;

        // Assuming your CircleMesh is a unit-ish circle.
        // If it’s diameter=1, then scale should be radius*2.
        // If it’s radius=1, then scale should be radius.
        // Start with diameter=1 assumption (most common when modeling a unit circle mesh):
        float diameter = radius * 2.0f;

        var xform = new Transform2D(0.0f, Vector2.Zero, 0.0f, position);
        xform = xform.Scaled(new Vector2(diameter, diameter));

        Multimesh.SetInstanceTransform2D(index, xform);

        // Optional: per-instance color if you want
        // Multimesh.SetInstanceColor(index, new Color(1, 1, 1));

        return index;
    }
}