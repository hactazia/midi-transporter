using UnityEngine;
using TMPro;
using VRC.SDK3.Data;
using BestHTTP.JSON;

public class DebugExample : ITransferer
{
    public TextMeshProUGUI textMessage;
    public TextMeshProUGUI textByte;

    public override void OnMessage(MidiReceptor receptor, string message)
    {
        if (textMessage != null)
            textMessage.text = message;
    }

    public override void OnJSON(MidiReceptor receptor, DataToken json)
    {
        if (textMessage != null
            && json.TokenType == TokenType.DataDictionary
            && VRCJson.TrySerializeToJson(json.DataDictionary, JsonExportType.Beautify, out DataToken jsonStr))
            textMessage.text = jsonStr.String;
    }
    public override void OnByte(MidiReceptor receptor, byte b)
    {
        if (textByte != null)
            textByte.text = b.ToString();
    }
}