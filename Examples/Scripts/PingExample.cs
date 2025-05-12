using Hactazia.MidiTransporter.Protocol;
using VRC.SDK3.Data;

namespace Hactazia.MidiTransporter.Examples
{
    public class PingExample : AddonTransporter
    {
        public override void OnEvent(MidiReceptor recep, string eventName, string state, params DataToken[] args)
        {
            base.OnEvent(recep, eventName, state, args);
            if (eventName != "ping") return;
            Send(recep, "pong", state, args);
        }
    }
}