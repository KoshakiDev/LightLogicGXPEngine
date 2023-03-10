using GXPEngine;
using System.Drawing;

public partial class Setup : Game
{
    public delegate void OnGameUpdateHandler();
    public static event OnGameUpdateHandler OnGameUpdate;
    public static Sprite postProcessing;
    private void Update() => OnGameUpdate.Invoke();
    private static void Main() => new Setup();
    public Setup() : base(800, 600, false, pPixelArt: true, pRealWidth: Settings.Screen.Width, pRealHeight: Settings.Screen.Height)
    {
        void settings()
        {
            Settings.RefreshAssetsPath();
            Settings.Validate();
            Settings.Volume = 0.8f;
            Settings.Fullscreen = false;
            SoundManager.Init();
        }
        void subscriptions()
        {        
            OnGameUpdate += InputManager.ListenToInput;
            OnGameUpdate += FirstLoad;
            OnGameUpdate += () => SetChildIndex(postProcessing, 1000);
            InputManager.Start();
        }
        void unitTests()
        {
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
            float floatResult = myVec.Dot();
            Debug.Log("Dot product ok ?: " +
                (floatResult == 13 && myVec.x == 2 && myVec.y == 3));

            myVec = new Vec2(2, 3);
            other = new Vec2(1, 1);
            result = Vec2.Lerp(myVec, other, 0.5f);
            Debug.Log("Lerp ok?: " +
                (result.x == 1.5 && result.y == 2 && myVec.x == 2 && myVec.y == 3));

            myVec = new Vec2(3, 4);
            floatResult = myVec.length;
            Debug.Log("Lenght ok?: " +
                (floatResult == 5 && myVec.x == 3 && myVec.y == 4));


            //normalize, normalized, setxy, tostring, setangledegrees, setangleradians, angleindeg, angleinrad
        }

        settings();
        subscriptions();
        unitTests();
        Start();
    }
    private void FirstLoad()
    {
        #region Setup level

        #region Player
        Sprite player = new Sprite("Circle");
        player.SetXY(width / 2, height / 2);
        AddChild(player);
        #endregion

        #endregion

        #region Post-processing
        AddChild(postProcessing = new Sprite("Empty"));
        Sprite screenEffect = new Sprite("ScreenEffect");
        screenEffect.blendMode = BlendMode.ADDITIVE;
        postProcessing.AddChildren(new GameObject[]
        {
            screenEffect,
        });
        #endregion

        OnGameUpdate -= FirstLoad;
    }
}