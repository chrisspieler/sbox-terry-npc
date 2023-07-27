using Sandbox;

namespace TerryNpc;

public partial class Gravity : EntityComponent<Terry>
{
    [Net] public float GravityAmount { get; set; } = 200f;
    [Net] public float GroundAngle { get; set; } = 40f;
    public bool IsGrounded => Entity.GroundEntity?.IsValid() == true;
    private Mover MoveComponent { get; set; }

    protected override void OnActivate()
    {
        base.OnActivate();

        MoveComponent = Entity.Components.GetOrCreate<Mover>(true);
    }

    [GameEvent.Tick.Server]
    public void OnServerTick()
    {
        var groundEntity = CheckForGround();

        Entity.GroundEntity = groundEntity;

        var gravity = Vector3.Zero.WithZ(IsGrounded ? 0f : -GravityAmount);
        MoveComponent.Forces.Add(gravity);
    }

    Entity CheckForGround()
    {
        if (Entity.Velocity.z > 300f)
            return null;

        var distance = Entity.BodyHeight / 2f;
        var trace = Entity.TraceBBox(Entity.Position, Entity.Position - Vector3.Down * distance);

        if (!trace.Hit)
            return null;

        if (trace.Normal.Angle(Vector3.Up) > GroundAngle)
            return null;

        return trace.Entity;
    }
}