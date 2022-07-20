using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBox_Move : OVRGrabbable
{
    [SerializeField]
    public List<BoundingBox_RotateAround> children = new List<BoundingBox_RotateAround>();
    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        base.GrabBegin(hand, grabPoint);

        foreach (var item in children)
        {
            item.ChangeMaterial(item.selectedMat);
            item.StopDelay();
        }
    }

    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        base.GrabEnd(linearVelocity, angularVelocity);
        ChangeMaterial(deselectedMat);
        foreach (var item in children)
        {
            item.DelayedStart();
        }
    }
}
