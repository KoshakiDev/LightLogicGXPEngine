using GXPEngine;
using System;

[Serializable]
public class LightCaster: Component
{
    private enum LightColor
    {
        RED = 1,
        GREEN = 2,
        BLUE = 3,
        WHITE = 0
    }

    private const int MAX_RAY_DISTANCE = 1200;
    private const int MAX_REFLECT_COUNT = 256;
    private const float IOR_RED = 1.39f;
    private const float IOR_GREEN = 1.44f;
    private const float IOR_BLUE = 1.47f;
    private const float IOR_GLASS = 1.5f;

    public string[] ActiveLayerMasks;

    private bool _hasHitPrismOnce = false;
    private int _reflectCount = 0;
    public LightCaster(GameObject owner, params string[] args) : base(owner)
    {
        ActiveLayerMasks = args;
    }
    protected override void Update()
    {
        base.Update();

        _hasHitPrismOnce = false;
        Vec2 direction = Vec2.GetUnitVectorDegrees(Owner.rotation);
        Vec2 startPosition = Owner.position;
        _reflectCount = 0;

        Raycast(startPosition, direction, LightColor.WHITE);
    }

    private void Raycast(Vec2 startPosition, Vec2 direction, LightColor color)
    {
        while (true)
        {
            bool collision = Physics.Collision.Raycast(startPosition, direction, Settings.RaycastStep, MAX_RAY_DISTANCE, out CollisionData collisionData);

            if (_reflectCount > MAX_REFLECT_COUNT) break;
            if (Settings.CollisionDebug)
            {
                Settings.ColliderDebug.StrokeWeight(4);

                switch (color)
                {
                    case LightColor.RED:
                        Settings.ColliderDebug.Stroke(255, 0, 0);
                        break;
                    case LightColor.GREEN:
                        Settings.ColliderDebug.Stroke(0, 255, 0);
                        break;
                    case LightColor.BLUE:
                        Settings.ColliderDebug.Stroke(0, 0, 255);
                        break;
                    default:
                        Settings.ColliderDebug.Stroke(255);
                        break;
                }
                Settings.ColliderDebug.Line(startPosition.x + Camera.Position.x, startPosition.y + Camera.Position.y, collisionData.collisionPoints[0].x + Camera.Position.x, collisionData.collisionPoints[0].y + Camera.Position.y);
                Settings.ColliderDebug.StrokeWeight(1);
            }
            if (!collision) break;

            collisionData.self.TryGetComponent(typeof(Prism), out Component prismComponent);
            collisionData.self.TryGetComponent(typeof(ColliderSurfaceAttributes), out Component attrComponent);

            if (!_hasHitPrismOnce && prismComponent != null)
            {
                #region Divide into 3 rays (only once)
                _hasHitPrismOnce = true;

                Vec2 d1 = direction;
                Vec2 d2 = direction;
                Vec2 d3 = direction;

                startPosition = collisionData.collisionPoints[0] + direction.normalized;

                d1.RotateRadians(GetAngleOfRefractionInRadians(direction, collisionData.collisionNormal, IOR_RED));
                Raycast(startPosition, d1, LightColor.RED);

                d2.RotateRadians(GetAngleOfRefractionInRadians(direction, collisionData.collisionNormal, IOR_GREEN));
                Raycast(startPosition, d2, LightColor.GREEN);

                d3.RotateRadians(GetAngleOfRefractionInRadians(direction, collisionData.collisionNormal, IOR_BLUE));
                Raycast(startPosition, d3, LightColor.BLUE);

                _reflectCount++;
                break;
                #endregion
            }
            else if (prismComponent != null)
            {
                #region Pass through prism

                switch (color)
                {
                    case LightColor.RED:
                        direction.RotateRadians(GetAngleOfRefractionInRadians(direction, collisionData.collisionNormal, IOR_RED));
                        break;
                    case LightColor.GREEN:
                        direction.RotateRadians(GetAngleOfRefractionInRadians(direction, collisionData.collisionNormal, IOR_GREEN));
                        break;
                    case LightColor.BLUE:
                        direction.RotateRadians(GetAngleOfRefractionInRadians(direction, collisionData.collisionNormal, IOR_BLUE));
                        break;
                }

                startPosition = collisionData.collisionPoints[0] + direction.normalized;

                _reflectCount++;
                #endregion
            }
            else if (attrComponent != null && (attrComponent as ColliderSurfaceAttributes).TrespassingNormals)
            {
                #region Pass through lens
                collisionData.collisionNormal = collisionData.isInside ? collisionData.collisionNormal.inverted : collisionData.collisionNormal;
                direction = GetLensRefractedDirection(direction, collisionData.collisionNormal, 0.6f);
                startPosition = collisionData.collisionPoints[0] + direction * 8;

                _reflectCount++;
                #endregion
            }
            else
            {
                #region Bounce Off
                startPosition = collisionData.collisionPoints[0] + collisionData.collisionNormal;
                direction.Reflect(collisionData.collisionNormal);

                _reflectCount++;
                #endregion
            }
        }
    }
    private float GetAngleOfRefractionInRadians(Vec2 directionOfIncidence, Vec2 normal, float refractiveIndex)
    {
        float angleOfIncidenceInRadians = Vec2.Dot(directionOfIncidence, normal);
        return Mathf.Asin(Mathf.Sin(angleOfIncidenceInRadians) / refractiveIndex);
    }
    private Vec2 GetLensRefractedDirection(Vec2 incidentRay, Vec2 normal, float ior)
    {
        float cosI = Vec2.Dot(incidentRay.inverted.normalized, normal);

        float sinT2 = (ior / 1f) * (ior / 1f) * (1f - cosI * cosI);
        if (sinT2 > 1f)
        {
            incidentRay.Reflect(normal);
            return incidentRay.normalized;
        }
        else
        {
            float cosT = Mathf.Sqrt(1.0f - sinT2);
            return ((ior / 1f) * incidentRay.normalized + ((ior / 1f) * cosI - cosT) * normal).normalized;
        }
    }
}