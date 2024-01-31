using Godot;
using System;

public partial class Interactable : Node {
  [Export]
  public String hudDisplayName = "Name Here";
  [Export]
  public String hudDisplayDesc = "Description Here";
  [Export]
  public String type = "info";
}
