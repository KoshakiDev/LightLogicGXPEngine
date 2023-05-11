﻿using GXPEngine;
using System;
using System.Windows.Forms;

[Serializable]
public class LightCaster: Component
{
    public string[] ActiveLayerMasks;

    public float refractiveIndexRed { get; protected set; } = 1.517f;
    public float refractiveIndexGreen { get; protected set; } = 1.624f;
    public float refractiveIndexBlue { get; protected set; } = 1.730f;


    int maxReflectAmount = 32;
    int currentReflections = 0;
    bool prismHit = false;

    public LightCaster(GameObject owner, params string[] args) : base(owner)
    {
        ActiveLayerMasks = args;
    }

    CollisionData prismHitCollisionData = CollisionData.Empty;
    Vec2 prismHitLastDirection = Vec2.Zero;           


    protected override void Update()
    {
        base.Update();

        prismHit = false;
        currentReflections = 0;

        SendRay(Owner.position, Vec2.GetUnitVectorDegrees(Owner.rotation), 6f);
        
        /*
        if (prismHit)
        {
            int savedReflectAmount = currentReflections;

            Vec2 prismHitStartPoint = prismHitCollisionData.collisionPoints[0];
            float angleOfIncidence = Vec2.Dot(prismHitLastDirection, prismHitCollisionData.collisionNormal);

            Vec2 directionRed = prismHitLastDirection;
            Vec2 directionGreen = prismHitLastDirection;
            Vec2 directionBlue = prismHitLastDirection;


            directionRed.RotateDegrees(GetAngleOfRefraction(angleOfIncidence, refractiveIndexRed));
            directionGreen.RotateDegrees(GetAngleOfRefraction(angleOfIncidence, refractiveIndexGreen));
            directionBlue.RotateDegrees(GetAngleOfRefraction(angleOfIncidence, refractiveIndexBlue));


            currentReflections = savedReflectAmount;
            SendRay(prismHitStartPoint, directionRed, refractiveIndexRed);

            currentReflections = savedReflectAmount;
            SendRay(prismHitStartPoint, directionGreen, refractiveIndexGreen);

            currentReflections = savedReflectAmount;
            SendRay(prismHitStartPoint, directionBlue, refractiveIndexBlue);
        }
        */

    }

    void SendRay(Vec2 startPosition, Vec2 direction, float refractiveIndex)
    {
        bool result = true;

        while (currentReflections <= maxReflectAmount)//(result && currentReflections <= maxReflectAmount)
        {
            result = Physics.Collision.Raycast(startPosition, direction, Settings.RaycastStep, 1200f, out CollisionData collisionData);

            if (Settings.CollisionDebug)
            {
                Settings.ColliderDebug.StrokeWeight(4);
                if (refractiveIndex == refractiveIndexRed)
                {
                    Settings.ColliderDebug.Stroke(255, 0, 0);
                }
                else if (refractiveIndex == refractiveIndexBlue)
                {
                    Settings.ColliderDebug.Stroke(0, 255, 0);
                }
                else if (refractiveIndex == refractiveIndexGreen)
                {
                    Settings.ColliderDebug.Stroke(0, 0, 255);
                }
                else
                {
                    Settings.ColliderDebug.Stroke(255);
                }

                Settings.ColliderDebug.Line(startPosition.x + Camera.Position.x, startPosition.y + Camera.Position.y, collisionData.collisionPoints[0].x + Camera.Position.x, collisionData.collisionPoints[0].y + Camera.Position.y);
                Settings.ColliderDebug.StrokeWeight(1);
            }




            float angleOfIncidence = Vec2.Dot(direction, collisionData.collisionNormal);

            float angleOfRefraction = GetAngleOfRefraction(angleOfIncidence, refractiveIndex);

            direction.RotateDegrees(angleOfRefraction);

            startPosition = collisionData.collisionPoints[0];

            /*
            if (result) //we hit a prism
            {
                //Settings.ColliderDebug.Ellipse(startPosition.x, startPosition.y, 20, 20);

                if (prismHit)
                {
                }
                else
                {
                    //prismHit = true;
                    //prismHitCollisionData = collisionData;
                    //prismHitLastDirection = direction;
                }
            }
            else
            {
                direction.Reflect(collisionData.collisionNormal);
                startPosition = collisionData.collisionPoints[0] + collisionData.collisionNormal;
            }
            */


            currentReflections++;
        }
    }
    float GetAngleOfRefraction(float angleOfIncidence, float refractiveIndex)
    {
        return Mathf.Asin((Mathf.Sin(angleOfIncidence) / refractiveIndex));
    }
}