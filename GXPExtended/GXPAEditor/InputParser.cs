using GXPEngine;
using System.Windows.Forms;

public class InputParser
{
    public string Text;
    private WaitForInput _wait;
    public InputParser(WaitForInput wait)
    {
        _wait = wait;
        Text = wait.Text;
    }
    public void ListenInput()
    {
        if (Input.GetKeyDown(Key.BACKSPACE))
            Backspace();
        else if (Input.GetKeyDown(Key.ENTER) || Input.GetKeyDown(Key.NUMPAD_ENTER))
            _wait.Finish(true);
        else if (Input.GetKeyDown(Key.ESCAPE))
            _wait.Finish(false);
        else if (Input.AnyKeyDown())
            Add(Input.GetKeys());
    }
    public void Add(int charId)
    {
        switch (charId)
        {
            case Key.CAPS_LOCK:
            case Key.TAB:
            case Key.LEFT_SHIFT:
            case Key.RIGHT_SHIFT:
            case Key.LEFT_CTRL:
            case Key.RIGHT_CTRL:
                return;
        }
        Text += Control.IsKeyLocked(Keys.CapsLock) ? char.ConvertFromUtf32(charId).ToUpper() : char.ConvertFromUtf32(charId).ToLower();
    }
    public void Add(int[] chars)
    {
        foreach (int charId in chars)
            Add(charId);
    }
    public void Backspace()
    {
        Text = Text.Length > 0 ? Text.Substring(0, Text.Length - 1) : Text;
    }
}
