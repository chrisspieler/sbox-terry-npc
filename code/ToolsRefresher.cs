using Sandbox;
using System.Linq;

namespace TerryNpc;

public partial class ToolsRefresher : Entity
{
    public override void Spawn()
    {
        base.Spawn();

        // Refresh the tools list on all clients.
        RefreshToolsList();
        _ = DeleteAsync(Time.Delta);
    }

    [ClientRpc]
    public static void RefreshToolsList()
    {
        var spawnMenu = Game.RootPanel?.ChildrenOfType<SpawnMenu>()?.FirstOrDefault();
        if (spawnMenu == null)
        {
            Log.Info("Spawn menu not found when refreshing tools list.");
            return;
        }
        // This is a hack to make the spawn menu reload the tools list.
        spawnMenu.OnHotloaded();
    }
}
