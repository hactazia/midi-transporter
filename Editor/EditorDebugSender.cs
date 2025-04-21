using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
public class EditorDebugSender : MonoBehaviour
{
    static EditorDebugSender()
    {
        EditorApplication.update += Update;
    }

    static string vrchatPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..", "LocalLow", "VRChat", "VRChat");
    static string lastOutputFile
    {
        get
        {
            var reg = new System.Text.RegularExpressions.Regex(@"output_log_\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}.txt");
            var files = Directory.GetFiles(vrchatPath);
            Array.Sort(files);
            Array.Reverse(files);
            foreach (var file in files)
                if (reg.IsMatch(file))
                    return file;
            return null;
        }
    }

    private static List<LastLog> lastLogSenders = new List<LastLog>();



    private static void Update()
    {
        if (!Application.isPlaying) return;
        var Receptors = FindObjectsOfType<MidiReceptor>();

        if (Receptors.Length == 0)
        {
            if (lastLogSenders.Count > 0)
                lastLogSenders.Clear();
            return;
        }

        foreach (var receptor in Receptors)
        {
            var last = lastLogSenders.Find(l => l.Receptor == receptor);
            if (last == null)
            {
                last = new LastLog() { Receptor = receptor, Index = 0 };
                lastLogSenders.Add(last);
            }
        }

        foreach (var sender in lastLogSenders.ToArray())
            if (!Receptors.Contains(sender.Receptor))
                lastLogSenders.Remove(sender);


        foreach (var sender in lastLogSenders)
        {
            var list = sender.Receptor.GetProgramVariable("m_editorMessages") as string[];
            var index = sender.Receptor.GetProgramVariable("m_editorMessageIndex") as byte?;
            while (sender.Index != index)
            {
                sender.Index = (byte)((sender.Index + 1) % byte.MaxValue);
                Send(list[sender.Index]);
            }
        }
    }

    private static void Send(string message)
    {
        var lastOutput = lastOutputFile;
        if (lastOutput == null) return;
        File.AppendAllText(lastOutputFile, "\n" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss") + " Debug        -  " + message + "\n");
    }
}

class LastLog
{
    public MidiReceptor Receptor;
    public byte Index;
}
