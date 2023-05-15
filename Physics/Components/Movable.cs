using GXPEngine;
using System;
using System.Windows.Forms;

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
        ClearRails();
        visualize();

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

        if (relativePoint1.x > relativePoint2.x || relativePoint1.y > relativePoint2.y)
            direction *= -1;

        Vec2 step = direction * (relativePoint2 - relativePoint1) * Time.deltaTime / 1000f * 0.5f;
        Owner.SetXY(Owner.position + step);

        Vec2 min = new Vec2(Mathf.Min(relativePoint1.x, relativePoint2.x), Mathf.Min(relativePoint1.y, relativePoint2.y));
        Vec2 max = new Vec2(Mathf.Max(relativePoint1.x, relativePoint2.x), Mathf.Max(relativePoint1.y, relativePoint2.y));

        if (Owner.x < min.x) Owner.x = min.x;
        if (Owner.y < min.y) Owner.y = min.y;
        if (Owner.x > max.x) Owner.x = max.x;
        if (Owner.y > max.y) Owner.y = max.y;
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

    Sprite rail;
    Sprite rail_point_1;
    Sprite rail_point_2;

    private void visualize()
    {
        Vec2 offsetY = new Vec2(0, -40);

        Vec2 offsetX = new Vec2(65, 0);

        float staticAngle = (Point2 - Point1).angleInDeg + 90;

        Vec2 offset_1 = (offsetY + offsetX);
        Vec2 offset_2 = (offsetY - offsetX);


        offset_1.RotateDegrees(staticAngle);

        offset_2.RotateDegrees(staticAngle);


        Vec2 start = relativePoint1 + offset_1;
            
        
        Vec2 end = relativePoint2 + offset_2;
        

        rail = new Sprite("rail_segment");
        rail_point_1 = new Sprite("rail_point");
        rail_point_2 = new Sprite("rail_point");

        int index = 50;

        Setup.MainLayer.AddChildAt(rail, index);
        Setup.MainLayer.AddChildAt(rail_point_1, index + 1);
        Setup.MainLayer.AddChildAt(rail_point_2, index + 1);


        rail_point_1.SetXY(start);

        rail_point_2.SetXY(end);

        rail_point_1.SetOrigin(rail_point_1.width / 2, rail_point_1.height / 2);
        rail_point_2.SetOrigin(rail_point_2.width / 2, rail_point_2.height / 2);


        rail.SetOrigin(rail.width / 2, rail.height / 2);

        rail.SetXY(start);

        rail.scaleX = Vec2.Distance(start, end);
        rail.rotation = (end - start).angleInDeg + 90;


        

        //rail.SetScaleXY(Owner.scaleY);
        rail_point_1.SetScaleXY(Owner.scaleY);
        rail_point_2.SetScaleXY(Owner.scaleY);




        

    }



    private void ClearRails()
    {
        rail?.Destroy();
        rail_point_1?.Destroy();
        rail_point_2?.Destroy();
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