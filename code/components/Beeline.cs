using Sandbox;

namespace TerryNpc;

public class Beeline : EntityComponent<Terry>
{
    public Entity Target { get; set; }
    public float Speed { get; set; } = 40f;
    private Mover MoveComponent { get; set; }

    protected override void OnActivate()
    {
        base.OnActivate();

        MoveComponent = Entity.Components.GetOrCreate<Mover>();
    }

    [GameEvent.Tick.Server]
    public void OnServerTick()
    {
        if (Target == null)
        {
            return;
        }

        var dir = (Target.Position - Entity.Position).Normal * Speed;
        MoveComponent.Forces.Add(dir);
    }
}
