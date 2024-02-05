using Godot;

public partial class InteractRay : RayCast3D {
  [Signal]
  public delegate void OnInteractEventHandler(InteractRay ray, Interactable prev, Interactable curr);

  RigidBody3D rbPrev = null;
  RigidBody3D rbCurr = null;
  Interactable iPrev = null;
  Interactable iCurr = null;

  private bool tryUpdate (Interactable iC) {
    rbPrev = rbCurr;
    iPrev = iCurr;

    iCurr = iC;

    if (iPrev == iCurr) return false;
    EmitSignal(SignalName.OnInteract, this, iPrev, iCurr);
    return true;
  }

	public override void _Process(double delta) {
    rbPrev = rbCurr;
    iPrev = iCurr;

    if (!IsColliding()) {
      tryUpdate(null);
      return;
    }

    var c = GetCollider();

    if (c == null || c is not RigidBody3D) {
      tryUpdate(null);
      return;
    }

    var rb = c as RigidBody3D;
    var i = rb.GetNodeOrNull("Interact");

    if (i == null || i is not Interactable) {
      tryUpdate(null);
      return;
    }

    rbCurr = rb;

    tryUpdate(i as Interactable);

	}
}
