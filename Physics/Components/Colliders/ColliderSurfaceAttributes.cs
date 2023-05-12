using GXPEngine;
using System;

[Serializable] public class ColliderSurfaceAttributes : Component
{
    public bool SmoothenedNormals { get; protected set; }
    public bool SmoothenedPoints { get; protected set; }
    public bool LoopedSmoothing { get; protected set; }
    public bool TrespassingNormals { get; protected set; } 
    public ColliderSurfaceAttributes(GameObject owner, params string[] args) : base(owner)
    {
        SmoothenedNormals = false;
        SmoothenedPoints = false;
        LoopedSmoothing = false;
        TrespassingNormals = false;
    }
}