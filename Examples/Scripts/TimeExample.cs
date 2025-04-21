
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

public class TimeExample : ITransferer
{
    public Light directionalLight;
    [UdonSynced] public Quaternion quaternion;

    public override void OnEvent(MidiReceptor receptor, string eventName, string state = null, params DataToken[] args)
    {
        if (eventName != "set_time" || string.IsNullOrEmpty(eventName)) return;
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
        if (directionalLight != null)
            SetTime((float)time);
    }

    void Start()
    {
        if (directionalLight == null)
            directionalLight = GetComponentInChildren<Light>();
        SetTime(DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second);
    }

    void Update()
    {
        if (directionalLight == null) return;
        directionalLight.transform.localRotation = Quaternion.Slerp(
            directionalLight.transform.localRotation,
            quaternion,
            Time.deltaTime
        );
    }

    private void SetTime(float time)
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (directionalLight == null) return;
        var timeOfDay = time / 86400;
        quaternion = Quaternion.Euler(timeOfDay * 360 - 90, 0, 0);
    }
}
