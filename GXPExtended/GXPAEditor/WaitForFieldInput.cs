using System.Reflection;
using System;

public class WaitForFieldInput : WaitForInput
{
    private FieldInfo _field;
    public WaitForFieldInput(FieldInfo field, object obj, bool isVector2 = false, bool isX = true, bool inQueue = true)
    {
        if (_instanceCount > 0)
            return;

        if(!inQueue)
            GXPAssetEditor.UnsubscribeEditor();

        Text = !isVector2 ? field.GetValue(obj).ToString()
            : (isX ? ((Vec2)field.GetValue(obj)).x.ToString()
            : ((Vec2)field.GetValue(obj)).y.ToString());
        _obj = obj;
        _field = field;
        _isX = isX;
        _isVector2 = isVector2;

        _input = new InputParser(this);
        Settings.Setup.OnBeforeStep += _input.ListenInput;
        _instanceCount++;

        Init();
    }
    public override void Finish(bool result)
    {
        Settings.Setup.OnBeforeStep -= _input.ListenInput;
        if (result)
            try
            {
                _field.SetValue
                (
                    _obj,
                    !_isVector2 ?
                    Convert.ChangeType(Text, _field.FieldType) :
                    (
                        _isX ?
                        new Vec2(float.Parse(Text), ((Vec2)_field.GetValue(_obj)).y) :
                        new Vec2(((Vec2)_field.GetValue(_obj)).x, float.Parse(Text))
                    )
                );
            }
            catch
            {
                Debug.Log(">> Field wasn't changed due to the invalid given value");
            }
        _instanceCount--;
        if (_instanceCount == 0 && !Setup.ComponentBox.visible)
            GXPAssetEditor.SubscribeEditor();
        _base.Destroy();
        _back.Destroy();
        base.Finish(result);
    }
}
