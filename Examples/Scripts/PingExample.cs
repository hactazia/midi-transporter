using UnityEngine;
using VRC.SDK3.Data;

public class PingExample : ITransferer
{
    public override void OnEvent(MidiReceptor receptor, string eventName, string state = null, params DataToken[] args)
    {
        if (eventName != "ping" || string.IsNullOrEmpty(eventName)) return;
        receptor.SendEvent("pong", state, args);
    }
}