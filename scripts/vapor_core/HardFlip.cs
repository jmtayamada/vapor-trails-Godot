using Godot;

public partial class HardFlip : Node
{
	Vector2 s;

	public override void _Ready()
	{
		ProcessPriority = 10;
	}

	public override void _Process(double _delta)
	{
		s = GetParent<Node2D>().Scale;
		if (s.X > 0 && s.X < 1)
		{
			s.X = 1;
		}
		else if (s.X < 0 && s.X > -1)
		{
			s.X = -1;
		}
		GetParent<Node2D>().Scale = s;
	}
}
