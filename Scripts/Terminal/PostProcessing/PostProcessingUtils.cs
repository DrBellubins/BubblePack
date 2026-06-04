using Godot;
using System;

/// <summary>
/// Utility helpers for modular fullscreen post-processing passes.
/// </summary>
public static class PostProcessingUtils
{
    /// <summary>
    /// Creates a SubViewport configured to be used as a render target.
    /// </summary>
    public static SubViewport CreateRenderViewport(string name, Vector2I size)
    {
        SubViewport viewport = new SubViewport
        {
            Name = name,
            Size = size,
            TransparentBg = true,
            HandleInputLocally = false,
            RenderTargetUpdateMode = SubViewport.UpdateMode.Once,
            RenderTargetClearMode = SubViewport.ClearMode.Always
        };

        return viewport;
    }

    /// <summary>
    /// Blits an input texture into a target viewport using a shader material.
    /// This is the Godot equivalent of a simple fullscreen shader pass.
    /// </summary>
    public static Texture2D Blit(Texture2D input, SubViewport output, Shader shader)
    {
        return Blit(input, output, shader, null);
    }

    /// <summary>
    /// Blits an input texture into a target viewport using a shader material,
    /// optionally configuring extra shader parameters before render.
    /// </summary>
    public static Texture2D Blit(
        Texture2D input,
        SubViewport output,
        Shader shader,
        Action<ShaderMaterial> configureMaterial
    )
    {
        if (input == null)
        {
            GD.PushWarning("TerminalPostUtils.Blit called with null input texture.");
            return null;
        }

        if (output == null)
        {
            GD.PushWarning("TerminalPostUtils.Blit called with null output viewport.");
            return null;
        }

        if (shader == null)
        {
            GD.PushWarning("TerminalPostUtils.Blit called with null shader.");
            return null;
        }

        ClearViewportChildren(output);

        ShaderMaterial material = new ShaderMaterial();
        material.Shader = shader;
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

        output.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;

        return output.GetTexture();
    }

    /// <summary>
    /// Copies an input texture directly into a target viewport without a shader.
    /// Useful as a pass-through stage or debugging helper.
    /// </summary>
    public static Texture2D BlitCopy(Texture2D input, SubViewport output)
    {
        if (input == null)
        {
            GD.PushWarning("TerminalPostUtils.BlitCopy called with null input texture.");
            return null;
        }

        if (output == null)
        {
            GD.PushWarning("TerminalPostUtils.BlitCopy called with null output viewport.");
            return null;
        }

        ClearViewportChildren(output);

        TextureRect rect = new TextureRect
        {
            Name = "CopyRect",
            Texture = input,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            Position = Vector2.Zero,
            Size = output.Size
        };

        output.AddChild(rect);
        output.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;

        return output.GetTexture();
    }

    /// <summary>
    /// Clears all child draw nodes from a viewport before drawing the next pass.
    /// </summary>
    public static void ClearViewportChildren(SubViewport viewport)
    {
        if (viewport == null)
            return;

        foreach (Node child in viewport.GetChildren())
            child.QueueFree();
    }
}