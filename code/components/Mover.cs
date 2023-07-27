using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace TerryNpc;

public class Mover : EntityComponent<Terry>, ISingletonComponent
{
    [ConVar.Server("terrynpc.vis.halted")] public static bool VisualizeHalted { get; set; } = false;
    public List<Vector3> Forces { get; private set; } = new();
    public float StepSize { get; set; } = 16.0f;
    public bool IsHalted { get; set; } = false;
    public Vector3 LastMoveDirection { get; set; }

    [GameEvent.Tick.Server]
    public void OnServerTick()
    {
        if (!Forces.Any())
        {
            return;
        }
        var moveDir = Vector3.Zero;
        foreach (var force in Forces)
        {
            moveDir += force;
        }
        PerformMove(moveDir);
        LastMoveDirection = moveDir;
        Forces.Clear();
    }

    private void PerformMove(Vector3 velocity)
    {
        MoveHelper helper = new MoveHelper(Entity.Position, velocity);
        helper.Trace = helper.Trace
            .Size(Entity.Hull)
            .WithAnyTags(Entity.ImpassibleTags.ToArray())
            .Ignore(Entity);
        if (helper.TryMoveWithStep(Time.Delta, StepSize) > 0)
        {
            Entity.Position = helper.Position;
            Entity.Velocity = helper.Velocity;
        }
        else if (!velocity.IsNearZeroLength)
        {
            IsHalted = true;
            if (VisualizeHalted)
            {
                DebugOverlay.Text($"[Halted Entity ({Entity.Name})]", Entity.Position, Color.Red);
                DebugOverlay.Box(Entity, Color.Red);
            }
            return;
        }

        IsHalted = false;
    }
}
