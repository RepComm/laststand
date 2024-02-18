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
  public Mountable mountable;

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
  public bool isDriving {
    get {
      return this.hoverMode != HoverMode.Landed && this.hoverMode != HoverMode.Landing;
    }
  }

	public override void _Ready() {
    this.altimeter = GetNode<RayCast3D>("altimeter");
    this.mountable = GetNode<Mountable>("Mountable");
    GD.Print(this.mountable);
    this.SetMode(HoverMode.Landed);
    this.targetRotation = this.Rotation;
    this.displayRotation = this.Rotation;
	}

  public void SetMode (HoverMode m) {
    GD.Print("Mode was: " + this.hoverMode + ", now is: " + m);
    this.hoverMode = m;
  }
  Vector2 look = new();
  //The value displayRotation lerps to
  Vector3 targetRotation = new();
  //The value used for rendering
  Vector3 displayRotation = new();
  float lookSensitivity = 0.0025f;

  public override void _Input(InputEvent @event) {
    if (!isDriving) return;

    if (@event is InputEventMouseMotion emm) {
      if (Input.MouseMode == Input.MouseModeEnum.Captured) {
        look.X = emm.Relative.X;
        look.Y = emm.Relative.Y;
      }
    }
  }

  public override void _Process(double delta) {
    if (this.mountable.hasAnyRiders) {
      if (Input.IsActionJustPressed("engine_toggle")) {
        if (this.isDriving) {
          this.SetMode(HoverMode.Landing);
        } else {
          this.SetMode(HoverMode.TakingOff);
        }
      }
    }
  }

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
    if (!this.mountable.hasAnyRiders && this.hoverMode != HoverMode.Landing && this.hoverMode != HoverMode.Landed) {
      this.SetMode(HoverMode.Landing);
    }

    if (this.hoverMode == HoverMode.Landing) {
      //No forces applied
      this.thrust.X = 0;
      this.thrust.Y = 0;
      this.thrust.Z = 0;

      if (this.altitude <= this.hoverAltMin) {
        this.SetMode(HoverMode.Landed);
      }

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

    ApplyCentralForce(this.thrust * this.Basis.Transposed());
    
    this.targetRotation.X += -this.look.Y * lookSensitivity;
    this.targetRotation.Y += -this.look.X * lookSensitivity;

    this.displayRotation.Z = 0;

    //why is this if statement actually necessary...
    if (this.displayRotation != this.targetRotation) {
      this.displayRotation = this.displayRotation.Slerp(
        this.targetRotation,
        0.025f
      );
    }

    this.displayRotation.Z = (
      this.targetRotation.Y - this.displayRotation.Y
    ) / 10f;

    this.Rotation = this.displayRotation;//this.targetRotation;
    

    //consume mouse movement for this frame
    this.look.X = 0;
    this.look.Y = 0;
	}

}
