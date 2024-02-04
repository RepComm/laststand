using Godot;
using System;

public partial class Player : CharacterBody3D {
  Camera3D camera;
  RayCast3D rInteract;

	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

  Vector2 look;

  [Export]
  float lookSensitivity = 0.005f;
  RigidBody3D lastInteractRigidBody;
  Interactable lastInteractable;
  Rider rider;

  public override void _Input(InputEvent @event) {
    if (@event is InputEventMouseButton emb) {
      if (Input.MouseMode != Input.MouseModeEnum.Captured) {
        Input.MouseMode = Input.MouseModeEnum.Captured;
      }
    } else if (@event is InputEventMouseMotion emm) {
      if (Input.MouseMode == Input.MouseModeEnum.Captured) {
        look.X = emm.Relative.X;
        look.Y = emm.Relative.Y;

        if (this.rInteract.IsColliding()) {
          var c = this.rInteract.GetCollider();
          if (this.lastInteractRigidBody == null || this.lastInteractRigidBody != c) {
            if (c is RigidBody3D) {
              var rb = c as RigidBody3D;

              this.lastInteractRigidBody = rb;

              var ch = rb.GetNodeOrNull("Interact");

              if (ch != null && ch is Interactable) {
                var inter = ch as Interactable;
                this.lastInteractable = inter;
                // GD.Print(inter.hudDisplayName);
              } else {
                this.lastInteractable = null;
              }
            }
          }
        } else {
          this.lastInteractRigidBody = null;
          this.lastInteractable = null;
        }
      }
    }
  }

  public override void _Ready() {
    this.camera = GetNode<Camera3D>("Camera3D");
    this.rInteract = this.camera.GetNode<RayCast3D>("RayInteract");

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

    this.updateCameraLook();

    if (Input.IsActionPressed("MouseReleaseCapture")) {
      Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    if (Input.IsActionJustPressed("Interact")) {
      GD.Print("Interact -> Riding: " + this.rider.isRiding() );
      //if we're riding and we press E, unmount
      if (this.rider.isRiding()) {
        this.rider.unmount();
      } else {
        
        if (this.lastInteractable == null) return;

        //otherwise handle different interactions
        if (this.lastInteractable.type == "vehicle") {
          //try and get the mount point
          var m = this.lastInteractable.GetParent().GetNode<Mountable>("Mountable");
          if (m == null) return;

          //try to mount it
          this.rider.mount(m);
        }
      }
    }

  }

  public void onMount (MountSlot m, Rider r) {
    this.disablePhysics();

    m.AddChild(this);

    Position = new();
    GlobalRotation = m.GlobalRotation;
    GD.Print("Player Mount");
  }
  public void onUnmount (MountSlot m, Rider r) {
    GD.Print("Player Unmount");
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

    // this.updateCameraLook();
	}
}
