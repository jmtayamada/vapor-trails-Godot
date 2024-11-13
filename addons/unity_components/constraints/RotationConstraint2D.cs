using Godot;

[Tool]
public partial class RotationConstraint2D : Node
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
			GetParent<Node2D>().GlobalRotation = (1 - Weight) * GetParent<Node2D>().GlobalRotation + Weight * (Source.GlobalRotation - RotationOffset);
		}
	}

	private void UpdateLock(bool value)
	{
		_var = value;
		if (value == true)
		{
			RotationOffset = Source.GlobalRotation - GetParent<Node2D>().GlobalRotation;
		}
	}

	public void SetSource(Node2D source)
	{
		Source = source;
	}
}
