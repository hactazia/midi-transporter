using Hactazia.Types;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Midi;
using Convert = System.Convert;

namespace Hactazia.MidiTransporter
{
    public class MidiReceptor : UdonSharpBehaviour
    {
        public const byte DefaultChannel = 10;
        public const MidiEvents DefaultEvents = MidiEvents.ControlChange;

        public byte listenChannel = DefaultChannel;
        public MidiEvents listenEvents = DefaultEvents;

        private BaseTransporter[] _transporters = ListExtensions.New<BaseTransporter>();

        public void AddListener(BaseTransporter transporter)
        {
            Debug.Log($"Adding listener {transporter}");
            _transporters = _transporters.AddIfNotExists(transporter);
            foreach (var tr in _transporters)
                tr.OnTransporterAdded(this, transporter);
        }

        public void RemoveListener(BaseTransporter transporter)
        {
            Debug.Log($"Removing listener {transporter}");
            _transporters = _transporters.RemoveMany(transporter);
            foreach (var tr in _transporters)
                tr.OnTransporterRemoved(this, transporter);
            transporter.OnTransporterRemoved(this, transporter);
        }

        [HideInInspector] public byte[] received = new byte[byte.MaxValue];
        [HideInInspector] public byte receivedIndex = 0;

#if UNITY_EDITOR
        [HideInInspector] public byte mEditorMessageIndex = byte.MinValue;
        [HideInInspector] public string[] mEditorMessages = new string[byte.MaxValue];
#endif

        void Awake()
        {
            received = new byte[byte.MaxValue];
            receivedIndex = 0;
#if UNITY_EDITOR
            mEditorMessageIndex = byte.MinValue;
            mEditorMessages = new string[byte.MaxValue];
#endif
        }

        /// <summary>
        /// Called when VRCMidiReceptor receives a CC message.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="number"></param>
        /// <param name="velocity"></param>
        public override void MidiControlChange(int channel, int number, int velocity)
        {
            if (channel != listenChannel || !FlagExtensions.HasFlag(listenEvents, MidiEvents.ControlChange))
                return;
            OnByte((byte)number);
            if (number != 0)
                OnByte((byte)velocity);
        }

        public override void MidiNoteOn(int channel, int number, int velocity)
        {
            if (channel != listenChannel || !FlagExtensions.HasFlag(listenEvents, MidiEvents.NoteOn))
                return;
            OnByte((byte)number);
            if (number != 0)
                OnByte((byte)velocity);
        }

        public override void MidiNoteOff(int channel, int number, int velocity)
        {
            if (channel != listenChannel || !FlagExtensions.HasFlag(listenEvents, MidiEvents.NoteOff))
                return;
            OnByte((byte)number);
            if (number != 0)
                OnByte((byte)velocity);
        }


        /// <summary>
        /// When a CC message is received, the two int (number and velocity) are sent as bytes [0-128[.
        /// </summary>
        /// <param name="b"></param>
        private void OnByte(byte b)
        {
            foreach (var transfer in _transporters)
                transfer.OnByte(this, b);

            if (b == 0)
            {
                if (receivedIndex == 0) return;
                var buffer = new byte[receivedIndex];
                for (var i = 0; i < receivedIndex; i++)
                    buffer[i] = received[i];
                OnReceivedData(buffer);
                receivedIndex = 0;
                return;
            }


            received[receivedIndex++] = b;
        }

        private void OnReceivedData(byte[] receive)
        {
            foreach (var transfer in _transporters)
                transfer.OnReceived(this, receive);

            var base64 = System.Text.Encoding.ASCII.GetString(receive);
            var bytes = Convert.FromBase64String(base64);
            
            foreach (var transfer in _transporters)
                transfer.OnBuffer(this, bytes);
        }

        public void SendRaw(byte[] buffer)
        {
            var base64 = $"hmt:{Convert.ToBase64String(buffer)}";
#if UNITY_EDITOR
            mEditorMessageIndex = (byte)((mEditorMessageIndex + 1) % byte.MaxValue);
            mEditorMessages[mEditorMessageIndex] = base64;
#else
            Debug.Log(base64);
#endif
        }
    }
}