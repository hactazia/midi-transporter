
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

public class LightParkExample : ITransferer
{
    public LightPark[] parks;

    public override void OnEvent(MidiReceptor receptor, string eventName, string state = null, params DataToken[] args)
    {
        if (eventName == "get_lights")
            SendAllLightParks(receptor, state);
        else if (eventName == "move_light_pointer")
            MoveLightPointer(receptor, args);
    }

    private void SendAllLightParks(MidiReceptor receptor, string state = null)
    {
        DataToken[] args = new DataToken[parks.Length];
        for (int i = 0; i < parks.Length; i++)
            args[i] = new DataToken(GetParkData(parks[i]));
        receptor.SendEvent("get_lights", state, args);
    }

    private void MoveLightPointer(MidiReceptor receptor, DataToken[] args)
    {
        if (args.Length < 4) return;
        int park_id = (int)args[0].Double;
        float rel_x = (float)args[1].Double;
        float rel_y = (float)args[2].Double;
        float rel_z = (float)args[3].Double;
        LightPark park = null;
        foreach (var p in parks)
            if (p.transform.GetInstanceID() == park_id)
            {
                park = p;
                break;
            }
        if (park == null) return;
        park.MovePointer(new Vector3(rel_x, rel_y, rel_z));
    }

    private void SendAddLightPark(LightPark park, MidiReceptor receptor, string state = null)
    {
        receptor.SendEvent("add_light", state, new DataToken(GetParkData(park)));
    }

    private void SendRemoveLightPark(LightPark park, MidiReceptor receptor, string state = null)
    {
        receptor.SendEvent("remove_light", state, park.name);
    }

    private void SendUpdateLightPark(LightPark park, MidiReceptor receptor, string state = null)
    {
        receptor.SendEvent("update_light", state, new DataToken(GetParkData(park)));
    }


    private DataDictionary GetParkData(LightPark park)
    {
        var data = new DataDictionary();

        // general data
        data.Add("id", new DataToken(park.transform.GetInstanceID()));

        // Position, size, and rotation are all arrays of floats

        // Position is a 3D vector
        var array_pos = new DataList();
        array_pos.Add(new DataToken(park.transform.position.x));
        array_pos.Add(new DataToken(park.transform.position.y));
        array_pos.Add(new DataToken(park.transform.position.z));
        data.Add("position", array_pos);

        // Size is a 2D vector
        var array_size = new DataList();
        array_size.Add(new DataToken(park.size.x));
        array_size.Add(new DataToken(park.size.y));
        array_size.Add(new DataToken(park.size.z));
        data.Add("size", array_size);

        // Rotation is a quaternion
        var array_rotation = new DataList();
        array_rotation.Add(new DataToken(park.transform.rotation.x));
        array_rotation.Add(new DataToken(park.transform.rotation.y));
        array_rotation.Add(new DataToken(park.transform.rotation.z));
        array_rotation.Add(new DataToken(park.transform.rotation.w));
        data.Add("rotation", array_rotation);

        // lights
        DataToken[] lights = new DataToken[park.lights.Length];
        for (int i = 0; i < park.lights.Length; i++)
        {
            var light = park.lights[i];
            var light_data = new DataDictionary();
            light_data.Add("id", new DataToken(light.transform.GetInstanceID()));
            light_data.Add("intensity", new DataToken(light.intensity));
            var pos = new DataList();
            var rp = park.GetRelative(light.transform);
            pos.Add(new DataToken(rp.x));
            pos.Add(new DataToken(rp.y));
            pos.Add(new DataToken(rp.z));
            light_data.Add("position", pos);
            lights[i] = light_data;
        }
        data.Add("lights", new DataList(lights));

        // pointer
        if (park.pointer != null)
        {
            var pointer_data = new DataDictionary();
            pointer_data.Add("id", new DataToken(park.pointer.transform.GetInstanceID()));
            var pos = new DataList();
            var rp = park.GetRelative(park.pointer.transform);
            pos.Add(new DataToken(rp.x));
            pos.Add(new DataToken(rp.y));
            pos.Add(new DataToken(rp.z));
            pointer_data.Add("position", pos);
            data.Add("pointer", pointer_data);
        }
        else data.Add("pointer", new DataToken());
        return data;
    }
}
