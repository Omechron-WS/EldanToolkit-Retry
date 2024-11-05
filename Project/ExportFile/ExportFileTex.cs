using Godot;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class ExportFileTex : ExportFile
{
    [JsonProperty]
    public string SourceFile;

    [JsonProperty]
    public bool OverrideAlpha = false;

    [JsonProperty]
    public byte AlphaValue = 0;

    [JsonProperty]
    public int MipMaps = 1;

    public override void AddOptions(Container container)
    {
        AddCheckbox(container, "Override Alpha", (bool val) => OverrideAlpha = val, OverrideAlpha);
        AddNumberBox(container, "Override Alpha Value", (float val) => AlphaValue = (byte)val, AlphaValue, true);
        AddNumberBox(container, "MipMaps", (float val) => MipMaps = (byte)val, MipMaps, true);
    }
}