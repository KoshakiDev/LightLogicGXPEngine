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

*/