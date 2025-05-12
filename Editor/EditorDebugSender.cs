#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Hactazia.UdonTools;
using Object = UnityEngine.Object;

namespace Hactazia.MidiTransporter.Editor
{
    [InitializeOnLoad]
    public class EditorDebugSender
    {
        static EditorDebugSender()
        {
            EditorApplication.update += Update;
        }

        private static string VRChatPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "..", "LocalLow", "VRChat", "VRChat");

        private static string LastOutputFile
        {
            get
            {
                var reg = new System.Text.RegularExpressions.Regex(
                    @"output_log_\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}.txt");
                var files = Directory.GetFiles(VRChatPath);
                Array.Sort(files);
                Array.Reverse(files);
                return files.FirstOrDefault(file => reg.IsMatch(file));
            }
        }

        private static readonly List<LastLog> LastLogSenders = new();


        private static void Update()
        {
            if (!Application.isPlaying) return;

            #region Find Receptors

            var receptors = Object.FindObjectsOfType<MidiReceptor>()
                .Where(r => r && r.gameObject)
                .ToArray();

            if (receptors.Length == 0)
            {
                if (LastLogSenders.Count > 0)
                    LastLogSenders.Clear();
                return;
            }

            foreach (var receptor in receptors)
            {
                var last = LastLogSenders.Find(l => l.Receptor == receptor);
                if (last != null) continue;
                last = new LastLog { Receptor = receptor, Index = 0 };
                LastLogSenders.Add(last);
            }

            foreach (var sender in LastLogSenders.ToArray())
                if (!receptors.Contains(sender.Receptor))
                    LastLogSenders.Remove(sender);

            #endregion

            foreach (var sender in LastLogSenders)
            {
                var c = sender.Receptor.GetUdonBehaviour();

                var list = !c
                    ? sender.Receptor.mEditorMessages
                    : c.GetProgramVariable("mEditorMessages") as string[];

                var index = !c
                    ? sender.Receptor.mEditorMessageIndex
                    : c.GetProgramVariable("mEditorMessageIndex") as byte?;

                if (list == null || list.Length == 0)
                    continue;

                while (sender.Index != index)
                {
                    sender.Index = (byte)((sender.Index + 1) % byte.MaxValue);
                    Send(list[sender.Index]);
                }
            }
        }

        private static void Send(string message)
        {
            var lastOutput = LastOutputFile;
            if (lastOutput == null)
            {
                lastOutput = Path.Combine(
                    VRChatPath,
                    "output_log_"
                    + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
                    + ".txt"
                );
                File.Create(lastOutput).Close();
            }

            File.AppendAllText(
                lastOutput,
                string.Join(
                    "",
                    DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"),
                    " Debug      -  ",
                    message,
                    Environment.NewLine
                )
            );
        }
    }

    internal class LastLog
    {
        public MidiReceptor Receptor;
        public byte Index;
    }
}
#endif