using GXPEngine;
using System;

[Serializable]
public class GunBottomThing : Component
{
    public GunBottomThing(GameObject owner, params string[] args) : base(owner){}
    public override void Refresh(){base.Refresh();}
    protected override void Update()
    {
        base.Update();
        InverseRotate();
    }
    public void InverseRotate()
    {
        if (Owner.parent is null)
            return;

        float staticAngle = 90;
        Owner.parent.TryGetComponent(typeof(Movable), out Component component);
        if (component != null)
        {
            Movable movable = component as Movable;
            staticAngle = (movable.Point2 - movable.Point1).angleInDeg;
        } 
        Owner.rotation = -Owner.parent.rotation - staticAngle;
    }

}