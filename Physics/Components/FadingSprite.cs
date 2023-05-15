using GXPEngine;
using System;

[Serializable]
public class FadingSprite : Component
{

    float fadeTime = 20f;

    float currentFadeTime = 0;

    public FadingSprite(GameObject owner, params string[] args) : base(owner)
    {

    }

    public override void Refresh()
    {
        base.Refresh();
    }
    protected override void Update()
    {
        base.Update();
        Fade();
    }

    public void Fade()
    {
        if (currentFadeTime > fadeTime)
        {
            return;
        }
        
        currentFadeTime += 1.0f / (float) Time.deltaTime;
        Debug.Log("time " + currentFadeTime);

        Sprite sprite = (Sprite)Owner;
        sprite.color = (uint)sprite.InterpolateColor(0xFFFFFF, 0x000000, currentFadeTime / fadeTime);
    }

}

/*



        Sprite empty = new Sprite("Empty");

        Sprite gun_top = new Sprite("gun_1");

        Sprite gun_bottom = new Sprite("gun_2");



       

gun_top.AddChild(gun_bottom);
gun_top.AddChild(empty);


gun_top.SetOrigin(320 - 172, 282 - 150);
gun_bottom.SetOrigin(320 - 267, 282 - 132);
gun_bottom.SetXY(0, 0);
gun_bottom.SetXY((320 - 267) - (320 - 172), (282 - 132) - (282 - 150));

//- 122 - 279
empty.SetXY(122 - (320 - 172), 279 - (282 - 150));

//gun_bottom.SetXY(320 - (150 - 132), 282 - (172 - 267));


empty.AddComponent(typeof(LightCaster), args: new string[]
    {
    }
);

gun_top.AddComponent(typeof(Movable), args: new string[]
    {
    }
);

gun_top.TryGetComponent(typeof(Movable), out Component c2);

Movable movableComponentTop = (Movable)c2;

// movableComponentTop.MovementLock.Equals(true);







AssetManager.UpdateAsset("laserGun", gun_top);

MainLayer.AddChild(gun_top);

Settings.CollisionDebug.Equals(true);

*/