using System;
using Hactazia.Types;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using Convert = System.Convert;

namespace Hactazia.MidiTransporter
{
    public class MidiReceptor : UdonSharpBehaviour
    {
        public int listenChannel = 10;
        private (ushort, byte[]) _received;
        public ITransferer[] transfers;

#if UNITY_EDITOR
        public byte mEditorMessageIndex;
        public string[] mEditorMessages;
#endif

        void Start()
        {
#if UNITY_EDITOR
            mEditorMessageIndex = byte.MinValue;
            mEditorMessages = new string[byte.MaxValue];
#endif
            _received = (0, new byte[ushort.MaxValue]);
            foreach (var transfer in transfers)
                transfer.OnStart(this);
        }

        /// <summary>
        /// Called when VRCMidiReceptor receives a CC message.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="number"></param>
        /// <param name="velocity"></param>
        public override void MidiControlChange(int channel, int number, int velocity)
        {
            if (channel != listenChannel) return;
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
            foreach (var transfer in transfers)
                transfer.OnByte(this, b);

            if (b == 0)
            {
                OnReceivedData(_received.Item2);
                _received.Item1 = 0;
            }

            _received.Item2[_received.Item1++] = b;
        }

        private void OnReceivedData(byte[] receive)
        {
            foreach (var transfer in transfers)
                transfer.OnReceived(this, receive);
            var buffer = Types.Convert.FromBase128ToBase256(receive);
            foreach (var transfer in transfers)
                transfer.OnBuffer(this, buffer);
        }

        public void Send(byte[] buffer)
        {
            var base64 = $"hmt:{Convert.ToBase64String(buffer)}";
#if UNITY_EDITOR
            mEditorMessageIndex = (byte)((mEditorMessageIndex + 1) % byte.MaxValue);
            mEditorMessages[mEditorMessageIndex] = base64;
#else
            Debug.Log(message);
#endif
        }
    }
}