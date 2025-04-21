
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class LightPark : UdonSharpBehaviour
{
    public Vector3 size;

    public GameObject pointer;

    [UdonSynced] public Vector3 AtReltativePointer;
    public float speed = 10;

    public Light[] lights;

    void Start()
    {
        if (pointer == null) return;
        AtReltativePointer = GetRelative(pointer.transform);
    }

    void Update()
    {
        if (pointer == null) return;
        var position = new Vector3(
            AtReltativePointer.x * size.x / 2,
            AtReltativePointer.y * size.y / 4,
            AtReltativePointer.z * size.z / 2
        );
        pointer.transform.localPosition = Vector3.Lerp(pointer.transform.localPosition, position, Time.deltaTime * speed);
    }

    public Vector3 GetRelative(Vector3 tr)
    {
        var local = transform.InverseTransformPoint(tr);
        return new Vector3(
            local.x / size.x * 2,
            local.y / size.y * 4,
            local.z / size.z * 2
        );
    }

    public void MovePointer(Vector3 rel)
    {
        if (!Networking.IsOwner(gameObject)) return;
        AtReltativePointer = rel;
    }

    public Vector3 GetRelative(Transform tr)
    {
        return GetRelative(tr.position);
    }

    public bool IsInner(Transform tr)
    {
        var rel = GetRelative(tr);
        return rel.x >= -1 && rel.x <= 1 && rel.y >= -1 && rel.y <= 1 && rel.z >= -1 && rel.z <= 1;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        var position = transform.position;
        var rotation = transform.rotation;
        Gizmos.DrawSphere(position, 0.2f);


        var forward = rotation * Vector3.forward;
        var right = rotation * Vector3.right;
        var up = rotation * Vector3.up;

        Gizmos.color = pointer == null ? Color.red : IsInner(pointer.transform) ? Color.green : Color.yellow;

        var halfSize = new Vector3(size.x / 2, size.y / 2, size.z / 2);
        var corners = new Vector3[]
        {
            position + right * halfSize.x + forward * halfSize.z,
            position + right * halfSize.x - forward * halfSize.z,
            position - right * halfSize.x - forward * halfSize.z,
            position - right * halfSize.x + forward * halfSize.z,

            position + right * halfSize.x + forward * halfSize.z + up * halfSize.y,
            position + right * halfSize.x - forward * halfSize.z + up * halfSize.y,
            position - right * halfSize.x - forward * halfSize.z + up * halfSize.y,
            position - right * halfSize.x + forward * halfSize.z + up * halfSize.y,
        };

        // down all corners by y half size
        for (int i = 0; i < corners.Length; i++)
            corners[i] -= up * halfSize.y / 2;


        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[1], corners[2]);
        Gizmos.DrawLine(corners[2], corners[3]);
        Gizmos.DrawLine(corners[3], corners[0]);

        Gizmos.DrawLine(corners[4], corners[5]);
        Gizmos.DrawLine(corners[5], corners[6]);
        Gizmos.DrawLine(corners[6], corners[7]);
        Gizmos.DrawLine(corners[7], corners[4]);

        Gizmos.DrawLine(corners[0], corners[4]);
        Gizmos.DrawLine(corners[1], corners[5]);
        Gizmos.DrawLine(corners[2], corners[6]);
        Gizmos.DrawLine(corners[3], corners[7]);


        Gizmos.color = Color.green;
        Gizmos.DrawSphere(position, 0.1f);

        // make line to lights if they exist
        if (pointer != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(position, pointer.transform.position);
            Gizmos.color = Color.white;
            foreach (var light in lights)
                Gizmos.DrawLine(pointer.transform.position, light.transform.position);
        }
    }
}
