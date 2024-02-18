using Godot;
using System;

public partial class ToggleAnim : Node {
  AnimationPlayer ap;
  [Export]
  public bool isClosed = true;

	public override void _Ready() {
    this.ap = GetNode<AnimationPlayer>("AnimationPlayer");
	}

  private void ToggleAnimSignalEndpoint (string open_anim_name, string close_anim_name) {
    // GD.Print("Toggle");
    this.isClosed = !this.isClosed;

    if (this.isClosed) {
      this.ap.Play(close_anim_name);//"normal/hoth_hanger_close");
    } else {
      this.ap.Play(open_anim_name);//"normal/hoth_hanger_open");
    }
  }

	// public override void _Process(double delta) {
	// }
}
