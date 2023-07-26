using Sandbox;

namespace TerryNpc;

public partial class Terry
{
    public Vector3 LookAtPoint { get; set; }
    public void Animate()
    {
        new CitizenAnimationHelper(this)
            .WithLookAt(LookAtPoint);
    }
}
