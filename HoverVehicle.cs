using Godot;
using System;

public partial class HoverVehicle : RigidBody3D {
  public static void Vector3Set (ref Vector3 v, Vector3 from) {
    v.X = from.X;
    v.Y = from.Y;
    v.Z = from.Z;
  }
  public static float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

  [Export]
  public Vector3 vTakeoff = new(0f, 2f + gravity , -4f);
	[Export]
  public Vector3 vHoverIdle = new (0f, gravity, -5f);
  [Export]
  public Vector3 vHoverActive = new(0f, gravity, -10f);
  [Export]
  public Vector3 vHoverBoost = new(0f, 2f + gravity, -20f);
  public Vector3 thrust = new ();

  [Export]
  public float startDelay = 4f;
  [Export]
  public Player driver;
  [Export]
  public float hoverAltMin = 5f;
  [Export]
  public float hoverAltMax = 20f;
  
  public float altitude {
    get {
      return GlobalPosition.Y - this.altimeter.GetCollisionPoint().Y;
    }
  }

  RayCast3D altimeter;

  public enum HoverMode {
    Landed,
    TakingOff,
    HoverIdle,
    HoverActive,
    HoverBoost,
    Landing
  }

  [Export]
  HoverMode hoverMode = HoverMode.Landed;

	public override void _Ready() {
    this.altimeter = GetNode<RayCast3D>("altimeter");
    this.SetMode(HoverMode.Landed);
	}

  public void SetMode (HoverMode m) {
    this.hoverMode = m;
  }

  // public override void _IntegrateForces(PhysicsDirectBodyState3D state) {
  //   state.ApplyCentralForce(this.thrust);
  //   base._IntegrateForces(state);
  // }

	public override void _PhysicsProcess(double delta) {
    //If landed, nothing to do
    if (this.hoverMode == HoverMode.Landed) {
      this.thrust.X = 0;
      this.thrust.Y = 0;
      this.thrust.Z = 0;
    }

    if (this.hoverAltMin != -this.altimeter.TargetPosition.Y) {
      // GD.Print("set min alt");
      this.altimeter.TargetPosition = new (0, -this.hoverAltMin, 0);
    }

    //if no driver but we're not landing (and not landed), land please
    if (this.driver == null && this.hoverMode != HoverMode.Landing) {
      this.SetMode(HoverMode.Landing);
    }

    if (this.hoverMode == HoverMode.Landing) {
      //No forces applied
      this.thrust.X = 0;
      this.thrust.Y = 0;
      this.thrust.Z = 0;
    } else if (this.hoverMode == HoverMode.TakingOff) {
      if (this.altitude > this.hoverAltMin) {
        this.SetMode(HoverMode.HoverIdle);
      } else {
        Vector3Set(ref this.thrust, this.vTakeoff);
      }
    } else if (this.hoverMode == HoverMode.HoverIdle) {
      Vector3Set(ref this.thrust, this.vHoverIdle);
    } else if (this.hoverMode == HoverMode.HoverActive) {
      Vector3Set(ref this.thrust, this.vHoverActive);
    } else if (this.hoverMode == HoverMode.HoverBoost) {
      Vector3Set(ref this.thrust, this.vHoverBoost);
    }

    // GD.Print(this.hoverMode);
    ApplyCentralForce(this.thrust * this.Basis.Transposed());

	}

}
