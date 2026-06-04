using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Runs a modular multi-pass post-processing pipeline.
/// The source comes from a capture SubViewport that is resized to a fixed-height render size.
/// </summary>
public partial class TerminalPostProcess : Node
{
    [Export]
    public int TargetRenderHeight { get; set; } = 480;

    [Export]
    public bool AutoInitializeOnReady { get; set; } = true;

    [Export]
    public bool ProcessEveryFrame { get; set; } = true;

    [Export]
    public SubViewport CaptureViewport { get; set; }

    [Export]
    public TextureRect DebugOutput { get; set; }

    private readonly List<PostProcessModule> _modules = new();

    private Texture2D _finalOutput;
    private Vector2I _renderSize = new Vector2I(854, 480);

    public IReadOnlyList<PostProcessModule> Modules
    {
        get
        {
            return _modules;
        }
    }

    public Texture2D FinalOutput
    {
        get
        {
            return _finalOutput;
        }
    }

    public Vector2I RenderSize
    {
        get
        {
            return _renderSize;
        }
    }

    public override void _Ready()
    {
        UpdateRenderSizeFromWindow();

        AddModule(GetNode<PostProcessModule>("BlurH"));
        AddModule(GetNode<PostProcessModule>("BlurV"));

        if (AutoInitializeOnReady)
            InitializeModules();
    }

    public override void _Process(double delta)
    {
        if (!ProcessEveryFrame)
        {
            return;
        }

        Vector2I expected = CalculateRenderSizeFromAppAspect();
        if (expected != _renderSize)
        {
            ResizePipeline(expected);
        }

        Texture2D result = Execute();

        if (DebugOutput != null && result != null)
        {
            DebugOutput.Texture = result;
        }
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

        if (module.GetParent() != this)
            AddChild(module);

        if (IsInsideTree())
            module.Initialize(_renderSize);
    }

    public void InitializeModules()
    {
        if (CaptureViewport != null)
        {
            CaptureViewport.Size = _renderSize;
            CaptureViewport.TransparentBg = false;
            CaptureViewport.HandleInputLocally = false;
            CaptureViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
            CaptureViewport.RenderTargetClearMode = SubViewport.ClearMode.Always;
        }

        foreach (PostProcessModule module in _modules)
        {
            if (module == null)
            {
                continue;
            }

            if (module.GetParent() != this)
            {
                AddChild(module);
            }

            module.Initialize(_renderSize);
        }
    }

    public void ResizePipeline(Vector2I newSize)
    {
        _renderSize = newSize;

        if (CaptureViewport != null)
        {
            CaptureViewport.Size = _renderSize;
        }

        foreach (PostProcessModule module in _modules)
        {
            module?.Resize(_renderSize);
        }

        GD.Print($"TerminalPostProcess resized to {_renderSize}.");
    }

    public void UpdateRenderSizeFromWindow()
    {
        ResizePipeline(CalculateRenderSizeFromAppAspect());
    }

    public Vector2I CalculateRenderSizeFromAppAspect()
    {
        Vector2 windowSize = GetViewport().GetVisibleRect().Size;

        int safeHeight = Mathf.Max(1, TargetRenderHeight);
        float aspect = windowSize.Y <= 0 ? (16.0f / 9.0f) : ((float)windowSize.X / windowSize.Y);
        int width = Mathf.Max(1, Mathf.RoundToInt(safeHeight * aspect));

        return new Vector2I(width, safeHeight);
    }

    public Texture2D Execute()
    {
        if (CaptureViewport == null)
        {
            GD.PushWarning("TerminalPostProcess.Execute called with null CaptureViewport.");
            return null;
        }

        Texture2D current = CaptureViewport.GetTexture();

        if (current == null)
        {
            GD.PushWarning("TerminalPostProcess.Execute: CaptureViewport texture is null.");
            return null;
        }

        foreach (PostProcessModule module in _modules)
        {
            if (module == null)
            {
                continue;
            }

            if (!module.Enabled)
            {
                continue;
            }

            current = module.DrawTexture(current);

            if (current == null)
            {
                GD.PushWarning($"Module '{module.Name}' returned null output texture.");
                return null;
            }
        }

        _finalOutput = current;
        return _finalOutput;
    }
}