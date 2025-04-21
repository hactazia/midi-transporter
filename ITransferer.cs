
using UdonSharp;
using VRC.SDK3.Data;

public class ITransferer : UdonSharpBehaviour
{
    public virtual void OnStart(MidiReceptor receptor) { }
    public virtual void OnEvent(MidiReceptor receptor, string eventName, string state = null, params DataToken[] args) { }
    public virtual void OnJSON(MidiReceptor receptor, DataToken dictionary) { }
    public virtual void OnMessage(MidiReceptor receptor, string message) { }
    public virtual void OnByte(MidiReceptor receptor, byte b) { }

}
