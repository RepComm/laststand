using Godot;
using System;

public partial class Interactable : Node {
  [Signal]
  public delegate void OnInteractEventHandler();

  [Export]
  public string hudDisplayName = "Name Here";
  [Export]
  public string hudDisplayDesc = "Description Here";
  [Export]
  public string type = "info";

  public void interact () {
    EmitSignal(SignalName.OnInteract);
  }
}
