#if TOOLS
using Godot;

[Tool]
public partial class UnityComponents : EditorPlugin
{
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		// Add the new type with a name, a parent type, a script and an icon.
		var script = GD.Load<Script>("res://addons/unity_components/constraints/RotationConstraint2D.cs");
		var texture = GD.Load<Texture2D>("res://addons/unity_components/unity_logo.png");
		AddCustomType("RotationConstraint2D", "Node", script, texture);

		script = GD.Load<Script>("res://addons/unity_components/constraints/LookAtConstraint2D.cs");
		AddCustomType("LookAtConstraint2D", "Node", script, texture);
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		// Always remember to remove it from the engine when deactivated.
		RemoveCustomType("RotationConstraint2D");
		RemoveCustomType("LookAtConstraint2D");
	}
}
#endif
