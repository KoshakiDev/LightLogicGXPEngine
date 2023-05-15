using GXPEngine;
using System.Reflection;

public static class Selection
{
    public const int UNSELECTED = 0x757575;
    public const int SELECTED = 0xFFFFFF;

    public static string Filename;
    public static GameObject DocumentObject;

    private static GameObject _selectedGameObject;
    public static GameObject SelectedGameObject
    {
        get => _selectedGameObject;
        set
        {
            _selectedGameObject = value;
            ClearComponents();

            if (value is null)
                return;

            if (!_displayersSet)
            {
                InitSelectionBox();
                _displayersSet = true;
            }
            RefreshText();
            RefreshComponents();
        }
    }
    private static EasyDraw _selectedName, _selectedLayer;
    private static VerticalList _componentList;
    private static bool _displayersSet = false;

    public static void Transform(Vec2 position, Vec2 scaleDelta, float rotationDelta = 0f)
    {
        if (SelectedGameObject is null)
            return;

        SelectedGameObject.SetScaleXY(SelectedGameObject.scaleX + scaleDelta.x, SelectedGameObject.scaleY + scaleDelta.y);
        SelectedGameObject.SetXY(SelectedGameObject.x - position.x, SelectedGameObject.y - position.y);
        SelectedGameObject.rotation += rotationDelta;
    }

    public static void InitSelectionBox()
    {
        Setup.SelectionBox.AddChild(
        new GUIButton("element_base0,5",
        action: () => new WaitForFieldInput(typeof(GameObject).GetField("name", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), SelectedGameObject, inQueue: false),
        layerMask: "GUI")
        {
            x = 110,
            y = 53,
            scaleX = 1.56f
        });
        Setup.SelectionBox.AddChild(
        new GUIButton("element_base0,5",
        action: () => new WaitForPropertyInput(typeof(GameObject).GetProperty("LayerMask", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), SelectedGameObject, inQueue: false),
        layerMask: "GUI")
        {
            x = 110,
            y = 70,
            scaleX = 1.56f
        });
        Setup.SelectionBox.AddChild(_selectedName
        = new EasyDraw(100, 50, layerMask: "GUI")
        {
            x = 60,
            y = 14,
            color = 0xffbb00,
            font = GXPAssetEditor.EditorFont,
        });
        Setup.SelectionBox.AddChild(_selectedLayer
        = new EasyDraw(100, 50, layerMask: "GUI")
        {
            x = 60,
            y = 30,
            color = 0xffbb00,
            font = GXPAssetEditor.EditorFont
        });
        Setup.SelectionBox.AddChild(
        new GUIButton("plus_icon",
        action: () => GXPAssetEditor.OpenAddComponent(SelectedGameObject),
        layerMask: "GUI")
        {
            x = 144,
            y = 380,
        });
        Setup.SelectionBox.AddChild(
        new GUIButton("img_icon",
        action: () => GXPAssetEditor.ChangeTexture(),
        layerMask: "GUI")
        {
            x = 28,
            y = 380,
        });
        Setup.SelectionBox.AddChild(_componentList =
        new VerticalList(21, layerMask: "GUI")
        {
            x = 86,
            y = 128,
        });
    }
    private static void ClearComponents()
    {
        if (_componentList is null)
            return;

        _componentList.Clear();
    }
    public static void RefreshComponents()
    {
        if (SelectedGameObject is null)
            return;

        _componentList.Clear();
        Component[] components = SelectedGameObject.GetAllComponents();
        GUIButton[] elements = new GUIButton[components.Length];
        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];
            string componentName = component.GetType().Name;
            elements[i] = new GUIButton("element_base", action: () => GXPAssetEditor.OpenComponentBox(component), layerMask: "GUI");
            elements[i].AddChild(new ConstValueDisplayer<string>(componentName.Length > 14 ? componentName.Substring(0, 12) + ".." : componentName, 130, 18)
            {
                x = -66,
                y = -8,
                color = 0xffbb00,
                font = GXPAssetEditor.EditorFont,
                HorizontalShapeAlign = CenterMode.Center,
                VerticalShapeAlign = CenterMode.Center
            });
            elements[i].AddChild(
            new GUIButton("solid_minus_icon",
            action: () => showEditorDebug(component),
            layerMask: "GUI")
            {
                x = 83,
                y = 0,
                scale = new Vec2(0.8f, 0.8f)
            });
        }
        _componentList.Add(elements);
        void showEditorDebug(Component component)
        {
            if (component is Collider)
            {
                Settings.Setup.OnBeforeStep -= SelectedGameObject.Collider.ShowEditorDebug;

                foreach (GameObject child in SelectedGameObject.GetChildren())
                    Settings.Setup.OnBeforeStep -= child.Collider.ShowEditorDebug;
            }
            SelectedGameObject.RemoveComponent(component);
            RefreshComponents();
        }
    }
    public static void RefreshText()
    {
        _selectedName.Text(SelectedGameObject.name.Length > 11 ? SelectedGameObject.name.Substring(0, 9) + ".." : SelectedGameObject.name, clear: true);
        _selectedLayer.Text(SelectedGameObject.LayerMask.Length > 11 ? SelectedGameObject.LayerMask.Substring(0, 9) + ".." : SelectedGameObject.LayerMask, clear: true);
    }
    public static void SelectionBoxSetVisible(bool visible)
    {
        Setup.SelectionBox.visible = visible;
    }
}
