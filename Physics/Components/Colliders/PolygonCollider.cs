using GXPEngine;
using System;
using System.Collections.Generic;

[Serializable] public class PolygonCollider : Collider
{ 
	public Vec2[] Points { get; protected set; }
    public Vec2[] TransformedPoints
    {
        get
        {
            Vec2[] transformedPoints = new Vec2[Points.Length];
            Vec2 origin = new Vec2(Owner.x, Owner.y);
            for (int i = 0; i < transformedPoints.Length; i++)
            {
                transformedPoints[i] = Points[i] * Owner.scale + origin;
                transformedPoints[i].RotateAroundDegrees(Owner.rotation, origin);
            }
            return transformedPoints;
        }
    }
    public Vec2[] DebugPoints
    {
        get
        {
            Vec2[] debugPoints = new Vec2[Points.Length];
            Vec2 origin = new Vec2(Owner.y, Owner.x);
            for (int i = 0; i < debugPoints.Length; i++)
            {
                debugPoints[i] = new Vec2(Points[i].x * Owner.scale.x + origin.x, origin.y - Points[i].y * Owner.scale.y);
                debugPoints[i].RotateAroundDegrees(-Owner.rotation + 90, origin);
                debugPoints[i] += new Vec2(Camera.Position.y , Camera.Position.x);
            }
            return debugPoints;
        }
    }
    public override float momentOfInertia(float mass)
    {
        float sum1 = 0f, sum2 = 0f;
        for (int i = 0; i < Points.Length; i++)
        {
            Vec2 p1 = Points[i];
            Vec2 p2 = Points[(i + 1) % Points.Length];
            float a = Vec2.Cross(p1, p2);
            float b = Vec2.DotSecondNormalized(p1, p1) + Vec2.DotSecondNormalized(p1, p2) + Vec2.DotSecondNormalized(p2, p2);
            sum1 += a * b;
            sum2 += a;
        }
        return (mass * sum1) / (6f * sum2);
    }
    public PolygonCollider(GameObject owner, params string[] args) : base(owner)  
    {
        IsTrigger = args.Length > 0 && bool.Parse(args[0]);

        List<Vec2> pointsBuffer = new List<Vec2>();
        if (args.Length > 0)
            for (int i = 1; i < args.Length - 1; i += 2)
                pointsBuffer.Add(new Vec2(float.Parse(args[i]), float.Parse(args[i + 1])));
        else
            pointsBuffer.Add(Vec2.Zero);

        Points = pointsBuffer.ToArray();
    }
    public override float CalculateActiveRadius()
    {
        LogicalCenterOfMass = Vec2.Zero;
        foreach (Vec2 point in Points)
            LogicalCenterOfMass += point;
        LogicalCenterOfMass /= Points.Length;

        float maxDistance = float.MinValue;
        foreach (Vec2 point in Points)
        {
            float currentDistance = Vec2.Distance(point * Owner.scale, LogicalCenterOfMass);
            if (currentDistance > maxDistance)
                maxDistance = currentDistance;
        }
        return maxDistance;
    }

    public Vec2[] GetAxes()
    {
        Vec2[] axes = new Vec2[Points.Length];
        for (int i = 0; i < Points.Length; i++)
        {
            Vec2 p1 = Points[i];
            Vec2 p2 = Points[(i + 1) % Points.Length];
            Vec2 edge = p2 - p1;
            edge.Normalize();
            axes[i] = new Vec2(-edge.y, edge.x);
        }
        return axes;
    }
    public Vec2[] GetEdges()
    {
        Vec2[] edges = new Vec2[Points.Length];
        for (int i = 0; i < Points.Length; i++)
        {
            Vec2 p1 = TransformedPoints[i];
            Vec2 p2 = TransformedPoints[(i + 1) % Points.Length];
            edges[i] = p2 - p1;
        }
        return edges;
    }

    public Vec2 CatmullRomPoint(int p1i, int p2i, float t)
    {
        t = t > 1 ? 1 : (t < 0 ? 0 : t);
        Vec2 p0 = p1i > 0 ? TransformedPoints[p1i - 1] : TransformedPoints[p1i];
        Vec2 p1 = TransformedPoints[p1i];
        Vec2 p2 = TransformedPoints[p2i];
        Vec2 p3 = p2i < Points.Length - 1 ? TransformedPoints[p2i + 1] : TransformedPoints[p2i];
        float t2 = t * t;
        float t3 = t2 * t;
        Vec2 smoothPoint = 0.5f * ((2.0f * p1) + (p2 - p0) * t +
            (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2 +
            (3.0f * p1 - 3.0f * p2 + p3 - p0) * t3);
        return smoothPoint;
    }
    public Vec2 CatmullRomPointLooped(int p1i, int p2i, float t)
    {
        t = t > 1? 1 : (t < 0 ? 0 : t);
        Vec2 p0 = p1i > 0 ? TransformedPoints[p1i - 1] : TransformedPoints[Points.Length - 1];
        Vec2 p1 = TransformedPoints[p1i];
        Vec2 p2 = TransformedPoints[p2i];
        Vec2 p3 = p2i < Points.Length - 1 ? TransformedPoints[p2i + 1] : TransformedPoints[0];
        float t2 = t * t;
        float t3 = t2 * t;
        Vec2 smoothPoint = 0.5f * ((2.0f * p1) + (p2 - p0) * t +
            (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2 +
            (3.0f * p1 - 3.0f * p2 + p3 - p0) * t3);
        return smoothPoint;
    }
    public Vec2 CatmullRomNormal(int p1i, int p2i, float t)
    {
        t = t > 1 ? 1 : (t < 0 ? 0 : t);
        Vec2 tangent = CatmullRomPoint(p1i, p2i, t + 0.01f) - CatmullRomPoint(p1i, p2i, t - 0.01f);
        Vec2 normal = new Vec2(-tangent.y, tangent.x).normalized;
        return normal;
    }
    public Vec2 CatmullRomNormalLooped(int p1i, int p2i, float t)
    {
        t = t > 1 ? 1 : (t < 0 ? 0 : t);
        Vec2 tangent = CatmullRomPointLooped(p1i, p2i, t + 0.01f) - CatmullRomPointLooped(p1i, p2i, t - 0.01f);
        Vec2 normal = new Vec2(-tangent.y, tangent.x).normalized;
        return normal;
    }

    public override void ShowDebug()
    {

        Settings.ColliderDebug.Stroke(255, 255, 255);
        float[] pointCoordinates = new float[DebugPoints.Length * 2];
        for (int i = 0; i < DebugPoints.Length; i++) 
        {
            Vec2 rotatedCoordinates = new Vec2(DebugPoints[i].x, DebugPoints[i].y);
            pointCoordinates[((i + 1) * 2) - 1] = rotatedCoordinates.x ;
            pointCoordinates[i * 2] = rotatedCoordinates.y ;
        }
        Settings.ColliderDebug.Polygon(pointCoordinates);      
    }
    public override void ShowEditorDebug()
    {
        float[] pointCoordinates = new float[DebugPoints.Length * 2];
        for (int i = 0; i < DebugPoints.Length; i++)
        {
            Vec2 rotatedCoordinates = new Vec2(DebugPoints[i].x, DebugPoints[i].y);
            pointCoordinates[((i + 1) * 2) - 1] = rotatedCoordinates.x;
            pointCoordinates[i * 2] = rotatedCoordinates.y;
        }
        Settings.EditorColliderDebug.Polygon(pointCoordinates);
    }

    protected virtual void RefreshPoints() { }
}


