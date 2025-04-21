
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

public class MidiReceptor : UdonSharpBehaviour
{

    public int listenChannel = 10;

#if UNITY_EDITOR
    public string[] m_editorMessages;
    public byte m_editorMessageIndex;
#endif

    private byte[] _currentMessage;
    private byte _currentMessageIndex;

    public ITransferer[] transferers;

    void Start()
    {
#if UNITY_EDITOR
        m_editorMessages = new string[byte.MaxValue];
        m_editorMessageIndex = byte.MinValue;
#endif
        _currentMessage = new byte[byte.MaxValue];
        _currentMessageIndex = byte.MinValue;
        foreach (var transferer in transferers)
            transferer.OnStart(this);
    }

    private void OnMessage(string message)
    {
        foreach (var transferer in transferers)
            transferer.OnMessage(this, message);
        if (VRCJson.TryDeserializeFromJson(message, out DataToken json))
        {
            foreach (var transferer in transferers)
                transferer.OnJSON(this, json);
            if (json.TokenType == TokenType.DataDictionary)
            {
                var dictionary = json.DataDictionary;
                if (dictionary.TryGetValue("type", TokenType.String, out DataToken type))
                {
                    var state = dictionary.TryGetValue("state", TokenType.String, out DataToken stateToken) && stateToken.String.Trim().Length > 0 ? stateToken.String.Trim() : null;
                    var args = dictionary.TryGetValue("args", TokenType.DataList, out DataToken argsToken) ? argsToken.DataList : new DataList();
                    foreach (var transferer in transferers)
                        transferer.OnEvent(this, type.String, state, args.ToArray());
                }
            }
        }
    }

    private void OnByte(byte b)
    {
        foreach (var transferer in transferers)
            transferer.OnByte(this, b);
        if (b == 0)
        {
            var subarray = new byte[_currentMessageIndex];
            Array.Copy(_currentMessage, subarray, _currentMessageIndex);
            var base64 = System.Text.Encoding.ASCII.GetString(subarray);
            var bytes = Convert.FromBase64String(base64);
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            OnMessage(message);
            _currentMessage = new byte[byte.MaxValue];
            _currentMessageIndex = byte.MinValue;
        }
        else _currentMessage[_currentMessageIndex++] = b;
    }

    public override void MidiControlChange(int channel, int number, int velocity)
    {
        if (channel != listenChannel) return;
        OnByte((byte)number);
        if (number != 0)
            OnByte((byte)velocity);
    }

    public void Send(DataToken element)
    {
        Debug.Log($"Sending {element}");
        if (VRCJson.TrySerializeToJson(element, JsonExportType.Minify, out DataToken json))
        {
            var message = $"mt({json})";
#if UNITY_EDITOR
            m_editorMessageIndex = (byte)((m_editorMessageIndex + 1) % byte.MaxValue);
            m_editorMessages[m_editorMessageIndex] = message;
#else
            Debug.Log(message);
#endif
        }
        else
        {
            Debug.LogError("Failed to serialize json");
            Debug.LogError(element);
        }
    }

    public void SendEvent(string type, string state = null, params DataToken[] args)
    {
        Debug.Log($"Sending event {type} with state {state} and {args.Length} arguments");
        var dictionary = new DataDictionary();
        dictionary.Add("type", new DataToken(type));
        if (!string.IsNullOrEmpty(state))
            dictionary.Add("state", new DataToken(state));
        if (args.Length > 0)
            dictionary.Add("args", new DataList(args));
        Send(new DataToken(dictionary));
    }
}
