using Hactazia.Types;
using TMPro;
using UnityEngine;

namespace Hactazia.MidiTransporter.Examples
{
    public class DebugExample : BaseTransporter
    {
        public TextMeshProUGUI textMessage;

        public override void OnBuffer(MidiReceptor recep, byte[] buffer)
        {
            base.OnBuffer(recep, buffer);
            textMessage.text = buffer.ToBufferString();
        }
    }
}