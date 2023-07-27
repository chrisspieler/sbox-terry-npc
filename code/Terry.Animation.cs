using Sandbox;

namespace TerryNpc;

public partial class Terry
{
    public Vector3 LookAtPoint { get; set; }
    public void Animate()
    {
        var helper = new CitizenAnimationHelper(this);
        helper.WithLookAt(LookAtPoint);
        helper.WithVelocity(Velocity);
    }
}
