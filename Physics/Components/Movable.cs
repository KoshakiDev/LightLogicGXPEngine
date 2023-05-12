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

    public Movable(GameObject owner, params string[] args) : base(owner){}

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
        if (!isSelected)
            return;
    }


    void OnMouseMoved()
    {
        Vec2 pointOnLine = Vec2.ClampPoint(Input.mouseWorldPosition, relativePoint1, relativePoint2);
        Vec2 rotateTowards = Input.mouseWorldPosition - pointOnLine;

        //TODO fix rotation as well

        if (!MovementLock) Owner.SetXY(pointOnLine);
        if (!RotationLock) Owner.rotation = (Input.mouseWorldPosition.y - pointOnLine.y) * 0.2f;
    }

    void ChangeSelection()
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

        //TryOutliningGameObject(Owner, isSelected);
        //Selection.SelectionBoxSetVisible(true);
    }
    private void OnUnselected()
    {

        Debug.Log(">> Deselected : " + Owner.name);
        UnsubscribeFromInputWhenUnselected();
        //TryOutliningGameObject(Owner, isSelected);
        //Selection.SelectionBoxSetVisible(false);
    }


    void SubscribeToInput()
    {
        InputManager.OnRightMousePressed += ChangeSelection;
    }

    void UnsubscribeFromInput()
    {
        InputManager.OnRightMousePressed -= ChangeSelection;
    }

    void SubscribeToInputWhenSelected()
    {
        InputManager.OnMouseMoved += OnMouseMoved;
    }

    void UnsubscribeFromInputWhenUnselected()
    {
        InputManager.OnMouseMoved -= OnMouseMoved;
    }
}