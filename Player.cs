using Godot;
using System;

public partial class Player : CharacterBody3D {
  Camera3D camera;

	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

  Vector2 look;

  [Export]
  float lookSensitivity = 0.005f;
  Rider rider;

  Interactable iCurrent;

  public void onInteract (InteractRay ray, Interactable prev, Interactable curr) {
    this.iCurrent = curr;
  }

  public override void _Input(InputEvent @event) {
    if (@event is InputEventMouseButton emb) {
      if (Input.MouseMode != Input.MouseModeEnum.Captured) {
        Input.MouseMode = Input.MouseModeEnum.Captured;
      }
    } else if (@event is InputEventMouseMotion emm) {
      if (Input.MouseMode == Input.MouseModeEnum.Captured) {
        look.X = emm.Relative.X;
        look.Y = emm.Relative.Y;
      }
    }
  }

  public override void _Ready() {
    this.camera = GetNode<Camera3D>("Camera3D");
    this.rider = GetNode<Rider>("Rider");
  }
  uint CollisionLayerPrevious = 0;
  uint CollisionMaskPrevious = 0;
  Node ParentPrevious = null;

  public void disablePhysics () {
    ParentPrevious = GetParent();
    ParentPrevious.RemoveChild(this);
    SetPhysicsProcess(false);
    CollisionLayerPrevious = CollisionLayer;
    CollisionLayer = 0;

    CollisionMaskPrevious = CollisionMask;
    CollisionMask = 0;
  }
  public void enablePhysics () {
    var p = GetParent() as Node3D;
    p.RemoveChild(this);

    ParentPrevious.AddChild(this);
    Position = p.GlobalPosition + new Vector3(0,1,0);

    ParentPrevious = p;

    SetPhysicsProcess(true);
    CollisionLayer = CollisionLayerPrevious;
    CollisionMask = CollisionMaskPrevious;
  }
  
  public override void _Process(double delta) {

    // this.updateCameraLook();

    if (Input.IsActionPressed("MouseReleaseCapture")) {
      Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    if (Input.IsActionJustPressed("Interact")) {
      // GD.Print("Interact -> Riding: " + this.rider.isRiding() );
      
      if (this.rider.isRiding()) {
      //Interact while riding == stop riding
        this.rider.unmount();
      } else {
        
        //Interact while not riding + looking at vehicle == start riding

        //check if we're trying to interact with a vehicle
        if (this.iCurrent == null) return;
        //otherwise handle different interactions
        if (this.iCurrent.type == "vehicle") {
          //try and get the mount point
          var m = this.iCurrent.GetParent().GetNode("Mountable");
          if (m == null || m is not Mountable) return;

          //try to mount it
          this.rider.mount(m as Mountable);
        }

        //notify interactable so it can send signals if applicable
        this.iCurrent.interact();
      }
    }

  }

  public void onMount (MountSlot m, Rider r) {
    this.disablePhysics();

    m.AddChild(this);

    Position = new();
    GlobalRotation = m.GlobalRotation;
    this.camera.Rotation = new();
    // GD.Print("Player Mount");
  }
  public void onUnmount (MountSlot m, Rider r) {
    // GD.Print("Player Unmount");
    this.enablePhysics();
  }

  public void updateCameraLook () {
    this.camera.RotateX(-this.look.Y * this.lookSensitivity);
    this.RotateY(-this.look.X * this.lookSensitivity);

    //consume look movement
    this.look.X = 0;
    this.look.Y = 0;
  }

	public override void _PhysicsProcess(double delta) {
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("strafe_left", "strafe_right", "fwd", "bwd");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero) {
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		} else {
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();

    this.updateCameraLook();
	}
}
