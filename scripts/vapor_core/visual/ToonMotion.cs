using System.Collections.Generic;
using Godot;

public partial class ToonMotion : Node
{

    public int fps = 18;
    public List<Node> ignoreGameobjects;

    float lastUpdateTime = 0f;
    bool forceUpdateThisFrame = false;
    List<Snapshot> snapshots = new List<Snapshot>();
    int i = 0;

    public override void _Ready()
    {
        ProcessPriority = 10;
        CreateTargetList(GetParent<Node2D>());
    }

    void CreateTargetList(Node2D parent)
    {
        if (parent == null) return;
        int childrenCount = parent.GetChildCount();

        for (int i = 0; i < childrenCount; i++)
        {
            Node2D target = parent.GetChild<Node2D>(i);
            // yeah this is slow. too bad, it runs once at startup
            if (!ignoreGameobjects.Contains(target))
            {
                snapshots.Add(new Snapshot(target));
            }
            CreateTargetList(target);
        }
    }

    public override void _Process(double _delta)
    {
        if (forceUpdateThisFrame || GetUnscaledTime() - lastUpdateTime > 1f / fps)
        {
            for (i = 0; i < snapshots.Count; i++)
            {
                snapshots[i].UpdateSelf();
            }
            lastUpdateTime = GetUnscaledTime();
            forceUpdateThisFrame = false;
        }
        else
        {
            for (i = 0; i < snapshots.Count; i++)
            {
                snapshots[i].Maintain();
            }
        }
    }

    private float GetUnscaledTime()
    {
        return Time.GetTicksMsec() * (float)0.001;
    }

    public void ForceUpdate()
    {
        forceUpdateThisFrame = true;
    }

    private class Snapshot
    {
        public Node2D transform;
        public Vector2 position;
        public float rotation;

        public Snapshot(Node2D transform)
        {
            this.transform = transform;
            this.UpdateSelf();
        }

        public void UpdateSelf()
        {
            position = transform.Position;
            rotation = transform.Rotation;
        }

        public void Maintain()
        {
            transform.Position = position;
            transform.Rotation = rotation;
        }
    }
}
