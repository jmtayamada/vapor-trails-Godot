using System;
using Godot;

public partial class RotateToVelocity : Node
{
    [Export]
    public bool enabled;
    private bool was_enabled;
    [Export]
    public RigidBody2D rb2d;

    // this is the default if entities are normally facing up
    [Export]
    public double offset = -90;

    public override void _Ready()
    {
        if (rb2d == null)
        {
            GD.PrintErr("RigidBody2D not found!");
        }
    }

    public override void _Process(double delta)
    {
        if (!enabled) { return; }
        CallDeferred("LateUpdate", delta);
    }

    public void LateUpdate(double _delta)
    {
        if (rb2d == null) { return; }

        if (rb2d.Sleeping)
        {
            return;
        }
        SetAngleForVelocity(rb2d.LinearVelocity);
    }

    public void SetAngleForVelocity(Vector2 v)
    {
        GetParent<Node2D>().RotationDegrees = (float)((Math.Atan2(v.Y, v.X) * 180 / Math.PI) + offset);
    }

    public void Enabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled) { GetParent<Node2D>().RotationDegrees = 0; }
    }
}
