using System;
using Hactazia.Types;
using UnityEngine;
using VRC.SDKBase;

namespace Hactazia.MidiTransporter.Protocol.Addons
{
    public class DiscoverHelper : AddonTransporter
    {
        public string icon = "icon:cell_tower";
        public string title = "Discover";

        public override void OnPacket(MidiReceptor recep, ushort length, byte eventType, byte[] data)
        {
            base.OnPacket(recep, length, eventType, data);
            if (eventType != (byte)Events.Discover) return;
            ushort index = 0;
            var protocolVersion = data.ReadBuffer<uint>(index);
            index += BufferExtensions.SizeOf<uint>();
            var initial = data.ReadBufferDateTime(index);
            index += BufferExtensions.SizeOf<DateTimeOffset>();
            var middle = data.ReadBufferDateTime(index);
            index += BufferExtensions.SizeOf<DateTimeOffset>();
            var final = DateTimeOffset.UtcNow;

            var total = (final - initial).TotalMilliseconds;
            var up = (middle - initial).TotalMilliseconds;
            var down = (final - middle).TotalMilliseconds;
            Debug.Log($"Ping: {total}ms ({up}ms up, {down}ms down)");
        }

        public override void Start()
        {
            base.Start();
            SendDiscover(0);
            _lastDiscover = Time.time;
        }

        public byte intervalDiscover = 5; // seconds
        private float _lastDiscover = -1;

        public void Update()
        {
            if (!receptor || _lastDiscover <= 0) return;
            if (Time.time - _lastDiscover < intervalDiscover - 1) return;
            _lastDiscover = Time.time;
            SendDiscover(0);
        }


        public void SendDiscover(ulong challenge)
        {
            if (!receptor) return;
            ushort index = 0;
            var buffer = BufferExtensions.NewBuffer();
            buffer = buffer.WriteBuffer(index, ProtocolVersion);
            buffer = buffer.WriteBuffer(index += BufferExtensions.SizeOf<uint>(), receptor.GetInstanceID());
            buffer = buffer.WriteBuffer(index += BufferExtensions.SizeOf<int>(), (byte)receptor.listenEvents);
            buffer = buffer.WriteBuffer(index += BufferExtensions.SizeOf<byte>(), receptor.listenChannel);
            buffer = buffer.WriteBuffer(index += BufferExtensions.SizeOf<byte>(), intervalDiscover);
            buffer = buffer.WriteBuffer(index += BufferExtensions.SizeOf<byte>(), title);
            buffer = buffer.WriteBuffer(index += BufferExtensions.SizeOf(title), icon);
            buffer = buffer.WriteBuffer(index += BufferExtensions.SizeOf(icon), DateTimeOffset.UtcNow);
            Send(receptor, buffer, Events.Discover);
        }
    }
}