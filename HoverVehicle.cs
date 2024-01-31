using Godot;
using System;

public partial class HoverVehicle : RigidBody3D {
	[Export]
  public Vector3 vIdle = new ();
  [Export]
  public Vector3 vActive = new();
  [Export]
  public Vector3 vBoost = new();
  [Export]
  public float startDelay = 4f;

	public override void _Ready() {

	}

	public override void _Process(double delta) {

	}
}
