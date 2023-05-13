using System.Reflection;
using System;

public class WaitForPropertyInput : WaitForInput
{
    private PropertyInfo _property;
    public WaitForPropertyInput(PropertyInfo property, object obj, bool isVector2 = false, bool isX = true, bool inQueue = true)
    {
        if (_instanceCount > 0)
            return;

        if (!inQueue)
            GXPAssetEditor.UnsubscribeEditor();

        Text = !isVector2 ? property.GetValue(obj).ToString()
            : (isX ? ((Vec2)property.GetValue(obj)).x.ToString()
            : ((Vec2)property.GetValue(obj)).y.ToString());
        _obj = obj;
        _property = property;
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
                _property.SetValue
                (
                    _obj,
                    !_isVector2 ?
                    Convert.ChangeType(_input.Text, _property.PropertyType) :
                    (
                        _isX ?
                        new Vec2(float.Parse(_input.Text), ((Vec2)_property.GetValue(_obj)).y) :
                        new Vec2(((Vec2)_property.GetValue(_obj)).x, float.Parse(_input.Text))
                    )
                );
            }
            catch
            {
                Debug.Log(">> Property wasn't changed due to the invalid given value");
            }

        _instanceCount--;
        if (_instanceCount == 0 && !Setup.ComponentBox.visible)
            GXPAssetEditor.SubscribeEditor();
        _base.Destroy();
        _back.Destroy();
        base.Finish(result);
    }
}
