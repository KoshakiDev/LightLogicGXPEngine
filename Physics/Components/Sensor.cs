using GXPEngine;
using System;
using System.Security.Policy;

[Serializable]
public class Sensor : Component
{

    float percentage = 0.0f;

    float speed = 1.0f; // per second

    public Sensor(GameObject owner, params string[] args) : base(owner)
    {

    }

    public override void Refresh()
    {
        base.Refresh();
        Sprite sprite = (Sprite)Owner;
        sprite.color = 0x000000;
    }
    protected override void Update()
    {
        
        base.Update();
    }

    public void OnHit()
    {
        if (percentage == 100f)
        {
            TurnOn();
            return;
        }
        percentage = Mathf.Clamp(percentage + (1.0f / (float) Time.deltaTime), 0f, 100f);
        Sprite sprite = (Sprite)Owner;
        sprite.color = (uint)sprite.InterpolateColor(0x000000, 0xFFFFFF, percentage / 100f);

    }

    public void TurnOn()
    {
        Debug.Log("Turn on");
        if (Owner.LayerMask == "Finish")
        {
            Debug.Log("Level complete");
        }
    }



}