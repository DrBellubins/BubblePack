using Godot;
using System;

/// <summary>
/// Utility helpers for modular fullscreen post-processing passes.
/// </summary>
public static class PostProcessingUtils
{
    public static Texture2D Blit(Texture2D input, SubViewport output, Shader shader)
    {
        return Blit(input, output, shader, null);
    }

    public static Texture2D Blit(
        Texture2D input,
        SubViewport output,
        Shader shader,
        Action<ShaderMaterial> configureMaterial
    )
    {
        if (input == null)
        {
            GD.PushWarning("PostProcessingUtils.Blit called with null input texture.");
            return null;
        }

        if (output == null)
        {
            GD.PushWarning("PostProcessingUtils.Blit called with null output viewport.");
            return null;
        }

        if (shader == null)
        {
            GD.PushWarning("PostProcessingUtils.Blit called with null shader.");
            return input;
        }

        ClearViewportChildren(output);

        ShaderMaterial material = new ShaderMaterial
        {
            Shader = shader
        };
        material.SetShaderParameter("input_texture", input);

        configureMaterial?.Invoke(material);

        ColorRect rect = new ColorRect
        {
            Name = "BlitRect",
            Color = Colors.White,
            Position = Vector2.Zero,
            Size = output.Size
        };

        rect.Material = material;
        output.AddChild(rect);

        output.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;

        return output.GetTexture();
    }

    public static void ClearViewportChildren(SubViewport viewport)
    {
        if (viewport == null)
        {
            return;
        }

        foreach (Node child in viewport.GetChildren())
        {
            child.QueueFree();
        }
    }
}