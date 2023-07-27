using Sandbox;
using System.Linq;

namespace TerryNpc;

/// <summary>
/// Contains utility functions that require knowledge of the Sandbox game.
/// </summary>
public static class SandboxHelper
{
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
