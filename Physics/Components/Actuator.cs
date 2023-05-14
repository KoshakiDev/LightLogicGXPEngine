using GXPEngine;
using System;

[Serializable]
public class Actuator : Component
{
    public string Key { get; protected set; }
    public Actuator(GameObject owner) : base(owner)
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