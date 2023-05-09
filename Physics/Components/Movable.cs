using GXPEngine;
using System;

[Serializable]
public class Movable: Component, IRefreshable
{
    public Vec2 MinPoint { get; protected set; }
    public Vec2 MaxPoint { get; protected set; }

    public bool MovementLock { get; protected set; }
    public bool RotationLock { get; protected set; }

    private bool isSelected = false;

    public Movable(GameObject owner, params string[] args) : base(owner)
    {

    }

    public override void Refresh()
    {
        base.Refresh();
        SubscribeToInput();
    }
    protected override void Update() 
    {
        base.Update();
        if (!isSelected)
            return;

        
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