using Sandbox;

namespace TerryNpc;

internal class Unstucker : EntityComponent<Terry>, ISingletonComponent
{
    [ConVar.Server("terrynpc.vis.unstuck")] private static bool VisualizeUnstucker { get; set; } = false;

    internal int StuckTries = 0;
    public bool IsStuck { get; set; } = false;

    [GameEvent.Tick.Server]
    public void OnServerTick()
    {
        IsStuck = TestAndFix();
    }

    private bool TestAndFix()
    {
        var result = Entity.TraceBBox(Entity.Position, Entity.Position);

        // Not stuck, we cool
        if (!result.StartedSolid)
        {
            StuckTries = 0;
            return false;
        }

        if (result.StartedSolid)
        {
            if (VisualizeUnstucker)
            {
                DebugOverlay.Text($"[stuck in {result.Entity}]", Entity.Position, Color.Red);
                DebugOverlay.Box(result.Entity, Color.Red);
            }
        }

        //
        // Client can't jiggle its way out, needs to wait for
        // server correction to come
        //
        if (Game.IsClient)
            return true;

        int AttemptsPerTick = 20;

        for (int i = 0; i < AttemptsPerTick; i++)
        {
            var pos = Entity.Position + Vector3.Random.WithZ(0).Normal * (StuckTries / 2.0f);

            // First try the up direction for moving platforms
            if (i == 0)
            {
                pos = Entity.Position + Vector3.Up * 5;
            }

            result = Entity.TraceBBox(pos, pos);

            if (!result.StartedSolid)
            {
                if (VisualizeUnstucker)
                {
                    DebugOverlay.Text($"unstuck after {StuckTries} tries ({StuckTries * AttemptsPerTick} tests)", Entity.Position, Color.Green, 5.0f);
                    DebugOverlay.Line(pos, Entity.Position, Color.Green, 5.0f, false);
                }

                Entity.Position = pos;
                return false;
            }
            else
            {
                if (VisualizeUnstucker)
                {
                    DebugOverlay.Line(pos, Entity.Position, Color.Yellow, 0.5f, false);
                }
            }
        }

        StuckTries++;

        return true;
    }
}
