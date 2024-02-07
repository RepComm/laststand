using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class Mountable : Node3D {

  [Signal]
  public delegate void OnMountEventHandler(MountSlot slot, Rider rider);
  [Signal]
  public delegate void OnUnmountEventHandler(MountSlot slot, Rider rider);

  MountSlot[] slots;

  private int riderCount = 0;
  public bool hasAnyRiders {
    get { return this.riderCount > 0; }
  }

  private MountSlot[] getMountSlots () {
    var children = GetChildren();
    var results = new Array<MountSlot>();
    
    foreach (var child in children) {
      if (child is MountSlot) {
        results.Add(child as MountSlot);
      }
    }

    return results.ToArray();
  }

	public override void _Ready() {
    this.slots = getMountSlots();
	}

  public delegate bool RiderToBoolCb(Rider rider);

  /**Returns int count of riders removed using the provided callback
   * The callback should return true when it decides to remove the rider passed to it
   * returning false unconditionally will remove no riders
   */
  public int unmountForeach (RiderToBoolCb cb) {
    int count = 0;
    foreach (MountSlot slot in this.slots) {
      if (slot.rider != null && cb(slot.rider)) {
        slot.unmount();
        count ++;
      }
    }
    return count;
  }
  /**Unmounts rider, returns true on success, false if rider was not riding this mountable*/
  public bool unmount (Rider rider) {
    foreach (MountSlot slot in this.slots) {
      if (this.unmountSlot(rider, slot)) return true;
    }
    return false;
  }
  /**Returns true on success, false if rider was not mounted in the first place
   */
  public bool unmountSlot (Rider rider, MountSlot slot) { 
    if (slot.unmount(rider)) {
      EmitSignal(SignalName.OnUnmount, slot, rider);
      this.riderCount --;
      if (this.riderCount < 0) this.riderCount = 0;
      return true;
    }
    return false;
  }
  /**Returns int slotIndex of mounted slot or
   * -1 if not successful
   */
  public int mount (Rider rider) {
    int slot = this.getEmptySlotIndex();
    if (this.mount(rider, slot)) return slot;
    return -1;
  }
  /**Returns true on success
   * returns false on failure due to:
   * - slot already has a rider
   * - invalid slotIndex
  */
  public bool mount (Rider rider, int slotIndex) {
    MountSlot slot = this.getMountSlot(slotIndex);
    if (slot == null) return false;
    if (slot.mount(rider)) {
      EmitSignal(SignalName.OnMount, slot, rider);
      this.riderCount ++;
      return true;
    }
    return false;
  }
  public bool isMountSlotIndexValid (int slotIndex) {
    return slotIndex >= 0 && slotIndex >= this.slots.Length;
  }

  /**Returns MountSlot or null*/
  public MountSlot getMountSlot (int slotIndex) {
    if (isMountSlotIndexValid(slotIndex)) return null;
    return this.slots[slotIndex];
  }
  /**Returns valid index, or -1*/
  public int getEmptySlotIndex () {
    int i=0;
    foreach (MountSlot slot in this.slots) {
      if (slot.rider == null) {
        return i;
      }
      i++;
    }
    return -1;
  }
  public MountSlot getEmptySlot () {
    foreach (MountSlot slot in this.slots) {
      if (slot.rider == null) {
        return slot;
      }
    }
    return null;
  }
}
