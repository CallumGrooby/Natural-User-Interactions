using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbleObject : InteractableObject
{
    protected override void Start()
    {
        base.Start();
    }

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        base.GrabBegin(hand, grabPoint);
        GetComponent<Renderer>().material.color = Color.red;   
    }

    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        base.GrabEnd(linearVelocity, angularVelocity);
        GetComponent<Renderer>().material.color = Color.green;
    }
}
