using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Runs a modular multi-pass post-processing pipeline using SubViewports as render targets.
/// Attach this to a CanvasLayer or Node that owns the post-process chain.
/// </summary>
public partial class TerminalPostProcess : Node
{
    [Export]
    public Vector2I RenderSize { get; set; } = new Vector2I(1280, 720);

    [Export]
    public bool AutoInitializeOnReady { get; set; } = true;

    [Export]
    public bool ProcessEveryFrame { get; set; } = true;

    /// <summary>
    /// Optional source texture for the first pass.
    /// If null, you can provide one manually when calling Execute().
    /// </summary>
    [Export]
    public Texture2D SourceTexture { get; set; }

    private readonly List<PostProcessModule> _modules = new();

    private Texture2D _finalOutput;

    public IReadOnlyList<PostProcessModule> Modules => _modules;
    public Texture2D FinalOutput => _finalOutput;

    public override void _Ready()
    {
        if (AutoInitializeOnReady)
        {
            InitializeModules();
        }
    }

    public override void _Process(double delta)
    {
        if (!ProcessEveryFrame)
            return;

        if (SourceTexture == null)
            return;

        Execute(SourceTexture);
    }

    public void AddModule(PostProcessModule module)
    {
        if (module == null)
        {
            GD.PushWarning("TerminalPostProcess.AddModule called with null module.");
            return;
        }

        if (_modules.Contains(module))
            return;

        _modules.Add(module);

        if (IsInsideTree())
        {
            if (module.GetParent() != this)
                AddChild(module);

            module.Initialize(RenderSize);
        }
    }

    public bool RemoveModule(PostProcessModule module)
    {
        if (module == null)
            return false;

        bool removed = _modules.Remove(module);

        if (removed)
            module.Cleanup();

        return removed;
    }

    public void ClearModules()
    {
        foreach (PostProcessModule module in _modules)
            module?.Cleanup();

        _modules.Clear();
        _finalOutput = null;
    }

    public void InitializeModules()
    {
        foreach (PostProcessModule module in _modules)
        {
            if (module == null)
                continue;

            if (module.GetParent() != this)
                AddChild(module);

            module.Initialize(RenderSize);
        }
    }

    public void ResizePipeline(Vector2I newSize)
    {
        RenderSize = newSize;

        foreach (PostProcessModule module in _modules)
            module?.Resize(newSize);
    }

    public Texture2D Execute(Texture2D input)
    {
        if (input == null)
        {
            GD.PushWarning("TerminalPostProcess.Execute called with null input.");
            return null;
        }

        Texture2D current = input;

        foreach (PostProcessModule module in _modules)
        {
            if (module == null)
                continue;

            if (!module.Enabled)
                continue;

            current = module.DrawTexture(current);

            if (current == null)
            {
                GD.PushWarning($"Module '{module.Name}' returned null output texture.");
                break;
            }
        }

        _finalOutput = current;
        return _finalOutput;
    }
}