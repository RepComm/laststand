using Godot;

public partial class PhysicsHole : Area3D {
  [Export]
  /**The individual layer in which objects are "cut" physically speaking*/
  public uint cutLayer = 2;

	public override void _Ready() {
    this.BodyEntered += this.OnBodyEntered;
    this.BodyExited += this.OnBodyExited;
	}
  public override void _ExitTree() {
    base._ExitTree();
    this.BodyEntered -= this.OnBodyEntered;
    this.BodyExited -= this.OnBodyExited;
  }

  private void OnBodyEntered (Node3D n) {
    //do you even physics?
    if (n is not PhysicsBody3D) return;
    
    //yeah, we physics
    var b = n as PhysicsBody3D;
    
    //disable collision with the cut layer
    b.CollisionMask &= ~cutLayer;
  }

  private void OnBodyExited (Node3D n) {
    //do you even physics?
    if (n is not PhysicsBody3D) return;
    
    //yeah, we physics
    var b = n as PhysicsBody3D;

    //enable collision with the cut layer
    b.CollisionMask |= this.cutLayer;
  }
}
