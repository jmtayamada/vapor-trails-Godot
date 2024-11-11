
# Basic Unity to Godot conversion guide

Replace all "using Unity.___;" lines with "using Godot;"

"public class ClassName : MonoBehaviour" -> "public partial class ClassName : Node"
Rather than attaching scripts to nodes, create a child of the type node, and attach the script to that child node

Any variables that would be visible in the editor must now have "[Export]" in the previous line
ex.
"public bool enabled;" ->  "[Export]
                            public bool enabled"

"void Start()" -> "public override void _Ready()"

"void Update()" -> "public override void _Process(double delta)"

"void LateUpdate()" -> "public void LateUpdate(double delta)" also include a "CallDefferred("LateUpdate", delta);" in the "_Ready()" function

OnEnable and OnDisable functions will need to be manually implemented, see RotateToVelocity.cs file for an example of this.
