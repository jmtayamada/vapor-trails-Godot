using System;
using Godot;

public partial class RotateToVelocity : Node
{
    [Export]
    public RigidBody2D rb2d;

    // this is the default if entities are normally facing up
    [Export]
    public double offset = -90;

    public override void _Ready()
    {
        ProcessPriority = 10;
        GD.Print(ProcessPriority);
        if (rb2d == null)
        {
            GD.PrintErr("RigidBody2D not found!");
        }
    }

    public override void _Process(double _delta)
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
        if (enabled)
        {
            ProcessMode = ProcessModeEnum.Inherit;
        }
        else
        {
            GetParent<Node2D>().RotationDegrees = 0;
            ProcessMode = ProcessModeEnum.Disabled;
        }
    }
}
