using System;
using System.Collections.Generic;
using Godot;

public partial class Bubbles : MultiMeshInstance2D
{
    [Export] public float MeshUnitsToPixels { get; set; } = 1.0f;
    
    private int _count;

    private List<Vector2> _positions = new();
    
    public override void _Ready()
    {
        if (Multimesh == null)
        {
            GD.PushError("Bubbles: MultiMesh is null. Assign it in the scene.");
            return;
        }

        if (Multimesh.Mesh == null)
        {
            GD.PushError("Bubbles: Multimesh.Mesh is null. Assign a mesh to the MultiMesh resource.");
            return;
        }

        GD.Print($"Bubbles: Mesh type = {Multimesh.Mesh.GetClass()}");
    }

    public override void _Draw()
    {
        foreach (Vector2 position in _positions)
        {
            DrawCircle(position, 1f, Colors.White);
        }
    }

    public void Clear()
    {
        _count = 0;

        if (Multimesh != null)
        {
            Multimesh.InstanceCount = 0;
        }
    }

    public int AddBubble(Vector2 position, float radiusPixels)
    {
        if (Multimesh == null || Multimesh.Mesh == null)
            return -1;

        if (!float.IsFinite(radiusPixels) || radiusPixels <= 0.01f)
            return -1;

        int index = _count;
        _count++;

        _positions.Add(position);
        
        Multimesh.InstanceCount = _count;
        Multimesh.VisibleInstanceCount = _count;

        // IMPORTANT:
        // Your Blender circle mesh might be "big" or "tiny" in its own units.
        // MeshUnitsToPixels lets you compensate without re-exporting.
        float diameter = radiusPixels * 2.0f * MeshUnitsToPixels;

        Transform2D xform = Transform2D.Identity;
        xform.Origin = position;

        // Scale around origin:
        xform = xform.Scaled(new Vector2(diameter, diameter));

        Multimesh.SetInstanceTransform2D(index, xform);

        GD.Print($"AddBubble idx={index} pos={position} radius={radiusPixels}" +
                 $" diameterScale={diameter} count={Multimesh.InstanceCount}");

        return index;
    }
}