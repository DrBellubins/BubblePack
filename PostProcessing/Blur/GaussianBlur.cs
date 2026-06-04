using Godot;
using System;

public partial class GaussianBlur : PostProcessModule
{
    public enum BlurDirection
    {
        Horizontal,
        Vertical
    }

    [Export]
    public BlurDirection Direction { get; set; } = BlurDirection.Horizontal;

    [Export]
    public Shader HorizontalShader { get; set; }

    [Export]
    public Shader VerticalShader { get; set; }

    private Shader _activeShader;

    protected override void OnInitialize()
    {
        _activeShader = Direction == BlurDirection.Horizontal
            ? HorizontalShader
            : VerticalShader;
    }

    protected override void OnResize(Vector2I newSize)
    {
        // Nothing special needed here for fixed-kernel blur.
    }

    public override Texture2D DrawTexture(Texture2D input)
    {
        if (input == null)
        {
            GD.PushWarning($"{ModuleName}: input texture is null.");
            return null;
        }

        if (_activeShader == null)
        {
            GD.PushWarning($"{ModuleName}: active blur shader is null.");
            return input;
        }

        return PostProcessingUtils.Blit(
            input,
            this,
            _activeShader,
            material =>
            {
                material.SetShaderParameter("texel_size", new Vector2(
                    1.0f / Mathf.Max(1, RenderSize.X),
                    1.0f / Mathf.Max(1, RenderSize.Y)
                ));
            }
        );
    }
}