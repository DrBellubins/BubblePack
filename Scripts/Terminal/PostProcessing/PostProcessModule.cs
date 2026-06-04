using Godot;
using System;

/// <summary>
/// Base class for a modular post-process stage.
/// Each module owns an output SubViewport that acts as its render target.
/// </summary>
public abstract partial class PostProcessModule : Node
{
    [Export]
    public bool Enabled { get; set; } = true;

    [Export]
    public string ModuleName { get; set; } = "PostProcessModule";

    protected Vector2I RenderSize;
    protected SubViewport OutputViewport;

    public Texture2D OutputTexture
    {
        get
        {
            if (OutputViewport == null)
                return null;

            return OutputViewport.GetTexture();
        }
    }

    public virtual void Initialize(Vector2I renderSize)
    {
        RenderSize = renderSize;

        if (OutputViewport == null)
        {
            OutputViewport = PostProcessingUtils.CreateRenderViewport(
                $"{ModuleName}_Output",
                RenderSize
            );

            AddChild(OutputViewport);
        }
        else
            OutputViewport.Size = RenderSize;

        OnInitialize();
    }

    public virtual void Resize(Vector2I renderSize)
    {
        RenderSize = renderSize;

        if (OutputViewport != null)
            OutputViewport.Size = RenderSize;

        OnResize(renderSize);
    }

    public virtual void Cleanup()
    {
        OnCleanup();

        if (IsInstanceValid(OutputViewport))
            OutputViewport.QueueFree();

        OutputViewport = null;
    }

    /// <summary>
    /// Performs this pass and returns the output texture for the next stage.
    /// </summary>
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