using UdonSharp;
using UnityEngine.Serialization;

namespace Hactazia.MidiTransporter
{
    public class BaseTransporter : UdonSharpBehaviour
    {
        public MidiReceptor receptor;

        public virtual void Start()
        {
            receptor.AddListener(this);
        }

        public virtual void OnDestroy()
        {
            receptor.RemoveListener(this);
        }

        public virtual void OnTransporterAdded(MidiReceptor recep, BaseTransporter transporter)
        {
        }

        public virtual void OnTransporterRemoved(MidiReceptor recep, BaseTransporter transporter)
        {
        }

        public virtual void OnByte(MidiReceptor recep, byte b)
        {
        }

        public virtual void OnReceived(MidiReceptor recep, byte[] data)
        {
        }

        public virtual void OnBuffer(MidiReceptor recep, byte[] buffer)
        {
        }

        /// <summary>
        /// Send a raw buffer to the receptor.
        /// </summary>
        /// <param name="recep"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool Send(MidiReceptor recep, byte[] buffer)
        {
            recep.SendRaw(buffer);
            return true;
        }
    }
}