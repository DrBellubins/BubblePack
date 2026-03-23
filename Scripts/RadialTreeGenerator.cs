using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using BubblePack.Scripts;

public partial class RadialTreeGenerator : Node2D
{
    [Export] public Bubbles Bubbles;
    [Export] public TreeLines Branches;

    [Export] public float MinBubbleRadius { get; set; } = 4.0f;
    [Export] public float MaxBubbleRadius { get; set; } = 40.0f;
    [Export] public float RingRadius { get; set; } = 300.0f;

    private LPU.PackageManagerType packageManagerType;
    private Dictionary<string, PackageData> packages = new(StringComparer.Ordinal);

    public override void _Ready()
    {
        packageManagerType = LPU.DetectPackageManager();

        if (packageManagerType == LPU.PackageManagerType.Pacman)
        {
            packages = Pacman.GetInstalledPackages();
            GD.Print($"Loaded {packages.Count} installed packages from pacman -Qi.");

            BuildVisualization();
        }
        else
        {
            GD.PushWarning($"Package manager {packageManagerType} not implemented yet.");
        }

        /*for (int i = 0; i < Bubbles.Multimesh.InstanceCount; i++)
        {
            var transform = Transform2D.Identity;
            
            var rngX = new Random().NextSingle();
            var rngY = new Random().NextSingle();
            
            transform = transform.Translated(new Vector2(rngX * 10f, rngY * 10f));
            transform = transform.Scaled(new Vector2(1f, 1f));
            
            Bubbles.Multimesh.SetInstanceTransform2D(i, transform);
        }*/
    }

    private void BuildVisualization()
    {
        if (Bubbles == null || Branches == null)
        {
            GD.PushError("RadialTreeGenerator: Bubbles or Branches node reference not set.");
            return;
        }

        Bubbles.Clear();
        Branches.Clear();

        if (packages.Count == 0)
            return;

        // Pick a root: largest installed size.
        PackageData root = packages.Values
            .OrderByDescending(p => p.SizeInMB)
            .First();

        // Position lookup by package name.
        var positions = new Dictionary<string, Vector2>(StringComparer.Ordinal);
        var radii = new Dictionary<string, float>(StringComparer.Ordinal);

        // Precompute radii with robust scaling:
        // Use log scaling so a few huge packages don't flatten everything else.
        float maxSize = MathF.Max(1.0f, packages.Values.Max(p => p.SizeInMB));
        float maxLog = MathF.Log(maxSize + 1.0f);

        float RadiusFor(PackageData p)
        {
            float log = MathF.Log(p.SizeInMB + 1.0f);
            float t = maxLog <= 0.0f ? 0.0f : (log / maxLog);
            return Mathf.Lerp(MinBubbleRadius, MaxBubbleRadius, Mathf.Clamp(t, 0.0f, 1.0f));
        }

        // Root at center
        positions[root.Name] = Vector2.Zero;
        radii[root.Name] = RadiusFor(root);
        Bubbles.AddBubble(Vector2.Zero, radii[root.Name]);

        // Put all other packages on a ring for now (simple & always stable)
        var others = packages.Keys.Where(n => n != root.Name).OrderBy(n => n, StringComparer.Ordinal).ToList();
        int nCount = others.Count;

        for (int i = 0; i < nCount; i++)
        {
            string name = others[i];
            PackageData pkg = packages[name];

            float angle = (Mathf.Tau * i) / Math.Max(1, nCount);
            Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * RingRadius;

            positions[name] = pos;
            radii[name] = RadiusFor(pkg);

            Bubbles.AddBubble(pos, radii[name]);
        }

        // Build dependency line segments.
        // Keep it robust: only draw when both endpoints exist in our dataset.
        /*var segments = new List<TreeLines.Segment>(packages.Count);

        foreach ((string fromName, PackageData fromPkg) in packages)
        {
            if (!positions.TryGetValue(fromName, out Vector2 fromPos))
            {
                continue;
            }

            if (fromPkg.Dependencies == null)
            {
                continue;
            }

            foreach (string dep in fromPkg.Dependencies)
            {
                if (!positions.TryGetValue(dep, out Vector2 toPos))
                {
                    continue;
                }

                segments.Add(new TreeLines.Segment
                {
                    A = fromPos,
                    B = toPos,
                    Color = new Color(1, 1, 1, 0.15f)
                });
            }
        }

        Branches.SetSegments(segments);*/
    }
}