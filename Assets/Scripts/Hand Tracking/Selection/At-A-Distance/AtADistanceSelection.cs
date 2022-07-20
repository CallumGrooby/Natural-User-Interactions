using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtADistanceSelection : OVRGrabber
{
    [SerializeField]
    private OVRHand m_hand;
    [SerializeField]
    private float pinchThreshold = 0.7f;
    [SerializeField]
    public bool grab = false; //Right = 0 Left = 1

    protected override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();
        CheckIndexPinch();
    }

    public void SetIsGrabbing(bool setTo)
    {
        grab = setTo;
        Debug.Log(grab);
    }

    void CheckIndexPinch()
    {
        float pinchStrength = m_hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        
        RaycastHit hit;
        if (Physics.Raycast(this.transform.position, this.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            Debug.DrawRay(this.transform.position, this.transform.TransformDirection(Vector3.forward), Color.green);
            if (hit.transform.GetComponent<OVRGrabbable>())
            {
                Debug.Log("At A Distance Hit a object");
                if (!m_grabbedObj && pinchStrength > pinchThreshold)
                {
                    //GrabBegin();
                }
            }

        }
        else if (m_grabbedObj && !(pinchStrength > pinchThreshold))
        {
            //GrabEnd();
        }


    }
}
