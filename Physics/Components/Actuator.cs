using GXPEngine;
using System;

[Serializable]
public class Actuate : Component
{
    public string Key { get; protected set; }
    public Actuate(GameObject owner) : base(owner)
    {

    }

    public override void Refresh()
    {
        base.Refresh();
    }
    protected override void Update()
    {
        base.Update();
    }
}