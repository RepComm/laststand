using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CommandPost : Area3D {
  [Export]
  public int team = 0;

  List<Player> capturers = new();
  bool hasForeignCapturer = false;

  double capturing = 0.0;
  [Export]
  double captureRatePerPlayerPerSecond = 0.1;

  public override void _Ready() {
    this.BodyEntered += this._on_area3d_body_entered;
    this.BodyExited += this._on_area3d_body_exited;
	}

  public void _on_area3d_body_entered (Node3D n) {
    if (n is not Player) return;
    var p = n as Player;
    this.capturers.Add(p);
    if (p.team != this.team) {
      this.hasForeignCapturer = true;
    }
  }
  public void _on_area3d_body_exited (Node3D n) {
    if (n is not Player) return;
    var p = n as Player;
    this.capturers.Remove(p);

    //calculate if we have a foreign capturer after the player left capture region
    this.hasForeignCapturer = this.calculateHasForeignCapturer();
  }

  public bool calculateHasForeignCapturer () {
    var result = false;
    foreach (var c in capturers) {
      if (c.team != this.team) {
        result = true;
        break;
      }
    }
    return result;
  }

	public void SetTeam (int team) {
    if (team == this.team) return;
    EmitSignal(SignalName.OnTeamChange, this.team, team);
    this.team = team;
    GD.Print("CP captured by team: " + this.team);
  }
  [Signal]
  public delegate void OnTeamChangeEventHandler(int old, int next);

  public static Dictionary<int,int> CountTeams (List<Player> players) {
    var teamCounts = new Dictionary<int, int>();

    foreach (var player in players) {
      // Update the count for the player's team
      if (!teamCounts.ContainsKey(player.team)) {
        teamCounts[player.team] = 0;
      }
      teamCounts[player.team]++;
    }
    return teamCounts;
  }

  public static int FindMostPresentTeam(List<Player> players) {
    var counts = CountTeams(players);
    // Find the team with the highest count
    var mostCommonTeam = counts.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;

    // Return null if no teams were found
    return mostCommonTeam;
  }

  public override void _Process(double delta) {
    if (!this.hasForeignCapturer) return;

    double amt = this.captureRatePerPlayerPerSecond * delta;

    foreach (var p in capturers) {
      if (p.team == this.team) {
        this.capturing -= amt;
      } else {
        this.capturing += amt;
      }
    }

    if (this.capturing < 0) {
      this.capturing = 0;
      return;
    } else if (this.capturing > 1) {
      var capturingTeam = FindMostPresentTeam(capturers);

      this.SetTeam(capturingTeam);
      this.capturing = 0;
      return;
    } else {
      // GD.Print("CP capture amount: " + this.capturing);
    }
	}
}
