using Godot;
using System;

public partial class Interactable : Node {
  [Export]
  public string hudDisplayName = "Name Here";
  [Export]
  public string hudDisplayDesc = "Description Here";
  [Export]
  public string type = "info";
}
