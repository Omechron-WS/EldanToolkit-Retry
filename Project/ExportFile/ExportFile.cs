using Godot;
using Newtonsoft.Json;
using System;

[JsonObject(MemberSerialization.OptIn)]
public abstract class ExportFile
{
    public string ExportFilePath;

    public abstract void AddOptions(Container container);

    public static void AddOptionsNonExportFile(Container container, string filePath)
    {

    }

    public static void AddCheckbox(Control ctrl, string text, Action<bool> callback, bool state)
    {
        //HBoxContainer hb = new HBoxContainer();
        //ctrl.AddChild(hb);
        CheckBox cb = new CheckBox();
        cb.ButtonPressed = state;
        //hb.AddChild(cb);
        ctrl.AddChild(cb);
        cb.Text = text;
        cb.Connect(CheckBox.SignalName.Toggled, Callable.From(callback));
    }

    public static void AddNumberBox(Control ctrl, string text, Action<float> callback, float value, bool rounded)
    {
        HBoxContainer hb = new HBoxContainer();
        ctrl.AddChild(hb);

        Label lbl = new Label();
        lbl.Text = text;
        hb.AddChild(lbl);
        
        SpinBox sb = new SpinBox();
        sb.Value = value;
        sb.Rounded = rounded;
        sb.Connect(SpinBox.SignalName.ValueChanged, Callable.From(callback));
        hb.AddChild(sb);
    }

    public void Save()
    {
        string output = JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}