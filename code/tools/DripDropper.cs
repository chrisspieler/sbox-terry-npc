using Sandbox;
using Sandbox.Tools;
using System.Linq;

namespace TerryNpc;

[Library("tool_dripdropper", Description = "Steal his look! LMB to steal, RMB to apply, R to randomize!")]
public class DripDropper : BaseTool
{
    protected ClothingContainer DripSlot { get; set; }
    public override void Simulate()
    {
        if (!Game.IsServer)
            return;

        using (Prediction.Off())
        {

            if (Input.Pressed("attack1"))
            {
                ApplyLook();
            }
            else if (Input.Pressed("attack2"))
            {
                StealLook();
            }
            else if (Input.Pressed("reload"))
            {
                ApplyFreshLook();
            }
        }
    }

    public override TraceResult DoTrace()
    {
        var startPos = Owner.EyePosition;
        var dir = Owner.EyeRotation.Forward;

        return Trace.Ray(startPos, startPos + (dir * MaxTraceDistance))
            .WithAnyTags("player", "npc")
            .Ignore(Owner)
            .Run();
    }

    protected void ApplyLook()
    {
        if (DripSlot == null)
        {
            Sound.FromEntity("player_use_fail", Owner);
            return;
        }
        var tr = DoTrace();
        if (!tr.Hit || tr.Entity is not AnimatedEntity animated)
        {
            Sound.FromEntity("player_use_fail", Owner);
            return;
        }
        UndressEntity(animated);
        DripSlot.DressEntity(animated);
    }

    protected void ApplyFreshLook()
    {
        var tr = DoTrace();
        if (!tr.Hit || tr.Entity is not AnimatedEntity animated)
        {
            Sound.FromEntity("player_use_fail", Owner);
            return;
        }
        UndressEntity(animated);
        RandomOutfit.Generate().DressEntity(animated);
    }

    protected void StealLook()
    {
        var tr = DoTrace();
        if (!tr.Hit)
        {
            Sound.FromEntity("player_use_fail", Owner);
            return;
        }

        Log.Info(tr.Entity.Name);
        var clothes = tr
            .Entity
            .Children
            .OfType<AnimatedEntity>()
            .Where(c => c.Tags.Has("clothes"));
        Log.Info($"Found {clothes.Count()} clothes");
        var clothingResources = ResourceLibrary.GetAll<Clothing>();
        var dripContainer = new ClothingContainer();
        foreach(var clothing in clothes)
        {
            var clothingResource = clothingResources.FirstOrDefault(c => c.Model == clothing.Model?.Name);
            if (clothingResource != null)
            {
                dripContainer.Add(clothingResource);
            }
        }
        DripSlot = dripContainer;
        Log.Info(DripSlot.Serialize());
    }

    protected void UndressEntity(AnimatedEntity animated)
    {
        var existingClothing = animated.Children.Where(c => c.Tags.Has("clothes")).ToList();
        foreach (var c in existingClothing)
        {
            c.Delete();
        }
    }
}
