#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;
using Hactazia.UdonTools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Midi;
using VRC.SDKBase.Midi;

namespace Hactazia.MidiTransporter.Editor
{
    [CustomEditor(typeof(MidiReceptor))]
    public class MidiReceptorEditor : UnityEditor.Editor
    {
        public static readonly MethodInfo ShowWindowMethod = typeof(VRCMidiWindow)
            .GetMethod("ShowWindow", BindingFlags.NonPublic | BindingFlags.Static);

        public static void ShowVrcMidiWindowWindow()
            => ShowWindowMethod.Invoke(null, null);

        public VRCMidiListener GetListener()
        {
            var midiReceptor = (MidiReceptor)target;
            var vrcMidiListeners = midiReceptor.GetComponents<VRCMidiListener>();
            var behaviour = midiReceptor.GetUdonBehaviour();
            var vrcMidiListener = vrcMidiListeners.FirstOrDefault(l => l.GetBehaviour() == behaviour);
            return vrcMidiListener;
        }

        private void OnEnable()
        {
            EditorApplication.update += OnUpdate;
            UpdateTransporters();
            CheckWarns();
            EditorApplication.DirtyHierarchyWindowSorting();
        }

        void OnDestroy()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.DirtyHierarchyWindowSorting();
        }

        private DateTime _lastUpdate = DateTime.MinValue;

        private void OnUpdate()
        {
            if ((DateTime.Now - _lastUpdate).TotalSeconds < 2) return;
            _lastUpdate = DateTime.Now;
            OnValidate();
        }

        void OnValidate()
        {
            var midiReceptor = (MidiReceptor)target;
            UpdateTransporters();
            EditorApplication.DirtyHierarchyWindowSorting();
            CheckWarns();
            UpdateSettings();

            EditorUtility.SetDirty(midiReceptor);
            if (_root == null) return;

            var listenChannel = _root.Q<UnsignedIntegerField>("listen-channel");
            if (listenChannel.value != midiReceptor.listenChannel)
                listenChannel.SetValueWithoutNotify(midiReceptor.listenChannel);
            var listenEvent = _root.Q<EnumField>("listen-event");
            if ((MidiEvents)listenEvent.value != midiReceptor.listenEvents)
                listenEvent.SetValueWithoutNotify(midiReceptor.listenEvents);
        }

        void CheckWarns()
        {
            if (_root == null) return;
            var midiReceptor = (MidiReceptor)target;

            var toReset = _root.Q("to_reset");
            toReset.style.display = midiReceptor.listenChannel != MidiReceptor.DefaultChannel
                                    || midiReceptor.listenEvents != MidiReceptor.DefaultEvents
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            var createMidi = _root.Q("create_midi");
            createMidi.style.display = !GetListener()
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
            UpdateTransporters();
            EditorApplication.DirtyHierarchyWindowSorting();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        public void UpdateTransporters()
        {
            if (_root == null) return;
            var midiReceptor = (MidiReceptor)target;

            var transporters = midiReceptor.gameObject.scene.GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<BaseTransporter>(true))
                .Where(t => t.receptor == midiReceptor)
                .ToList();

            var container = _root.Q("transporters");
            var ids = transporters.Select(t => t.GetInstanceID()).ToList();
            foreach (var child in container.Children())
                if (child.userData is not int id || !ids.Contains(id))
                    child.RemoveFromHierarchy();
                else ids.Remove(id);

            if (ids.Count > 0)
            {
                var transporterElement = Resources.Load<VisualTreeAsset>("MidiTransporterElement");
                foreach (var id in ids)
                {
                    var transporter = transporters.Find(t => t.GetInstanceID() == id);
                    if (!transporter) continue;
                    var element = transporterElement.CloneTree();
                    element.userData = id;
                    container.Add(element);
                }
            }

            foreach (var child in container.Children())
            {
                var transporter = transporters.Find(t => t.GetInstanceID() == (int)child.userData);
                if (!transporter) continue;
                var obj = child.Q<ObjectField>();
                obj.value = transporter;
            }
        }

        private VisualElement _root;

        public override VisualElement CreateInspectorGUI()
        {
            if (_root != null) return _root;
            var midiReceptor = (MidiReceptor)target;
            _root = Resources.Load<VisualTreeAsset>("MidiReceptorEditor").CloneTree();

            var listenEvent = _root.Q<EnumField>("listen-event");
            listenEvent.value = midiReceptor.listenEvents;
            listenEvent.RegisterCallback<ChangeEvent<Enum>>(evt =>
            {
                midiReceptor.listenEvents = (MidiEvents)evt.newValue;
                EditorUtility.SetDirty(midiReceptor);
                OnValidate();
            });

            var receptorId = _root.Q<IntegerField>("receptor-id");
            receptorId.value = midiReceptor.GetInstanceID();

            var listenChannel = _root.Q<UnsignedIntegerField>("listen-channel");
            listenChannel.value = midiReceptor.listenChannel;
            listenChannel.RegisterCallback<ChangeEvent<uint>>(evt =>
            {
                switch (evt.newValue)
                {
                    case > 16:
                        listenChannel.value = 16;
                        return;
                    case < 1:
                        listenChannel.value = 1;
                        return;
                    default:
                        midiReceptor.listenChannel = (byte)evt.newValue;
                        break;
                }

                EditorUtility.SetDirty(midiReceptor);
                OnValidate();
            });

            var toReset = _root.Q("to_reset");
            var resetButton = toReset.Q<Button>();
            resetButton.RegisterCallback<ClickEvent>(evt =>
            {
                midiReceptor.listenChannel = MidiReceptor.DefaultChannel;
                midiReceptor.listenEvents = MidiReceptor.DefaultEvents;
                EditorUtility.SetDirty(midiReceptor);
                OnValidate();
            });
            var createMidi = _root.Q("create_midi");
            var createButton = createMidi.Q<Button>();
            createButton.RegisterCallback<ClickEvent>(evt =>
            {
                var midiListener = GetListener();
                if (midiListener) return;
                midiListener = midiReceptor.gameObject.AddComponent<VRCMidiListener>();
                midiListener.activeEvents = VRCMidiListener.MidiEvents.CC
                                            | VRCMidiListener.MidiEvents.NoteOn
                                            | VRCMidiListener.MidiEvents.NoteOff;
                midiListener.SetBehaviour(midiReceptor.GetUdonBehaviour());
                midiListener.enabled = true;
                EditorUtility.SetDirty(midiListener);
                OnValidate();
            });


            var change = _root.Q<Button>("change-device");
            change.RegisterCallback<ClickEvent>(evt => ShowVrcMidiWindowWindow());

            EditorApplication.update += OnUpdate;
            OnValidate();

            return _root;
        }

        private void UpdateSettings()
        {
            if (_root == null) return;

            var midi = new VRCPortMidiInput();
            var deviceNames = midi.GetDeviceNames().ToList();
            var currentDeviceValue = EditorPrefs.GetString(VRCMidiWindow.DEVICE_NAME_STRING);
            var defaultValue = deviceNames.Contains(currentDeviceValue) ? currentDeviceValue : "";
            var deviceNameField = _root.Q<TextField>("device-name");
            deviceNameField.value = defaultValue;
        }
    }
}
#endif