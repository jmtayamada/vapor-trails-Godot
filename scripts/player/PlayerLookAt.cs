using Godot;

public partial class PlayerLookAt : Node
{
	LookAtConstraint2D looker;

	void Start()
	{
		looker = NodeHelper.getComponent<LookAtConstraint2D>(GetParent());
		looker.ProcessMode = ProcessModeEnum.Disabled;
	}

	public void LookAt(Node2D target)
	{
		looker.ProcessMode = ProcessModeEnum.Inherit;
		looker.SetSource(target);
	}

	public void StopLookingAt()
	{
		looker.ProcessMode = ProcessModeEnum.Disabled;
	}
}
