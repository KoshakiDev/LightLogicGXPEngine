using GXPEngine;
using System;

[Serializable]
public class Activator: Component
{
    public string Key { get; protected set; }
    public Activator(GameObject owner): base(owner)
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