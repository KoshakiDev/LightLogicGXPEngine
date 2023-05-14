using GXPEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class LightCaster: Component
{
    private enum LightColor
    {
        RED = 0xff0000,
        GREEN = 0x00ff00,
        BLUE = 0x0000ff,
        WHITE = 0xffffff
    }

    private const int MAX_RAY_DISTANCE = 1200;
    private const int MAX_REFLECT_COUNT = 256;
    private const float IOR_RED = 1.39f;
    private const float IOR_GREEN = 1.44f;
    private const float IOR_BLUE = 1.47f;

    public string[] ActiveLayerMasks;

    private int _reflectCount = 0;
    private List<Sprite> _rays = new List<Sprite>();
    public LightCaster(GameObject owner, params string[] args) : base(owner)
    {
        ActiveLayerMasks = args;      
    }
    protected override void Update()
    {
        base.Update();
        ClearRays();

        Vec2 direction = Vec2.GetUnitVectorDegrees(Owner.rotation);
        Vec2 startPosition = Owner.position;

        Sprite collisionPoint = new Sprite("laser_collision");
        Setup.MainLayer.AddChild(collisionPoint);

        collisionPoint.SetXY(startPosition);
        collisionPoint.SetOrigin(collisionPoint.width / 2, collisionPoint.height / 2);

        _rays.Add(collisionPoint);
        _reflectCount = 0;

        Raycast(startPosition, direction, LightColor.WHITE, collisionPoint.Index);
    }

    private void Raycast(Vec2 startPosition, Vec2 direction, LightColor color, int order)
    {
        Vec2 visualOffset = Vec2.Zero;
        while (true)
        {
            bool collision = Physics.Collision.Raycast(startPosition, direction, Settings.RaycastStep, MAX_RAY_DISTANCE, out CollisionData collisionData);

            if (_reflectCount > MAX_REFLECT_COUNT) 
                break;
            visualize();
            if (!collision) break;
            collisionData.self.TryGetComponent(typeof(ColliderSurfaceAttributes), out Component attrComponent);
            
            if(collisionData.self.LayerMask == "Finish")
            {
                Debug.Log("Sensor've got hit!");
                //Settings.Setup.Exit();
                return;
            }

            if (color == LightColor.WHITE && collisionData.self.LayerMask == "Prisms")
            {
                #region Divide into 3 rays (only once)

                Vec2 d1 = direction;
                Vec2 d2 = direction;
                Vec2 d3 = direction;

                startPosition = collisionData.collisionPoints[0] + direction.normalized;

                d1.RotateRadians(GetAngleOfRefractionInRadians(direction, collisionData.collisionNormal, IOR_RED));
                Raycast(startPosition, d1, LightColor.RED, order);

                d2.RotateRadians(GetAngleOfRefractionInRadians(direction, collisionData.collisionNormal, IOR_GREEN));
                Raycast(startPosition, d2, LightColor.GREEN, order);

                d3.RotateRadians(GetAngleOfRefractionInRadians(direction, collisionData.collisionNormal, IOR_BLUE));
                Raycast(startPosition, d3, LightColor.BLUE, order);

                _reflectCount++;
                break;
                #endregion
            }
            else if (collisionData.self.LayerMask == "Prisms")
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
            else if (collisionData.self.LayerMask == "Mirrors" && attrComponent != null && (attrComponent as ColliderSurfaceAttributes).TrespassingNormals)
            {
                #region Pass through lens
                collisionData.collisionNormal = collisionData.isInside ? collisionData.collisionNormal.inverted : collisionData.collisionNormal;
                direction = GetLensRefractedDirection(direction, collisionData.collisionNormal, 0.6f);
                startPosition = collisionData.collisionPoints[0] + direction * 8;
                visualOffset = direction * 8;

                _reflectCount++;
                #endregion
            }
            else if (collisionData.self.LayerMask == "Mirrors")
            {
                #region Bounce Off
                startPosition = collisionData.collisionPoints[0] + collisionData.collisionNormal * 8;
                visualOffset = collisionData.collisionNormal * 8;
                direction.Reflect(collisionData.collisionNormal);
                _reflectCount++;
                #endregion
            }
            else break;

            void visualize()
            {
                Vec2 start = startPosition - visualOffset;
                Vec2 end = collisionData.collisionPoints[0];
                Sprite ray = new Sprite("laser_segment");
                Sprite rayLight = new Sprite("laser_light");
                Sprite rayLight2 = new Sprite("laser_light2");
                Sprite collisionPoint = new Sprite("laser_collision");
                Sprite colLight = new Sprite("collision_light");
                Sprite colLight2 = new Sprite("collision_light2");
                Setup.MainLayer.AddChildAt(collisionPoint, order);
                Setup.MainLayer.AddChildAt(ray, order);
                ray.AddChild(rayLight);
                ray.AddChild(rayLight2);
                collisionPoint.AddChild(colLight);
                collisionPoint.AddChild(colLight2);
                collisionPoint.SetXY(end);
                collisionPoint.SetOrigin(collisionPoint.width/2, collisionPoint.height / 2);
                rayLight.blendMode = BlendMode.ADDITIVE;
                rayLight2.blendMode = BlendMode.ADDITIVE;
                colLight.blendMode = BlendMode.ADDITIVE;
                colLight2.blendMode = BlendMode.ADDITIVE;
                ray.SetXY(start);
                ray.SetOrigin(0, ray.height / 2);
                rayLight.SetOrigin(0, rayLight.height / 2);
                rayLight2.SetOrigin(0, rayLight2.height / 2);
                colLight.SetOrigin(colLight.width / 2, colLight.height / 2);
                colLight2.SetOrigin(colLight2.width / 2, colLight2.height / 2);
                ray.scaleX = Vec2.Distance(start, end);
                ray.rotation = (end - start).angleInDeg + 90;
                ray.color = (uint)color - 0x222222;
                rayLight.color = (uint)color;
                rayLight2.color = (uint)color;
                collisionPoint.color = (uint)color - 0x222222;
                colLight.color = (uint)color;
                colLight2.color = (uint)color;
                _rays.Add(ray);
                _rays.Add(collisionPoint);
            }
        }
    }
    private void ClearRays()
    {
        for (int i = 0; i < _rays.Count; i++)
        {
            _rays[0].Destroy();
            _rays.RemoveAt(0);
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