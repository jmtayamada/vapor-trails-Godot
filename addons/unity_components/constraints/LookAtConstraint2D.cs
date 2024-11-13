using System;
using Godot;

[Tool]
public partial class LookAtConstraint2D : Node
{
    [Export(PropertyHint.Range, "0, 1")]
    float Weight = 1;

    [Export]
    Node2D Source;

    [ExportGroup("Constraint Settings")]
    private bool _var;
    [Export]
    bool Lock
    {
        get => _var;
        set => UpdateLock(value);
    }

    [Export]
    float RotationOffset;

    public override void _Process(double delta)
    {
        CallDeferred("LateUpdate", delta);
    }

    public void LateUpdate(double _delta)
    {
        if (Lock && Source != null)
        {
            Node2D parent = GetParent<Node2D>();
            parent.LookAt(Source.GlobalPosition);
            parent.Rotation -= RotationOffset;
        }
    }

    private void UpdateLock(bool value)
    {
        _var = value;
        if (value == true)
        {
            Node2D parent = GetParent<Node2D>();
            RotationOffset = Vector2.Right.Rotated(parent.Rotation).AngleTo(Source.GlobalPosition - parent.GlobalPosition);
        }
    }

    public void SetSource(Node2D source)
    {
        Source = source;
    }
}
