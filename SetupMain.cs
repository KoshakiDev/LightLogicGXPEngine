using GXPEngine;
using System;
using System.Windows.Forms;

[Serializable]
public class Setup : Game
{
    //-------------------------------- GRAPHICAL - LAYERS ----------------------------------//
    public static Sprite MainLayer, CollisionDebug, PostProcessing, EditorCollisionDebug, GUI;

    //--------------------------------- GXPA - EDITOR --------------------------------//
    public static GameObject DocumentPointer, SelectionBox, ComponentBox, ComponentList;

    [STAThread] private static void Main() => new Setup();

    public Setup() : base(1280, 720, false, pPixelArt: false)
    {
        void settings()
        {
            Settings.Setup = this;
            Settings.RefreshAssetsPath();
            Settings.ReadParameters();
            Settings.Volume = 0.8f;
            Settings.Fullscreen = false;
            Settings.CollisionDebug = true;
            Settings.CollisionPrecision = 0;
            Settings.ComponentRegistrationBlock = false;
            Settings.RaycastStep = 100;
        }
        void subscriptions()
        {
            SoundManager.Init();
            OnBeforeStep += InputManager.ListenToInput;
            OnBeforeStep += Camera.Interpolate;
            AssetManager.UpdateRegistry();
        }
        void layers()
        {
            Physics.AddLayers(new string[]
            {
                "Default",
                "Bullets",
                "Creatures",
                "Walls",
                "Mirrors",
                "GUI",
            });
        }
        void unitTests()
        {
            Debug.Log("\n-----Unit-tests-----\n");
            Vec2 myVec = new Vec2(2, 3);
            Vec2 result = myVec * 3;
            Debug.Log("Scalar multiplication right ok ?: " +
             (result.x == 6 && result.y == 9 && myVec.x == 2 && myVec.y == 3));
            myVec = new Vec2(2, 3);
            result = 4 * myVec;
            Debug.Log("Scalar multiplication left ok ?: " +
             (result.x == 8 && result.y == 12 && myVec.x == 2 && myVec.y == 3));
            myVec = new Vec2(2, 3);
            Vec2 other = new Vec2(2, 3);
            result = other * myVec;
            Debug.Log("Vector multiplication ok ?: " +
             (result.x == 4 && result.y == 9 && myVec.x == 2 && myVec.y == 3));

            myVec = new Vec2(2, 3);
            other = new Vec2(1, -1);
            result = myVec + other;
            Debug.Log("Vector addition ok ?: " +
             (result.x == 3 && result.y == 2 && myVec.x == 2 && myVec.y == 3));
            myVec = new Vec2(2, 3);
            result = myVec + 4;
            Debug.Log("Addition right ok ?: " +
             (result.x == 6 && result.y == 7 && myVec.x == 2 && myVec.y == 3));
            myVec = new Vec2(2, 3);
            result = 4 + myVec;
            Debug.Log("Addition left ok ?: " +
             (result.x == 6 && result.y == 7 && myVec.x == 2 && myVec.y == 3));

            myVec = new Vec2(2, 3);
            other = new Vec2(1, -1);
            result = myVec - other;
            Debug.Log("Vector subtraction ok ?: " +
             (result.x == 1 && result.y == 4 && myVec.x == 2 && myVec.y == 3));
            myVec = new Vec2(2, 3);
            result = myVec - 4;
            Debug.Log("Subtraction right ok ?: " +
             (result.x == -2 && result.y == -1 && myVec.x == 2 && myVec.y == 3));
            myVec = new Vec2(2, 3);
            result = 4 - myVec;
            Debug.Log("Subtraction left ok ?: " +
             (result.x == 2 && result.y == 1 && myVec.x == 2 && myVec.y == 3));

            myVec = new Vec2(2, 3);
            other = new Vec2(2, 2);
            result = myVec ^ other;
            Debug.Log("Vector power ok ?: " +
             (result.x == 4 && result.y == 9 && myVec.x == 2 && myVec.y == 3));
            myVec = new Vec2(2, 3);
            result = myVec ^ 2;
            Debug.Log("Power right ok ?: " +
             (result.x == 4 && result.y == 9 && myVec.x == 2 && myVec.y == 3));
            myVec = new Vec2(2, 3);
            result = 2 ^ myVec;
            Debug.Log("Power left ok ?: " +
             (result.x == 4 && result.y == 8 && myVec.x == 2 && myVec.y == 3));

            myVec = new Vec2(2, 3);
            other = new Vec2(-5, 2);
            float floatResult = Vec2.DotSecondNormalized(myVec, other);
            Debug.Log("Dot product ok ?: " +
                (floatResult == -4 && myVec.x == 2 && myVec.y == 3 && other.x == -5 && other.y == 2));

            myVec = new Vec2(2, 3);
            other = new Vec2(1, 1);
            result = Vec2.Lerp(myVec, other, 0.5f);
            Debug.Log("Lerp ok?: " +
                (result.x == 1.5 && result.y == 2 && myVec.x == 2 && myVec.y == 3));

            myVec = new Vec2(3, 4);
            floatResult = myVec.length;
            Debug.Log("Lenght ok?: " +
                (floatResult == 5 && myVec.x == 3 && myVec.y == 4));

        }

        settings();
        subscriptions();
        layers();
        unitTests();
        Start();
    }
    private void Awake()
    {
        Physics.Start();
        Debug.Log("\n-----Awake-----\n");

        #region LayerInit
        AddChild(MainLayer = new Sprite("Empty"));
        AddChild(CollisionDebug = new Sprite("Empty"));
        AddChild(PostProcessing = new Sprite("Empty"));
        AddChild(EditorCollisionDebug = new Sprite("Empty"));
        AddChild(GUI = new Sprite("Empty"));

        MainLayer.AddChild(DocumentPointer = AssetManager.LoadAsset("pointer"));
        #endregion

        #region Post-processing

        PostProcessing.AddChildren(new GameObject[]
        {
            //AssetManager.LoadAsset("screenEffect_MULT"),
            //AssetManager.LoadAsset("screenEffect_Add")
        });

        #endregion

        #region GXP Asset Editor
        string[] args = Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            OpenEditor(args[1]);
            LightLogicGame.InEditor = true;
            return;
        }
        #endregion

        Camera.AddFocus(DocumentPointer);
        Camera.SetLevel(MainLayer);
        LightLogicGame.Start();
        Debug.Log("\n-----Start-----\n");
    }
    public static void OpenEditor(string document)
    {
        GUI.DestroyChildren();

        EditorCollisionDebug.AddChild(AssetManager.LoadAsset("editor"));
        GUI.AddChild(SelectionBox = new Sprite("selection_box", layerMask: "GUI")
        {
            x = 7,
            y = 240,
            visible = false,
        });
        GUI.AddChild(ComponentBox = new Sprite("component_box", layerMask: "GUI")
        {
            x = 500,
            y = 100,
            visible = false,
        });
        GUI.AddChild(ComponentList = new Sprite("component_list", layerMask: "GUI")
        {
            x = 600,
            y = 100,
            visible = false,
        });

        Camera.SetLevel(MainLayer);
        Camera.AddFocus(DocumentPointer);
        Camera.SetFactor(0.1f);
        Settings.ComponentRegistrationBlock = false;

        GXPAssetEditor.Start(document);
        GXPAssetEditor.SubscribeEditor();
        Debug.Log("\n-----Start-----\n");
    }
}

public static class LightLogicGame
{
    private static string _currentLevel;
    public static bool InEditor = false;
    public static void Start()
    {
        OpenMenu();
    }
    public static void GameOver()
    {
        Setup.GUI.AddChildren(new GameObject[]
        {
            new Sprite("game_over") { x = 0, y = 0, scaleX = 0.67f, scaleY = 0.67f },
            new Sprite("Empty"), new Sprite("Empty"), new Sprite("Empty"), new Sprite("Empty"),
            new GUIButton("btn_return", OpenMenu ) { x = 406, y = 660, scaleX = 0.67f, scaleY = 0.67f },
            new GUIButton("btn_again", () => OpenLevel(_currentLevel)) { x = Settings.Setup.width - 416, y = 660, scaleX = 0.67f, scaleY = 0.67f },
        });
    }
    public static void OpenMenu()
    {
        Setup.MainLayer.DestroyChildren();
        Setup.GUI.DestroyChildren();

        Setup.GUI.AddChildren(new GameObject[]
        {
            new GUIButton("Endurance") { x = 206, y = 360, scaleX = 0.67f, scaleY = 0.67f },
            new GUIButton("Sandbox", ChooseAsset ) { x = Settings.Setup.width - 216, y = 360, scaleX = 0.67f, scaleY = 0.67f },
            new GUIButton("Story", () => OpenLevel("Level 1")) { x = 635, y = 360, scaleX = 0.67f, scaleY = 0.67f },
        });
    }
    public static void OpenLevel(string name)
    {
        Setup.MainLayer.DestroyChildren();
        Setup.GUI.DestroyChildren();

        Setup.MainLayer.AddChild(AssetManager.LoadAsset(name));
        _currentLevel = name;
    }
    public static void ChooseAsset()
    {
        Debug.Log(">> Started GXP Asset load");

        using (OpenFileDialog FileDialog = new OpenFileDialog())
        {
            FileDialog.InitialDirectory = Settings.AssetsPath;
            FileDialog.Filter = "GXP Assets (*.gxpa)|*.gxpa";
            FileDialog.RestoreDirectory = true;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                InEditor = true;
                Setup.OpenEditor(FileDialog.FileName);
                Debug.Log(">> Opened GXP Asset import : " + FileDialog.FileName);
            }
        }
    }
    public static void FinishLevel()
    {

    }
}