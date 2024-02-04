using Godot;
using System;

public partial class MountSlot : Node3D {
	public Rider rider;
  public Mountable root;

	public override void _Ready() {
    this.root = GetParent<Mountable>();
	}
  public bool mount (Rider rider, bool notifyRider = true) {
    if (this.rider != null) return false;
    this.rider = rider;
    if (notifyRider) rider.mounted(this);
    return true;
  }

  public bool unmount (bool notifyRider = true) {
    if (this.rider == null) return false;
    if (notifyRider) this.rider.unmounted(this);
    this.rider = null;
    return true;
  }

  public bool unmount (Rider rider, bool notifyRider = true) {
    if (this.rider != rider) return false;
    return this.unmount(notifyRider);
  }
}
