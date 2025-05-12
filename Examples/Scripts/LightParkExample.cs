using Hactazia.MidiTransporter.Protocol;
using UnityEngine;
using VRC.SDK3.Data;

namespace Hactazia.MidiTransporter.Examples
{
    public class LightParkExample : AddonTransporter
    {
        public LightPark[] parks;

        public override void OnEvent(MidiReceptor recep, string eventName, string state, params DataToken[] args)
        {
            base.OnEvent(recep, eventName, state, args);
            if (eventName == "get_lights")
                SendAllLightParks(receptor, state);
            else if (eventName == "move_light_pointer")
                MoveLightPointer(receptor, args);
        }

        private void SendAllLightParks(MidiReceptor recep, string state = null)
        {
            DataToken[] args = new DataToken[parks.Length];
            for (int i = 0; i < parks.Length; i++)
                args[i] = new DataToken(GetParkData(parks[i]));
            Send(recep, "get_lights", state, args);
        }

        private void MoveLightPointer(MidiReceptor recep, DataToken[] args)
        {
            if (args.Length < 4) return;
            var parkID = args[0].Int;
            var rel = new Vector3(args[1].Float, args[2].Float, args[3].Float);
            LightPark park = null;
            foreach (var p in parks)
                if (p.transform.GetInstanceID() == parkID)
                {
                    park = p;
                    break;
                }

            if (!park) return;
            park.MovePointer(rel);
        }

        private void SendAddLightPark(LightPark park, MidiReceptor recep, string state = null)
        {
            Send(recep, "add_light", state, new DataToken[] { GetParkData(park) });
        }

        private void SendRemoveLightPark(LightPark park, MidiReceptor recep, string state = null)
        {
            Send(recep, "remove_light", state, new DataToken[] { park.name });
        }

        private void SendUpdateLightPark(LightPark park, MidiReceptor recep, string state = null)
        {
            Send(recep, "update_light", state, new DataToken[] { GetParkData(park) });
        }


        private DataDictionary GetParkData(LightPark park)
        {
            var data = new DataDictionary();

            // general data
            data.Add("id", new DataToken(park.transform.GetInstanceID()));

            // Position, size, and rotation are all arrays of floats

            // Position is a 3D vector
            var arrayPos = new DataList();
            arrayPos.Add(new DataToken(park.transform.position.x));
            arrayPos.Add(new DataToken(park.transform.position.y));
            arrayPos.Add(new DataToken(park.transform.position.z));
            data.Add("position", arrayPos);

            // Size is a 2D vector
            var arraySize = new DataList();
            arraySize.Add(new DataToken(park.size.x));
            arraySize.Add(new DataToken(park.size.y));
            arraySize.Add(new DataToken(park.size.z));
            data.Add("size", arraySize);

            // Rotation is a quaternion
            var arrayRotation = new DataList();
            arrayRotation.Add(new DataToken(park.transform.rotation.x));
            arrayRotation.Add(new DataToken(park.transform.rotation.y));
            arrayRotation.Add(new DataToken(park.transform.rotation.z));
            arrayRotation.Add(new DataToken(park.transform.rotation.w));
            data.Add("rotation", arrayRotation);

            // lights
            var lights = new DataToken[park.lights.Length];
            for (var i = 0; i < park.lights.Length; i++)
            {
                var parkLight = park.lights[i];
                var lightData = new DataDictionary();
                lightData.Add("id", new DataToken(parkLight.transform.GetInstanceID()));
                lightData.Add("intensity", new DataToken(parkLight.intensity));
                var pos = new DataList();
                var rp = park.GetRelative(parkLight.transform);
                pos.Add(new DataToken(rp.x));
                pos.Add(new DataToken(rp.y));
                pos.Add(new DataToken(rp.z));
                lightData.Add("position", pos);
                lights[i] = lightData;
            }

            data.Add("lights", new DataList(lights));

            // pointer
            if (park.pointer != null)
            {
                var pointerData = new DataDictionary();
                pointerData.Add("id", new DataToken(park.pointer.transform.GetInstanceID()));
                var pos = new DataList();
                var rp = park.GetRelative(park.pointer.transform);
                pos.Add(new DataToken(rp.x));
                pos.Add(new DataToken(rp.y));
                pos.Add(new DataToken(rp.z));
                pointerData.Add("position", pos);
                data.Add("pointer", pointerData);
            }
            else data.Add("pointer", new DataToken());

            return data;
        }
    }
}