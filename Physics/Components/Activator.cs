using GXPEngine;
using System;

[Serializable]
public class Activate: Component
{
    public string Key { get; protected set; }
    public Activate(GameObject owner): base(owner)
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