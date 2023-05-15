using GXPEngine;
using System;

[Serializable]
public class Movable : Component, IRefreshable
{
    public Vec2 Point1 { get; protected set; }
    public Vec2 Point2 { get; protected set; }

    public bool MovementLock { get; protected set; }
    public bool RotationLock { get; protected set; }

    private bool isSelected = false;

    Vec2 relativePoint1 = Vec2.Zero;
    Vec2 relativePoint2 = Vec2.Zero;

    public Movable(GameObject owner, params string[] args) : base(owner)
    {
        Point1 = new Vec2(-20, 0);
        Point2 = new Vec2(20, 0);
        (Owner as Sprite).SetColor(0.8f, 0.8f, 0.8f);
    }

    public override void Refresh()
    {
        base.Refresh();
        SubscribeToInput();

        relativePoint1 = Owner.position + Point1;
        relativePoint2 = Owner.position + Point2;
    }
    protected override void Update()
    {
        base.Update();
        ShowDebug();
        if (!isSelected)
            return;
    }
    private void OnMouseMoved()
    {
        if (RotationLock)
            return;
        Owner.rotation = (Input.mouseWorldPosition - Owner.position).angleInDeg;
    }
    private void Move(sbyte direction) 
    {
        if (MovementLock)
            return;

        Vec2 step = direction * (relativePoint2 - relativePoint1) * Time.deltaTime / 1000f * 0.5f;
        Owner.SetXY(Owner.position + step);
        if (Owner.x < relativePoint1.x) Owner.x = relativePoint1.x;
        if (Owner.y < relativePoint1.y) Owner.y = relativePoint1.y;
        if (Owner.x > relativePoint2.x) Owner.x = relativePoint2.x;
        if (Owner.y > relativePoint2.y) Owner.y = relativePoint2.y;
    }
    private void Right()
    {
        Move(1);
    }
    private void Left()
    {
        Move(-1);
    }
    private void ChangeSelection()
    {
        bool selectionStatus = isSelected;

        if (!isSelected && Owner is Sprite sprite && sprite.HitTest(Input.mouseScreenPosition))
           isSelected = true;
        else
            isSelected = false;
        
        if (selectionStatus != isSelected && isSelected == true)
        {
            OnSelected();
        }
        else if (selectionStatus != isSelected && isSelected == false)
        {
            OnUnselected();
        }

    }

    private void OnSelected()
    {
        Debug.Log(">> Selected : " + Owner.name);
        SubscribeToInputWhenSelected();
    }
    private void OnUnselected()
    {
        Debug.Log(">> Deselected : " + Owner.name);
        UnsubscribeFromInputWhenUnselected();
    }

    private void SubscribeToInput()
    {
        InputManager.OnRightMousePressedDown += ChangeSelection;
    }
    private void UnsubscribeFromInput()
    {
        InputManager.OnRightMousePressedDown -= ChangeSelection;
    }

    private void SubscribeToInputWhenSelected()
    {
        InputManager.OnMouseMoved += OnMouseMoved;
        InputManager.OnAButtonPressed += Left;
        InputManager.OnDButtonPressed += Right;
        (Owner as Sprite).SetColor(1f, 1f, 1f);
    }
    private void UnsubscribeFromInputWhenUnselected()
    {
        InputManager.OnMouseMoved -= OnMouseMoved;
        InputManager.OnAButtonPressed -= Left;
        InputManager.OnDButtonPressed -= Right;
        (Owner as Sprite).SetColor(0.8f, 0.8f, 0.8f);
    }

    private void ShowDebug()
    {
        if (Settings.CollisionDebug)
        {
            Settings.ColliderDebug.Stroke(0, 180, 0);
            Settings.ColliderDebug.Line(relativePoint1.x + Camera.Position.x, relativePoint1.y + Camera.Position.y, relativePoint2.x + Camera.Position.x, relativePoint2.y + Camera.Position.y);
            Settings.ColliderDebug.Ellipse(relativePoint1.x + Camera.Position.x, relativePoint1.y + Camera.Position.y, 10, 10);
            Settings.ColliderDebug.Ellipse(relativePoint2.x + Camera.Position.x, relativePoint2.y + Camera.Position.y, 10, 10);
            Settings.ColliderDebug.Stroke(255);
        }
    }
}