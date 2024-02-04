using Godot;
using System;

public partial class Rider : Node {
  MountSlot slot;
  public bool isRiding () {
    return this.slot != null;
  }
  public MountSlot getMountSlot () {
    return this.slot;
  }
  public Mountable getMountable () {
    if (this.slot == null) return null;
    return this.slot.root;
  }

  public bool unmount () {
    var m = this.getMountable();
    if (m == null) return false;
    return m.unmountSlot(this, this.slot);
  }
  public int mount (Mountable m) {
    return m.mount(this);
  }

  [Signal]
  public delegate void OnMountEventHandler(MountSlot slot, Rider rider);
  [Signal]
  public delegate void OnUnmountEventHandler(MountSlot slot, Rider rider);

  public void unmounted (MountSlot slot) {
    this.slot = null;
    EmitSignal(SignalName.OnUnmount, slot, this);
  }
  public void mounted (MountSlot slot) {
    this.slot = slot;
    EmitSignal(SignalName.OnMount, slot, this);
  }
}
