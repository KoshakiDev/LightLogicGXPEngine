using GXPEngine;
using System;

[Serializable]
public class GunBottomThing : Component
{

    public GunBottomThing(GameObject owner, params string[] args) : base(owner)
    {

    }

    public override void Refresh()
    {
        base.Refresh();
    }
    protected override void Update()
    {
        base.Update();
        InverseRotate();
    }

    public void InverseRotate()
    {
        Owner.rotation = -Owner.parent.rotation - 90;
    }

}

/*

*/