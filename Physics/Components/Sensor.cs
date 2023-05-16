using GXPEngine;
using System;
using System.Security.Policy;

[Serializable]
public class Sensor : Component
{
    public float Percentage { get; protected set; } = 0.0f;
    public float Speed { get; protected set; } = 6.0f; // per second

    public Sensor(GameObject owner, params string[] args) : base(owner){}

    public override void Refresh()
    {
        base.Refresh();
        Sprite sprite = (Sprite)Owner;
        sprite.color = 0x555555;
    }
    protected override void Update()
    { 
        base.Update();
    }

    public void OnHit()
    {
        if (Percentage == 100f)
        {
            TurnOn();
            return;
        }
        Percentage = Mathf.Clamp(Percentage + (Speed * Time.deltaTime / 1000f), 40f, 100f);
        Sprite sprite = (Sprite)Owner;
        sprite.color = (uint)sprite.InterpolateColor(0x000000, 0xFFFFFF, Percentage / 100f);
    }

    public void TurnOn()
    {
        if (Owner.LayerMask == "Finish")
        {
            LightLogicGame.FinishLevel();
        }
    }
}