using GXPEngine;

public static class InputManager
{
    public delegate void OnUpButtonPressedHandler();
    public static OnUpButtonPressedHandler OnUpButtonPressed;

    public delegate void OnDownButtonPressedHandler();
    public static OnDownButtonPressedHandler OnDownButtonPressed;

    public delegate void OnRightButtonPressedHandler();
    public static OnRightButtonPressedHandler OnRightButtonPressed;

    public delegate void OnLeftButtonPressedHandler();
    public static OnLeftButtonPressedHandler OnLeftButtonPressed;

    public delegate void OnSpaceButtonPressedHandler();
    public static OnSpaceButtonPressedHandler OnSpaceButtonPressed;

    public delegate void OnAnyButtonPressedHandler();
    public static OnAnyButtonPressedHandler OnAnyButtonPressed;

    public static void Start()
    {
        SubscribeAnyButtonPressed();
        OnAnyButtonPressed += OnAnyButtonPressedNull;

        void OnAnyButtonPressedNull() { }
        void SubscribeAnyButtonPressed()
        {
            OnDownButtonPressed += () => OnAnyButtonPressed.Invoke();
            OnUpButtonPressed += () => OnAnyButtonPressed.Invoke();
            OnLeftButtonPressed += () => OnAnyButtonPressed.Invoke();
            OnRightButtonPressed+= () => OnAnyButtonPressed.Invoke();
            OnSpaceButtonPressed += () => OnAnyButtonPressed.Invoke();
        }
    }
    public static void ListenToInput()
    {
        if (Input.GetKey(Key.S) && OnUpButtonPressed != null) OnUpButtonPressed.Invoke();
        if (Input.GetKey(Key.W) && OnDownButtonPressed != null) OnDownButtonPressed.Invoke();
        if (Input.GetKey(Key.A) && OnLeftButtonPressed != null) OnLeftButtonPressed.Invoke();
        if (Input.GetKey(Key.D) && OnRightButtonPressed != null) OnRightButtonPressed.Invoke();
        if (Input.GetKeyDown(Key.SPACE) && OnSpaceButtonPressed != null) OnSpaceButtonPressed.Invoke();
    }

}