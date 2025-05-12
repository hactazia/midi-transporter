using UdonSharp;

namespace Hactazia.MidiTransporter
{
    public class BaseTransporter : UdonSharpBehaviour
    {
        public virtual void OnStart(MidiReceptor receptor)
        {
        }

        public virtual void OnByte(MidiReceptor receptor, byte b)
        {
        }

        public virtual void OnReceived(MidiReceptor receptor, byte[] data)
        {
        }

        public virtual void OnBuffer(MidiReceptor receptor, byte[] buffer)
        {
        }
    }
}