using GXPEngine;

public class WaitForInput
{
    protected static int _instanceCount = 0;
    public string Text;

    protected object _obj;
    protected Sprite _base, _back;
    protected ValueDisplayer<string> _foreground;
    protected InputParser _input;
    protected bool _isX;
    protected bool _isVector2;

    protected void Init()
    {
        Setup.GUI.AddChild(
       _back = new Sprite("block")
       {
           scaleX = Settings.Setup.width,
           scaleY = Settings.Setup.height,
           color = 0x000000,
           alpha = 0.7f
       });
        Setup.GUI.AddChild(
        _base = new Sprite("value_entry")
        {
            x = 540 + _instanceCount * 30,
            y = 320 + _instanceCount * 30
        });
        _base.AddChild(
        _foreground = new ValueDisplayer<string>(() => _input.Text, 285, 30)
        {
            x = 22,
            y = 45,
            color = 0xffbb00,
            font = GXPAssetEditor.EditorFont,
            HorizontalTextAlign = CenterMode.Center
        });
        _base.AddChild(
        new ConstValueDisplayer<string>("<press 'ENTER' to save value>", 285, 30)
        {
            x = 22,
            y = 70,
            color = 0xff0000,
            font = GXPAssetEditor.EditorFont,
            HorizontalTextAlign = CenterMode.Center
        });
        _base.AddChild(
        new GUIButton("minus_icon", action: () => Finish(false), layerMask: "GUI")
        {
            x = 308,
            y = 21,
        });
    }
    public virtual void Finish(bool result) => Selection.RefreshText();
}
