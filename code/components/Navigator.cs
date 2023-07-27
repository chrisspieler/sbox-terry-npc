using Sandbox;
using System.Linq;

namespace TerryNpc;

public partial class Navigator : EntityComponent<Terry>
{
    [ConVar.Server("terrynpc.vis.path")] 
    public static bool VisualizePath { get; set; } = false;
    public IEntity Target { get; set; }
    public float MovementSpeed { get; set; } = 60f;
    public float PathRecalculationInterval { get; set; } = 1f;
    public float ArriveDistance { get; set; } = 40f;
    protected TimeSince TimeSinceGeneratedPath { get; set; } = 0;
    protected Vector3[] Path { get; set; }
    protected int CurrentPathSegment { get; set; }
    protected Mover MoveComponent { get; set; }

    protected override void OnActivate()
    {
        base.OnActivate();

        MoveComponent = Entity.Components.GetOrCreate<Mover>();
    }

    [GameEvent.Tick.Server]
    public void OnServerTick()
    {
        if (Target == null)
            return;

        if (VisualizePath)
            DebugDrawPath();

        var distanceToTarget = Entity.Position.Distance(Target.Position);
        var pathRecalculationInterval = MathX.Remap(distanceToTarget, 50f, 4000f, 0.25f, 5f);

        if (TimeSinceGeneratedPath > pathRecalculationInterval)
        {
            GeneratePath(Target.Position);
        }

        TraversePath();

        if (Target.Position.Distance(Entity.Position) <= ArriveDistance)
        {
            Target = null;
        }
    }

    protected void GeneratePath(Vector3 target)
    {
        TimeSinceGeneratedPath = 0;

        Path = NavMesh.PathBuilder(Entity.Position)
            .WithMaxClimbDistance(16f)
            .WithMaxDropDistance(16f)
            .WithStepHeight(16f)
            .WithMaxDistance(99999999)
            .WithPartialPaths()
            .Build(target)
            .Segments
            .Select(x => x.Position)
            .ToArray();

        CurrentPathSegment = 0;
    }

    protected void TraversePath()
    {
        if (Path == null)
            return;

        var tickDistance = MovementSpeed * Time.Delta;

        var currentTarget = Path[CurrentPathSegment];
        if (CurrentPathSegment < Path.Length - 1)
        {
            var nextTarget = Path[CurrentPathSegment + 1];
            var rotation = Rotation.LookAt(nextTarget - Entity.Position);
            Entity.Rotation = rotation.Angles().WithRoll(0).WithPitch(0).ToRotation();
        }
        var distanceToCurrentTarget = Entity.Position.Distance(currentTarget);

        if (distanceToCurrentTarget > tickDistance)
        {
            var direction = (currentTarget - Entity.Position).Normal;
            MoveComponent.Forces.Add(direction * MovementSpeed);
            return;
        }
        else
        {
            CurrentPathSegment++;
        }

        if (CurrentPathSegment == Path.Count())
        {
            Path = null;
            return;
        }
    }

    private void DebugDrawPath()
    {
        if (Path == null)
            return;

        for (int i = CurrentPathSegment; i < Path.Length; i++)
        {
            if (i == CurrentPathSegment)
            {
                DebugOverlay.Sphere(Path[i], 10f, Color.Blue);
                continue;
            }
            DebugOverlay.Line(Path[i - 1], Path[i], Color.Red);
            DebugOverlay.Sphere(Path[i], 10f, Color.Green);
        }
    }
}
