using GXPEngine;
using System;

[Serializable]
public class LightCaster: Component
{
    private const int MAX_RAY_DISTANCE = 1200;
    private const int MAX_REFLECT_COUNT = 256;
    public string[] ActiveLayerMasks;


    float refractiveIndexRed = 1.39f;
    float refractiveIndexGreen= 1.44f;
    float refractiveIndexBlue = 1.47f;

    enum Color 
    {
        RED,
        GREEN,
        BLUE,
        WHITE
    }

    Color color = Color.WHITE;

    int maxIterationCount = 16; //120

    bool hasHitPrismOnce = false;
    public LightCaster(GameObject owner, params string[] args) : base(owner)
    {
        ActiveLayerMasks = args;
    }
    protected override void Update()
    {
        base.Update();

        hasHitPrismOnce = false;
        Vec2 direction = Vec2.GetUnitVectorDegrees(Owner.rotation);
        Vec2 startPosition = Owner.position;

        RaycastRecursion(startPosition, direction, 0, Color.WHITE);
    }

    void RaycastRecursion(Vec2 startPosition, Vec2 direction, int iterationCount, Color color)
    {
        
        bool hasFoundCollision = Physics.Collision.Raycast(startPosition, direction, Settings.RaycastStep, 1200f, out CollisionData collisionData);

        if (iterationCount > maxIterationCount)
        {
            return;
        }

        if (Settings.CollisionDebug)
        {
            Settings.ColliderDebug.StrokeWeight(4);
            
            switch(color)
            {
                case Color.RED:
                    Settings.ColliderDebug.Stroke(255, 0, 0);
                    break;
                case Color.GREEN:
                    Settings.ColliderDebug.Stroke(0, 255, 0);
                    break;
                case Color.BLUE:
                    Settings.ColliderDebug.Stroke(0, 0, 255);
                    break;
                default:
                    Settings.ColliderDebug.Stroke(255);
                    break;
            }
            Settings.ColliderDebug.Line(startPosition.x + Camera.Position.x, startPosition.y + Camera.Position.y, collisionData.collisionPoints[0].x + Camera.Position.x, collisionData.collisionPoints[0].y + Camera.Position.y);
            Settings.ColliderDebug.StrokeWeight(1);
        }

        if (!hasFoundCollision)
        {
            return;
        }

        /*
        Collision Data has "self" as the "other" collider. KEEP THIS IN MIND
        */
        GameObject collidedObject = collisionData.self;

        collidedObject.TryGetComponent(typeof(Prism), out Component prismComponent);
        
        if (!hasHitPrismOnce && prismComponent != null)
        {
            #region Divide into 3 rays (only once)
            hasHitPrismOnce = true;

            Vec2 d1 = direction;

            Vec2 d2 = direction;

            Vec2 d3 = direction;

            startPosition = collisionData.collisionPoints[0] + direction.normalized;

            d1.RotateRadians(GetAngleOfRefractionInRadians(direction, collisionData.collisionNormal, refractiveIndexRed));
            RaycastRecursion(startPosition, d1, iterationCount + 1, Color.RED);

            d2.RotateRadians(GetAngleOfRefractionInRadians(direction, collisionData.collisionNormal, refractiveIndexGreen));
            RaycastRecursion(startPosition, d2, iterationCount + 1, Color.GREEN);

            d3.RotateRadians(GetAngleOfRefractionInRadians(direction, collisionData.collisionNormal, refractiveIndexBlue));
            RaycastRecursion(startPosition, d3, iterationCount + 1, Color.BLUE);
            #endregion
        }
        else if(prismComponent != null)
        {
            #region Pass Through Prism

            switch (color)
            {
                case Color.RED:
                    direction.RotateRadians(GetAngleOfRefractionInRadians(direction, collisionData.collisionNormal, refractiveIndexRed));
                    break;
                case Color.GREEN:
                    direction.RotateRadians(GetAngleOfRefractionInRadians(direction, collisionData.collisionNormal, refractiveIndexGreen));

                    break;
                case Color.BLUE:
                    direction.RotateRadians(GetAngleOfRefractionInRadians(direction, collisionData.collisionNormal, refractiveIndexBlue));
                    break;
            }

            startPosition = collisionData.collisionPoints[0] + direction.normalized;

            RaycastRecursion(startPosition, direction, iterationCount + 1, color);
            #endregion
        }
        else
        {
            #region Bounce Off
            startPosition = collisionData.collisionPoints[0] + collisionData.collisionNormal;

            direction.Reflect(collisionData.collisionNormal);

            RaycastRecursion(startPosition, direction, iterationCount + 1, color);
            #endregion
        }
    }
    float GetAngleOfRefractionInRadians(Vec2 directionOfIncidence, Vec2 normal, float refractiveIndex)
    {
        float angleOfIncidenceInRadians = Vec2.Dot(directionOfIncidence, normal);
        return Mathf.Asin((Mathf.Sin(angleOfIncidenceInRadians) / refractiveIndex));
    }

}