using GXPEngine;
using System;

[Serializable]
public class LightCaster: Component
{
    public string[] ActiveLayerMasks;
    public LightCaster(GameObject owner, params string[] args) : base(owner)
    {
        ActiveLayerMasks = args;
    }
    protected override void Update()
    {
        base.Update();

        int iterationCount = 0;
        bool result = true;
        Vec2 direction = Vec2.GetUnitVectorDegrees(Owner.rotation);
        Vec2 startPosition = Owner.position;
        while (result)
        {
            result = Physics.Collision.Raycast(startPosition, direction, Settings.RaycastStep, 1200f, out CollisionData collisionData);

            if (Settings.CollisionDebug)
            {
                Settings.ColliderDebug.StrokeWeight(4);
                Settings.ColliderDebug.Stroke(255);
                Settings.ColliderDebug.Line(startPosition.x + Camera.Position.x, startPosition.y + Camera.Position.y, collisionData.collisionPoints[0].x + Camera.Position.x, collisionData.collisionPoints[0].y + Camera.Position.y);
                Settings.ColliderDebug.StrokeWeight(1);
            }
            direction.Reflect(collisionData.collisionNormal);
            startPosition = collisionData.collisionPoints[0] + collisionData.collisionNormal;

            iterationCount++;
            if (iterationCount > 120)
                break;
        }
    }
}