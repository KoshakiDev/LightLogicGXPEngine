using GXPEngine;
using System;
using System.Windows.Forms.VisualStyles;

[Serializable]
public class Prism : Component, IRefreshable
{
    
    public Prism(GameObject owner, params string[] args) : base(owner)
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