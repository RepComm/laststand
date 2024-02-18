using Godot;
using System;

public partial class LabelInteract : Label {
  private void OnInteractSetText (InteractRay ray, Interactable prev, Interactable curr) {
    if (curr == null) {
      Text = "";
    } else {
      Text = curr.hudDisplayName;
    }
  }

}
