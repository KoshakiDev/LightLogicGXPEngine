using GXPEngine;
using System;

[Serializable]
public class Movable: Component, IRefreshable
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
    
    }



    public override void Refresh()
    {
        base.Refresh();
        SubscribeToInput();
        relativePoint1 = Owner.position + Point1;
        relativePoint2 = Owner.position + Point2;
        Debug.Log("The first point is at " + relativePoint1 + ". The second point is at " + relativePoint2);
    }
    protected override void Update() 
    {
        base.Update();
        if (!isSelected)
            return;
        if (!MovementLock)
            MoveTowardsMouse();
        
    }
    void MoveTowardsMouse()
    {
        if (Owner.Rigidbody is null) return;

        Vec2 mousePosition = new Vec2(Input.mouseX, Input.mouseY);

        
        Vec2 pointOnLine = new Vec2(
            Mathf.Clamp(mousePosition.x, relativePoint1.x, relativePoint2.x),
            Mathf.Clamp(mousePosition.y, relativePoint1.y, relativePoint2.y)
        );

        Debug.Log("Point On Line: " + pointOnLine + " with Mouse Position of " + mousePosition + " and owner position of " + Owner.position);


        if (pointOnLine == Owner.position)
        {
            Debug.Log("Destination Reached");
            return;
        }    

        Vec2 directionToPoint = (pointOnLine - Owner.position).normalized;

        //Debug.Log("Direction to point: " + directionToPoint);

        Owner.position.SetXY(pointOnLine.x, pointOnLine.y);
        /**/

    }
    void ChangeSelection()
    {
        Vec2 mousePosition = new Vec2(Input.mouseX, Input.mouseY);

        bool selectionStatus = isSelected;

        if (!isSelected && Owner is Sprite sprite && sprite.HitTest(mousePosition))
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
        //if (Selection.SelectedGameObject is null)
            //return;

        Debug.Log(">> Selected : " + Owner.name);

        //TryOutliningGameObject(Selection.SelectedGameObject, true);
        //Selection.SelectionBoxSetVisible(true);
    }
    private void OnUnselected()
    {
        //if (Selection.SelectedGameObject is null)
            //return;

        Debug.Log(">> Deselected : " + Owner.name);

        //TryOutliningGameObject(Selection.SelectedGameObject, false);
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



}