using Godot;
using System;

public partial class HothHanger : Node {
  AnimationPlayer ap;
  bool isClosed = true;

	public override void _Ready() {
    this.ap = GetNode<AnimationPlayer>("AnimationPlayer");
	}

  public void toggle_doors () {
    // GD.Print("Toggle");
    this.isClosed = !this.isClosed;

    if (this.isClosed) {
      this.ap.Play("close");
    } else {
      this.ap.Play("open");
    }
  }

	// public override void _Process(double delta) {
	// }
}
