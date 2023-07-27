using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerryNpc;

public partial class Terry
{
    public float BodyGirth => BaseBodyGirth * Scale;
    public float BodyHeight => BaseBodyHeight * Scale;
    [Net] public float BaseBodyGirth { get; set; } = 3f;
    [Net] public float BaseBodyHeight { get; set; } = 74f;
    public BBox Hull
    {
        get => new
        (
            new Vector3(-BodyGirth, -BodyGirth, 0),
            new Vector3(BodyGirth, BodyGirth, BodyHeight)
        );
    }
    public List<string> ImpassibleTags = new();

    /// <summary>
    /// Any bbox traces we do will be offset by this amount.
    /// todo: this needs to be predicted
    /// </summary>
    public Vector3 TraceOffset;

    /// <summary>
    /// Traces the bbox and returns the trace result.
    /// LiftFeet will move the start position up by this amount, while keeping the top of the bbox at the same 
    /// position. This is good when tracing down because you won't be tracing through the ceiling above.
    /// </summary>
    public virtual TraceResult TraceBBox(Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f)
    {
        if (liftFeet > 0)
        {
            start += Vector3.Up * liftFeet;
            maxs = maxs.WithZ(maxs.z - liftFeet);
        }

        var impassible = new List<string>(ImpassibleTags);
        impassible.AddRange(new string[] { "solid", "playerclip" });

        var tr = Trace.Ray(start + TraceOffset, end + TraceOffset)
                    .Size(mins, maxs)
                    .WithAnyTags(impassible.ToArray())
                    .Ignore(this)
                    .Run();

        tr.EndPosition -= TraceOffset;
        return tr;
    }

    /// <summary>
    /// This calls TraceBBox with the right sized bbox. You should derive this in your controller if you 
    /// want to use the built in functions
    /// </summary>
    public virtual TraceResult TraceBBox(Vector3 start, Vector3 end, float liftFeet = 0.0f)
    {
        return TraceBBox(start, end, Hull.Mins, Hull.Maxs, liftFeet);
    }
}
