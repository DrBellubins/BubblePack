using Godot;
using System;

/// <summary>
/// Base class for a modular post-process stage.
/// Each module is itself a SubViewport render target.
/// </summary>
public abstract partial class PostProcessModule : SubViewport
{
    [Export]
    public bool Enabled { get; set; } = true;

    [Export]
    public string ModuleName { get; set; } = "PostProcessModule";

    protected Vector2I RenderSize;

    public Texture2D OutputTexture
    {
        get
        {
            return GetTexture();
        }
    }

    public virtual void Initialize(Vector2I renderSize)
    {
        RenderSize = renderSize;
        Size = renderSize;
        TransparentBg = true;
        HandleInputLocally = false;
        RenderTargetClearMode = ClearMode.Always;
        RenderTargetUpdateMode = UpdateMode.Always;

        OnInitialize();
    }

    public virtual void Resize(Vector2I renderSize)
    {
        RenderSize = renderSize;
        Size = RenderSize;

        OnResize(renderSize);
    }

    public virtual void Cleanup()
    {
        OnCleanup();
    }

    public abstract Texture2D DrawTexture(Texture2D input);

    protected virtual void OnInitialize()
    {
    }

    protected virtual void OnResize(Vector2I newSize)
    {
    }

    protected virtual void OnCleanup()
    {
    }
}