using Hactazia.Types;
using UnityEngine;
using VRC.SDK3.Data;

namespace Hactazia.MidiTransporter.Protocol
{
    public class AddonTransporter : BaseTransporter
    {
        public const uint ProtocolVersion = 3;

        private DataDictionary _details;

        public virtual void OnPacket(MidiReceptor recep, ushort length, byte eventType, byte[] data)
        {
        }

        public virtual void OnEvent(MidiReceptor recep, string eventName, ushort state, byte[] data)
        {
        }

        public virtual void OnEvent(MidiReceptor recep, string eventName, string state, params DataToken[] args)
        {
        }

        public override void OnBuffer(MidiReceptor recep, byte[] ba)
        {
            base.OnBuffer(recep, ba);
            ushort index = 0;
            var length = ba.ReadBuffer<ushort>(index);
            index += BufferExtensions.SizeOf<ushort>();
            var eventType = ba.ReadBuffer<byte>(index);
            index += BufferExtensions.SizeOf<byte>();

            var rawData = ba.ReadBufferBytes(index, (ushort)(length - index));
            OnPacket(recep, length, eventType, rawData);

            if (eventType == (byte)Events.EventBinary)
            {
                var eventName = ba.ReadBufferString(index);
                var strLength = BufferExtensions.SizeOf(eventName);
                index += strLength;
                var state = ba.ReadBuffer<ushort>(index);
                index += BufferExtensions.SizeOf<ushort>();
                var data = ba.ReadBufferBytes(index, (ushort)(length - index));
                OnEvent(recep, eventName, state, data);
                return;
            }

            if (eventType == (byte)Events.EventJson)
            {
                var json = ba.ReadBufferString(index);
                if (!VRCJson.TryDeserializeFromJson(json, out var token)) return;
                var data = token.DataDictionary;
                if (!data.TryGetValue("event", out var eventName)) return;
                var state = string.Empty;
                if (data.TryGetValue("state", out var tokenState))
                    state = tokenState.ToString();
                var list = new DataList();
                if (data.TryGetValue("data", out var tokenData) && tokenData.TokenType != TokenType.DataList)
                    list = tokenData.DataList;
                OnEvent(recep, eventName.ToString(), state, list.ToArray());
                return;
            }
        }


        public bool IsRegistered()
        {
            return _details != null && _details.ContainsKey("id");
        }

        public string GetId()
        {
            if (_details == null || !_details.TryGetValue("id", out var id))
                return string.Empty;
            return id.ToString();
        }

        public string GetVersion()
        {
            if (_details == null || !_details.TryGetValue("version", out var version))
                return string.Empty;
            return version.ToString();
        }


        public bool RegisterAddon(MidiReceptor recep, DataDictionary details)
        {
            if (details == null || IsRegistered()) return false;
            if (!details.TryGetValue("id", out var id) || id.IsEmpty) return false;
            if (!details.TryGetValue("version", out var version) || version.IsEmpty) return false;
            if (!Send(recep, details, Events.Register)) return false;
            _details = details;
            return true;
        }

        public bool UnregisterAddon(MidiReceptor recep)
        {
            if (!IsRegistered()) return false;
            if (!_details.TryGetValue("id", out var id)) return false;
            var str = id.ToString();
            var strLength = BufferExtensions.SizeOf(str);
            var buffer = BufferExtensions.NewBuffer(strLength);
            buffer = buffer.WriteBuffer(0, str);
            if (!Send(recep, buffer, Events.Unregister))
                return false;
            _details = null;
            return true;
        }

        public bool Send(MidiReceptor recep, byte[] data, Events ev)
        {
            return Send(recep, data, (byte)ev);
        }

        public bool Send(MidiReceptor recep, byte[] data, byte ev)
        {
            ushort index = 0;
            var buffer = BufferExtensions.NewBuffer();
            buffer = buffer.WriteBuffer(index, (ushort)(
                BufferExtensions.SizeOf<byte>()
                + BufferExtensions.SizeOf<ushort>()
                + data.Length
            ));
            index += BufferExtensions.SizeOf<ushort>();
            buffer = buffer.WriteBuffer(index, ev);
            index += BufferExtensions.SizeOf<byte>();
            buffer = buffer.WriteBuffer(index, data);
            return Send(receptor, buffer);
        }

        public bool Send(MidiReceptor recep, DataDictionary data, Events ev)
        {
            return Send(recep, data, (byte)ev);
        }

        public bool Send(MidiReceptor recep, DataDictionary data, byte ev)
        {
            if (!VRCJson.TrySerializeToJson(data, JsonExportType.Minify, out var json))
                return false;
            var str = json.ToString();
            var strLength = BufferExtensions.SizeOf(str);
            var buffer = BufferExtensions.NewBuffer(strLength);
            buffer = buffer.WriteBuffer(0, str);
            return Send(recep, buffer, ev);
        }

        public bool Send(MidiReceptor recep, string eventName, string state, params DataToken[] args)
        {
            var dict = new DataDictionary();
            if (!string.IsNullOrEmpty(state))
                dict.Add("state", state);
            var list = new DataList();
            foreach (var arg in args)
                list.Add(arg);
            dict.Add("data", list);
            dict.Add("event", eventName);
            return Send(recep, dict, Events.EventBinary);
        }
    }
}