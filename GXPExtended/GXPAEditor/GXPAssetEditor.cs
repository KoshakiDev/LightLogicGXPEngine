using GXPEngine;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
public static class GXPAssetEditor
{
    [NonSerialized] public static Font EditorFont;
    [NonSerialized] private static Sprite _darkening;

    [NonSerialized] private static PolygonCollider _colliderBuffer;
    [NonSerialized] private static Vec2[] _colliderPointsBuffer;
    [NonSerialized] private static PropertyInfo _pointsInfo;
    [NonSerialized] private static int _selectedPointId = -1;
    [NonSerialized] private static Sprite _editColliderParent;

    public static void Start(string filename)
    {
        try{EditorFont = Utils.LoadFont(Settings.AssetsPath + "DOS.ttf", 12);}
        catch{EditorFont = EasyDraw.DefaultFont;}
        Settings.CreateEditorDebugDraw();
        GameObject documentObject = AssetManager.LoadAsset(filename, fullname: true);
        Setup.MainLayer.AddChild(documentObject);
        Selection.Filename = filename; 
        Selection.DocumentObject = documentObject;
        TryOutliningGameObject(documentObject, false, true);
    }
    public static void SubscribeEditor()
    {
        InputManager.OnRightMousePressed += ChangeSelection;
        InputManager.OnSaveCombination += Save;
        InputManager.OnAddCombination += Add;
        InputManager.OnMouseMoved += Transform;
        InputManager.OnDeleteButtonPressed += Delete;
    }
    public static void UnsubscribeEditor()
    {
        InputManager.OnRightMousePressed -= ChangeSelection;
        InputManager.OnSaveCombination -= Save;
        InputManager.OnAddCombination -= Add;
        InputManager.OnMouseMoved -= Transform;
        InputManager.OnDeleteButtonPressed -= Delete;
    }
    private static void OnSelected()
    {
        if (Selection.SelectedGameObject is null)
            return;

        Debug.Log(">> Selected : " + Selection.SelectedGameObject.name);

        TryOutliningGameObject(Selection.SelectedGameObject, true);
        Selection.SelectionBoxSetVisible(true);
    }
    private static void OnUnselected()
    {
        if (Selection.SelectedGameObject is null)
            return;

        Debug.Log(">> Deselected : " + Selection.SelectedGameObject.name);

        TryOutliningGameObject(Selection.SelectedGameObject, false);
        Selection.SelectionBoxSetVisible(false);
    }
    private static void Add()
    {
        Debug.Log(">> Started GXP Asset import");

        using (OpenFileDialog FileDialog = new OpenFileDialog())
        {
            FileDialog.InitialDirectory = Settings.AssetsPath;
            FileDialog.Filter = "GXP Assets (*.gxpa)|*.gxpa";
            FileDialog.RestoreDirectory = true;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                GameObject loadedAsset = AssetManager.LoadAsset(FileDialog.FileName, true);

                Selection.DocumentObject.AddChild(loadedAsset);
                TryOutliningGameObject(loadedAsset, false);
                loadedAsset.SetXY(Input.mouseWorldPosition.x, Input.mouseWorldPosition.y);

                Debug.Log(">> Imported GXP Asset import : " + FileDialog.FileName);
            }
        }

        ResetCamera();
    }
    private static void Save()
    {
        string shortenedFileName = Selection.Filename
            .Substring(0, Selection.Filename.LastIndexOf('.'))
            .Substring(Selection.Filename.LastIndexOf('\\') + 1);

        AssetManager.DeleteAsset(shortenedFileName);
        AssetManager.CreateAsset(shortenedFileName, Selection.DocumentObject);
        Debug.Log(">> Saved file");
    }
    private static void Delete()
    {
        if (Selection.SelectedGameObject is null)
            return;

        Debug.Log(">> Deleted gameObject : " + Selection.SelectedGameObject.name);

        if(Selection.SelectedGameObject == Selection.DocumentObject)
        {
            if (Selection.DocumentObject is Sprite sprite)
                sprite.ResetParameters("Empty");
            return;
        }

        Selection.SelectedGameObject.Destroy();
        Selection.SelectedGameObject = null;
    }
    private static void Transform()
    {
        if (Selection.SelectedGameObject is null)
            return;

        Selection.Transform
        (
            Input.GetKey(Key.LEFT_SHIFT) && !Input.GetKey(Key.LEFT_CTRL) && !Input.GetKey(Key.LEFT_ALT) ? InputManager.MouseDelta * 0.8f : Vec2.Zero,
            Input.GetKey(Key.LEFT_CTRL) ?
            (
                Input.GetKey(Key.LEFT_SHIFT)?
                new Vec2(InputManager.MouseDelta.y, InputManager.MouseDelta.y) * 0.01f :
                new Vec2(-InputManager.MouseDelta.x, InputManager.MouseDelta.y) * 0.01f
            ) 
            : Vec2.Zero,
            Input.GetKey(Key.LEFT_ALT) && !Input.GetKey(Key.LEFT_CTRL) ? InputManager.MouseDelta.y * 0.4f : 0
        );
    }
    private static void ChangeSelection()
    {
        Vec2 mousePosition = new Vec2(Input.mouseX, Input.mouseY);
        if (Input.mouseX < 192) return;
        foreach (GameObject gameObject in Setup.GUI.GetChildren())
            if (gameObject is Sprite sprite && sprite.visible && sprite.HitTest(mousePosition))
                return;

        GameObject currentGameObject = null, prevGameObject = Selection.SelectedGameObject;
        foreach (GameObject gameObject in Selection.DocumentObject.GetChildren())
        {
            if (gameObject != Selection.SelectedGameObject && gameObject is Sprite sprite && sprite.HitTest(mousePosition))
                currentGameObject = gameObject;
        }
        if (currentGameObject == null && Input.GetMouseButton(1))
        {
            prevGameObject = null;
            currentGameObject = Selection.DocumentObject;
        }
        OnUnselected();
        Selection.SelectedGameObject = currentGameObject;
        if (prevGameObject != Selection.SelectedGameObject)
            OnSelected();
    }
    private static void TryOutliningGameObject(GameObject gameObject, bool selected, bool withChildren = false)
    {
        if (gameObject is Sprite sprite)
            sprite.color = (uint)(selected ? Selection.SELECTED : Selection.UNSELECTED);

        editorDebug(gameObject);

        if (!withChildren)
            return;

        foreach (GameObject child in gameObject.GetChildren())
        {
            if (child is Sprite childSprite)
                childSprite.color = (uint)(selected ? Selection.SELECTED : Selection.UNSELECTED);

            editorDebug(child);
        }
        void editorDebug(GameObject debugObject)
        {
            if (debugObject.Collider is null)
                return;

            if (selected)
                Settings.Setup.OnBeforeStep += debugObject.Collider.ShowEditorDebug;
            else
                Settings.Setup.OnBeforeStep -= debugObject.Collider.ShowEditorDebug;
        }
    }
    private static void ResetCamera()
    {
        Camera.ClearFocuses();
        Camera.AddFocus(Setup.DocumentPointer);
        Camera.SetLevel(Setup.MainLayer);
    }
    public static void ChangeTexture()
    {
        if (Selection.SelectedGameObject is null || Selection.SelectedGameObject.GetType() != typeof(Sprite))
            return;

        Debug.Log(">> Started GXP Asset texture change");

        using (OpenFileDialog FileDialog = new OpenFileDialog())
        {
            FileDialog.InitialDirectory = Settings.AssetsPath;
            FileDialog.Filter = "Images (*.png)|*.png";
            FileDialog.RestoreDirectory = true;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                (Selection.SelectedGameObject as Sprite).ResetParameters(true, FileDialog.FileName);

                Debug.Log(">> Changed texture of GXP Asset : " + FileDialog.FileName);
            }
        }
    }
    public static void OpenComponentBox(Component component)
    {
        if (component is null)
            return;
        CloseComponentBox();
        UnsubscribeEditor();
        VerticalList fieldNameList, fieldChangeList, fieldChangeList2, fieldValueList, fieldValueList2;

        Setup.ComponentBox.visible = true;
        Setup.GUI.AddChild(
        _darkening = new Sprite("block")
        {
            scaleX = Settings.Setup.width,
            scaleY = Settings.Setup.height,
            color = 0x000000,
            alpha = 0.7f
        });
        Setup.GUI.SetChildIndex(_darkening, Setup.ComponentBox.Index);
        Setup.ComponentBox.AddChild(
        new ConstValueDisplayer<string>(component.GetType().Name, 380, 50)
        {
            x = 158,
            y = 38,
            color = 0xffbb00,
            font = EditorFont,
            VerticalTextAlign = CenterMode.Center
        });
        Setup.ComponentBox.AddChild(
        new ConstValueDisplayer<string>(component.Owner.name, 380, 50)
        {
            x = 125,
            y = 59,
            color = 0xffbb00,
            font = EditorFont,
            VerticalTextAlign = CenterMode.Center
        });
        Setup.ComponentBox.AddChild(
        new GUIButton("minus_icon", action: CloseComponentBox, layerMask: "GUI")
        {
            x = 382,
            y = 22,
        });
        Setup.ComponentBox.AddChild(
        fieldNameList = new VerticalList(21, layerMask: "GUI")
        {
            x = 27,
            y = 100,
        });
        Setup.ComponentBox.AddChild(
        fieldChangeList = new VerticalList(21, layerMask: "GUI")
        {
            x = 310,
            y = 140,
        });
        Setup.ComponentBox.AddChild(
        fieldChangeList2 = new VerticalList(21, layerMask: "GUI")
        {
            x = 380,
            y = 140,
        });
        Setup.ComponentBox.AddChild(
        fieldValueList = new VerticalList(21, layerMask: "GUI")
        {
            x = 242,
            y = 100,
        });
        Setup.ComponentBox.AddChild(
        fieldValueList2 = new VerticalList(21, layerMask: "GUI")
        {
            x = 310,
            y = 100,
        });
        if (typeof(PolygonCollider).IsAssignableFrom(component.GetType()))
        {
            fieldNameList.Add(new ConstValueDisplayer<string>("Collider points:", 320, 50)
            {
                color = 0xff0000,
                font = EditorFont,
            });
            fieldChangeList.Add(new GUIButton("element_base", () => OpenEditCollider(component as PolygonCollider), layerMask: "GUI"));
            fieldChangeList2.Add(new Sprite("Empty"));
            fieldValueList.Add(new ConstValueDisplayer<string>("Edit", 320, 50)
            {
                color = 0xffbb00,
                font = EditorFont,
            });
            fieldValueList2.Add(new Sprite("Empty"));
        }
        PropertyInfo[] properties = component.GetType().GetProperties();
        foreach (PropertyInfo property in properties)
        {
            if (property.PropertyType.IsGenericType 
                && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>) 
                || typeof(IEnumerable).IsAssignableFrom(property.PropertyType) 
                || property.PropertyType.GetInterfaces().Contains(typeof(ICollection)))
                continue;

            bool oneColumn = true;
            if (property.PropertyType == typeof(Vec2))
                oneColumn = false;

            fieldNameList.Add(new ConstValueDisplayer<string>(property.Name.Length > 22 ? property.Name.Substring(0, 20) + ".." : property.Name, 320, 50)
            {
                color = 0xff0000,
                font = EditorFont,
            });

            if (oneColumn)
            {
                fieldChangeList.Add(new GUIButton("element_base", () => new WaitForPropertyInput(property, component), layerMask: "GUI"));
                fieldChangeList2.Add(new Sprite("Empty"));
                fieldValueList.Add(new ValueDisplayer<string>(() => property.GetValue(component).ToString(), 320, 50)
                {
                    color = 0xffbb00,
                    font = EditorFont,
                });
                fieldValueList2.Add(new Sprite("Empty"));
            }
            else
            {
                Sprite column1, column2;
                fieldChangeList.Add(column1 = new GUIButton("element_base0,5", () => new WaitForPropertyInput(property, component, true, true), layerMask: "GUI"));
                fieldChangeList2.Add(column2 = new GUIButton("element_base0,5", () => new WaitForPropertyInput(property, component, true, false),layerMask: "GUI"));
                fieldValueList.Add(new ValueDisplayer<string>(() => "x:" + ((Vec2)property.GetValue(component)).x.ToString(), 320, 50)
                {
                    color = 0xffbb00,
                    font = EditorFont,
                });
                fieldValueList2.Add(new ValueDisplayer<string>(() => "y:" + ((Vec2)property.GetValue(component)).y.ToString(), 320, 50)
                {
                    color = 0xffbb00,
                    font = EditorFont,
                });
                column1.SetOrigin(68, column1.GetOrigin().y);
                column2.SetOrigin(69, column2.GetOrigin().y);
            }
        }
    }
    private static void CloseComponentBox()
    {
        if (Setup.ComponentBox.visible)
        {
            SubscribeEditor();
            _darkening.Destroy();
            Setup.ComponentBox.visible = false;
            Setup.ComponentBox.DestroyChildren();
        }
    }
    public static void OpenAddComponent(GameObject gameObject)
    {
        if (gameObject is null)
            return;
        CloseAddComponent();
        UnsubscribeEditor();
        VerticalList componentList;
        Setup.GUI.AddChild(
        _darkening = new Sprite("block")
        {
            scaleX = Settings.Setup.width,
            scaleY = Settings.Setup.height,
            color = 0x000000,
            alpha = 0.7f
        });
        Setup.GUI.SetChildIndex(_darkening, Setup.ComponentList.Index);
        Setup.ComponentList.visible = true;
        Setup.ComponentList.AddChild(
        new GUIButton("minus_icon", action: CloseAddComponent, layerMask: "GUI")
        {
            x = 296,
            y = 22,
        });
        Setup.ComponentList.AddChild(
        componentList = new VerticalList(21, layerMask: "GUI")
        {
            x = 20,
            y = 48,
        });
        Type[] componentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => typeof(Component).IsAssignableFrom(type)).ToArray();
        foreach (Type componentType in componentTypes)
        {
            gameObject.TryGetComponentsAssignableFrom(componentType, out Component[] components);
            if (componentType == typeof(Component) || components != null && components.Length > 0 || (typeof(Collider).IsAssignableFrom(componentType) && gameObject.Collider != null)) continue;
            GUIButton button = new GUIButton("component_base", action:() => createComponent(componentType), layerMask: "GUI");
            button.SetOrigin(0, 0);
            button.AddChild(new ConstValueDisplayer<string>(componentType.Name, 282, 32) 
            {
                y = -12,
                color = 0xffbb00,
                font = EditorFont,
                HorizontalTextAlign = CenterMode.Center
            });
            componentList.Add(button);
        }
        void createComponent(Type type)
        {
            gameObject.AddComponent(type);
            Selection.RefreshComponents();

            if (typeof(Collider).IsAssignableFrom(type) && gameObject == Selection.SelectedGameObject)
            {
                Settings.Setup.OnBeforeStep += gameObject.Collider.ShowEditorDebug;

                foreach (GameObject child in gameObject.GetChildren())
                    Settings.Setup.OnBeforeStep += child.Collider.ShowEditorDebug;
            }
            CloseAddComponent();
            OpenAddComponent(gameObject);
        }
    }
    private static void CloseAddComponent()
    {
        if (Setup.ComponentList.visible)
        {
            SubscribeEditor();
            _darkening.Destroy();
            Setup.ComponentList.visible = false;
            Setup.ComponentList.DestroyChildren();
        }
    }
    public static void OpenEditCollider(PolygonCollider polygonCollider)
    {
        CloseComponentBox();
        UnsubscribeEditor();

        _colliderBuffer = polygonCollider;
        _colliderPointsBuffer = new Vec2[_colliderBuffer.Points.Length];
        polygonCollider.Points.CopyTo(_colliderPointsBuffer, 0);
        _pointsInfo = typeof(PolygonCollider).GetProperty("Points", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        Setup.GUI.AddChild(_editColliderParent = new Sprite("Empty"));

        _editColliderParent.AddChild(new ConstValueDisplayer<string>(
            "[EDIT MODE] \n" +
            " To move points drag&drop them with LMB\n" +
            " To add point press 'RMB'\n" +
            " To remove point press 'Ctrl+RMB'\n" +
            " To apply changes press 'Enter'\n" +
            " To discard changes press 'Backspace'", 400, 400)
        {
            x = 200,
            y = 290,
            color = 0xff0000,
            font = EditorFont,
        });
        Settings.Setup.OnBeforeStep += UpdateEditMode;
    }
    private static void CloseEditCollider(bool result)
    {
        _editColliderParent.Destroy();
        if (!result)
        {
            try {_pointsInfo.SetValue(_colliderBuffer, _colliderPointsBuffer);}
            catch {}
        }
        SubscribeEditor();
        Settings.Setup.OnBeforeStep -= UpdateEditMode;
    }
    private static void UpdateEditMode()
    {
        if (Input.GetKeyDown(Key.BACKSPACE))
            CloseEditCollider(false);
        else if (Input.GetKeyDown(Key.ENTER))
            CloseEditCollider(true);
        else if (Input.GetMouseButtonDown(1))
        {
            if(Input.GetKey(Key.LEFT_CTRL)) 
            try
            {
                if (_colliderBuffer.Points.Length <= 2)
                    return;
                Vec2[] enlargedPointBuffer = new Vec2[_colliderBuffer.Points.Length - 1];
                    for (int i = 0; i < enlargedPointBuffer.Length; i++)
                        enlargedPointBuffer[i] = _colliderBuffer.Points[i];
                _pointsInfo.SetValue(_colliderBuffer, enlargedPointBuffer);
            }
            catch { }
            else try
            {
                Vec2[] enlargedPointBuffer = new Vec2[_colliderBuffer.Points.Length + 1];
                _colliderBuffer.Points.CopyTo(enlargedPointBuffer, 0);
                Vec2 relativeMouse = Input.mouseWorldPosition - _colliderBuffer.Owner.position;
                relativeMouse.RotateDegrees(-_colliderBuffer.Owner.rotation);
                relativeMouse *= new Vec2(_colliderBuffer.Owner.scaleX, _colliderBuffer.Owner.scaleY) ^ -1;
                enlargedPointBuffer[enlargedPointBuffer.Length - 1] = relativeMouse;
                _pointsInfo.SetValue(_colliderBuffer, enlargedPointBuffer);
            }
            catch { }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (_selectedPointId != -1)
            {
                _selectedPointId = -1;
                return;
            }
            float minDistance = float.MaxValue;
            int pointId = 0;
            for (int i = 0; i < _colliderBuffer.Points.Length; i++)
            {
                float distance = Vec2.Distance(_colliderBuffer.TransformedPoints[i], Input.mouseWorldPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    pointId = i;
                }
            }
            if (minDistance > 20)
                return;
            else
                _selectedPointId = pointId;
        }
        else 
        {
            if (_selectedPointId == -1)
                return;

            Vec2 relativeDelta = InputManager.MouseDelta;
            relativeDelta.RotateDegrees(_colliderBuffer.Owner.rotation);
            relativeDelta *= new Vec2(_colliderBuffer.Owner.scaleX, _colliderBuffer.Owner.scaleY) ^ -1;
            _colliderBuffer.Points[_selectedPointId] += relativeDelta.inverted * 0.5f;
        }
    }
}
