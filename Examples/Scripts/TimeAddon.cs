using System;
using Hactazia.MidiTransporter.Protocol;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Hactazia.MidiTransporter.Examples
{
    public class TimeAddon : AddonTransporter
    {
        public Light directionalLight;
        [UdonSynced] public Quaternion quaternion;


        public override void OnEvent(MidiReceptor recep, string eventName, string state, params DataToken[] args)
        {
            base.OnEvent(recep, eventName, state, args);
            if (eventName != "set_time") return;
            if (args.Length < 1)
            {
                Debug.LogError("set_time requires an argument");
                return;
            }

            if (args[0].TokenType != TokenType.Double)
            {
                Debug.LogError($"set_time requires an integer argument but got {args[0].TokenType}");
                return;
            }

            var time = args[0].Double;
            var timeString = TimeSpan.FromSeconds(time).ToString();
            Debug.Log($"Setting time to {timeString}");
            if (directionalLight)
                SetTime((float)time);
        }

        public override void Start()
        {
            if (!directionalLight)
                directionalLight = GetComponentInChildren<Light>();
            SetTime(DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second);
            base.Start();
        }

        private void Update()
        {
            if (!directionalLight) return;
            directionalLight.transform.localRotation = Quaternion.Slerp(
                directionalLight.transform.localRotation,
                quaternion,
                Time.deltaTime
            );
        }

        private void SetTime(float time)
        {
            if (!Networking.IsOwner(gameObject))
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            if (directionalLight == null) return;
            var timeOfDay = time / 86400;
            quaternion = Quaternion.Euler(timeOfDay * 360 - 90, 0, 0);
        }
    }
}