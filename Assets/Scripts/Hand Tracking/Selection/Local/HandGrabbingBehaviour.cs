using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HandGrabbingBehaviour : OVRGrabber
{
    [SerializeField]
    private OVRHand m_hand;
    [SerializeField]
    private float pinchThreshold = 0.2f;
    [SerializeField]
    public bool grab = false; //Right = 0 Left = 1

    [SerializeField]
    GameObject parentObject;

    [SerializeField]
    LineRenderer line;
    int lengthOfLine;

    private Transform indexFinger;
    private Transform thumbFinger;

    public OVRSkeleton skeleton;
    public GameObject rayCastPoint;
    bool isAtADistanceSelection = false;
    public GameObject XRRig;
    public bool recentlyTeleported;

    protected override void Start()
    {
        base.Start();
        if (line != null)
        {
            lengthOfLine = line.positionCount;
        }
    }

    public override void Update()
    {
        base.Update();
        CheckIndexPinch();


        if (indexFinger == null && thumbFinger == null)
        {
            foreach (OVRBone bone in skeleton.Bones)
            {
                if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
                {
                    indexFinger = bone.Transform;
                }
                if (bone.Id == OVRSkeleton.BoneId.Hand_ThumbTip)
                {
                    thumbFinger = bone.Transform;
                }
            }
        }
        else
        {
            Vector3 positionOfRayCast = (thumbFinger.position + indexFinger.position) / 2;
            Quaternion rotationOfRayCast = thumbFinger.rotation * Quaternion.Euler(0,90,0);
            if (this.rayCastPoint != null)
            {
                this.rayCastPoint.transform.position = positionOfRayCast;
                this.rayCastPoint.transform.rotation = Quaternion.Slerp(thumbFinger.rotation , indexFinger.rotation, 0.5f) * Quaternion.Euler(0, 90, 0);
            }
        }
    }

    public void StartVisualizing()
    {
        //Set Enabled Line Render
        //Create a ray cast from this 
        //Debug.Log("StartVisualizing");
        VisualizeAtADistanceSelection(rayCastPoint.transform.forward);
    }

    public void SetIsGrabbing(bool setTo)
    {
        grab = setTo;
        //Debug.Log(grab);
    }

    void CheckIndexPinch()
    {
        float pinchStrength = m_hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

        //Debug.LogWarning(pinchStrength);

        if (pinchStrength > pinchThreshold)
        {
            if (rayCastPoint == null)
            {
                return;
            }
            RaycastHit hit;
            if (Physics.Raycast(rayCastPoint.transform.position, rayCastPoint.transform.forward, out hit, Mathf.Infinity))
            {
                if (m_grabbedObj == null && !recentlyTeleported)
                {
                    if (hit.transform.GetComponent<OVRGrabbable>() != null)
                    {
                        m_grabbedObj = hit.transform.GetComponent<OVRGrabbable>();
                        Collider grabbableCollider = null;
                        for (int i = 0; i < m_grabbedObj.grabPoints.Length; i++)
                        {
                            grabbableCollider = m_grabbedObj.grabPoints[i];
                        }
                        isAtADistanceSelection = true;
                        SetUpGrabbedObject(grabbableCollider);
                    }
                    else
                    {
                        if (!recentlyTeleported)
                        {
                            Vector3 hitLocation = TeleportationLogic(XRRig);
                            XRRig.transform.position = new Vector3(hitLocation.x,XRRig.transform.position.y,hitLocation.z);
                            recentlyTeleported = true;
                        }
                    }
                }
            }
        }
        //Draw a sphere around the hand if any object is in the sphere then run this logic instead of the at a distance
        if (!m_grabbedObj && pinchStrength > pinchThreshold && m_grabCandidates.Count > 0)
        {
            Debug.LogWarning("GRAB");
            isAtADistanceSelection = false;
            //GrabBegin();
        }
        else if (m_grabbedObj &&!(pinchStrength > pinchThreshold))
        {
            GrabEnd();
        }
        else if (m_grabbedObj == null && !(pinchStrength > pinchThreshold))
        {
            recentlyTeleported = false;
        }
    }

    private void VisualizeAtADistanceSelection(Vector3 hitLocation)
    {
        //Debug.Log("Visualising Line");
        if (line != null)
        {
            var points = new Vector3[lengthOfLine];

            for (int i = 0; i < lengthOfLine; i++)
            {
                if (i == 0)
                {
                    points[i] = rayCastPoint.transform.position;
                    //Debug.Log(points[i] + " " + rayCastPoint.transform.position);
                }
                if (i == lengthOfLine - 1)
                {
                    float pinchStrength = m_hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
                    Debug.DrawRay(rayCastPoint.transform.position, rayCastPoint.transform.forward, Color.red);
                    points[i] = (rayCastPoint.transform.position + rayCastPoint.transform.forward);
                    //Debug.Log(hitLocation);
                }
                else
                {
                    //Draw Arch
                    //points[i] = new Vector3(i * 0.5f, Mathf.Sin(i), 0.0f);
                }
            }
            line.SetPositions(points);

            //Debug.Log(" " + rayCastPoint.transform.position);
        }
    }

    public Vector3 TeleportationLogic(GameObject XR_Rig)
    {
        RaycastHit hit;
        if (Physics.Raycast(rayCastPoint.transform.position, rayCastPoint.transform.forward, out hit, Mathf.Infinity))
        {
            if (hit.transform.gameObject.tag == "Ground")
            {
                return hit.point;
            }
        }
        return XR_Rig.transform.localPosition;
    }

    protected override void GrabBegin()
    {
        float closestMagSq = float.MaxValue;
        OVRGrabbable closestGrabbable = null;
        Collider closestGrabbableCollider = null;

        // Iterate grab candidates and find the closest grabbable candidate
        foreach (OVRGrabbable grabbable in m_grabCandidates.Keys)
        {
            bool canGrab = !(grabbable.isGrabbed && !grabbable.allowOffhandGrab);
            if (!canGrab)
            {
                continue;
            }

            for (int j = 0; j < grabbable.grabPoints.Length; ++j)
            {
                Collider grabbableCollider = grabbable.grabPoints[j];
                // Store the closest grabbable
                Vector3 closestPointOnBounds = grabbableCollider.ClosestPointOnBounds(m_gripTransform.position);
                float grabbableMagSq = (m_gripTransform.position - closestPointOnBounds).sqrMagnitude;
                if (grabbableMagSq < closestMagSq)
                {
                    closestMagSq = grabbableMagSq;
                    closestGrabbable = grabbable;
                    closestGrabbableCollider = grabbableCollider;
                }
            }
            SetUpGrabbedObject(closestGrabbableCollider);
        }
    }

    void SetUpGrabbedObject(Collider closestGrabbableCollider)
    {
            m_grabbedObj.GrabBegin(this, closestGrabbableCollider);

            m_lastPos = transform.position;
            m_lastRot = transform.rotation;

            // Set up offsets for grabbed object desired position relative to hand.
            if (m_grabbedObj.snapPosition)
            {
                m_grabbedObjectPosOff = m_gripTransform.localPosition;
                if (m_grabbedObj.snapOffset)
                {
                    Vector3 snapOffset = m_grabbedObj.snapOffset.position;
                    if (m_controller == OVRInput.Controller.LTouch) snapOffset.x = -snapOffset.x;
                    m_grabbedObjectPosOff += snapOffset;
                }
            }
            else
            {
                Vector3 relPos = m_grabbedObj.transform.position - transform.position;
                relPos = Quaternion.Inverse(transform.rotation) * relPos;
                m_grabbedObjectPosOff = relPos;
            }

            if (m_grabbedObj.snapOrientation)
            {
                m_grabbedObjectRotOff = m_gripTransform.localRotation;
                if (m_grabbedObj.snapOffset)
                {
                    m_grabbedObjectRotOff = m_grabbedObj.snapOffset.rotation * m_grabbedObjectRotOff;
                }
            }
            else
            {
                Quaternion relOri = Quaternion.Inverse(transform.rotation) * m_grabbedObj.transform.rotation;
                m_grabbedObjectRotOff = relOri;
            }

            // NOTE: force teleport on grab, to avoid high-speed travel to dest which hits a lot of other objects at high
            // speed and sends them flying. The grabbed object may still teleport inside of other objects, but fixing that
            // is beyond the scope of this demo.
            //MoveGrabbedObject(m_lastPos, m_lastRot, true);
            // NOTE: This is to get around having to setup collision layers, but in your own project you might
            // choose to remove this line in favor of your own collision layer setup.
            SetPlayerIgnoreCollision(m_grabbedObj.gameObject, true);

            if (m_parentHeldObject)
            {
               m_grabbedObj.transform.parent = transform;
            }
        }

    public override void MoveGrabbedObject(Vector3 pos, Quaternion rot, bool forceTeleport = false)
    {
        if (m_grabbedObj == null)
        {
            return;
        }

        Rigidbody grabbedRigidbody = m_grabbedObj.grabbedRigidbody;
        Vector3 grabbablePosition = pos + rot * m_grabbedObjectPosOff;
        Quaternion grabbableRotation = rot * m_grabbedObjectRotOff;

        if (isAtADistanceSelection)
        {
            if (m_grabbedObj.interactionMethod == OVRGrabbable.InteractionType.grounded)
            {
                //Debug.Log("RunGrounded");
                snapToGround(grabbedRigidbody, pos, rot);
                //AtADistanceMoveObject(grabbedRigidbody, Vector3.zero, pos, rot);
            }
            else if (m_grabbedObj.interactionMethod == OVRGrabbable.InteractionType.free)
            {

            }
            else
            {
                if (forceTeleport)
                {
                    grabbedRigidbody.transform.position = grabbablePosition;
                    grabbedRigidbody.transform.rotation = grabbableRotation;
                }
                else
                {
                    grabbedRigidbody.MovePosition(grabbablePosition);
                    grabbedRigidbody.MoveRotation(grabbableRotation);
                }
            }
        }
        else
        {
            if (forceTeleport)
            {
                grabbedRigidbody.transform.position = grabbablePosition;
                grabbedRigidbody.transform.rotation = grabbableRotation;
            }
            else
            {
               grabbedRigidbody.MovePosition(grabbablePosition);
               grabbedRigidbody.MoveRotation(grabbableRotation);
            }
        }
    }

    void snapToGround(Rigidbody grabbedRigidbody, Vector3 pos, Quaternion rot)
    {
        //Cast a ray that gets the ground position
        //snap the object to the ground only following the user hands X and Y 
        RaycastHit hit;
        Debug.DrawRay(m_grabbedObj.transform.position, -m_grabbedObj.transform.TransformDirection(Vector3.forward), Color.blue);
        AtADistanceMoveObject(grabbedRigidbody,Vector3.zero, pos, rot);
        if (Physics.Raycast(m_grabbedObj.transform.position, -m_grabbedObj.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            //AtADistanceMoveObject(grabbedRigidbody, hit.point, pos, rot);
        }

        //AtADistanceMoveObject();
    }

    void AtADistanceMoveObject(Rigidbody grabbedRigidbody, Vector3 groundPos, Vector3 pos, Quaternion rot)
    {
        float Zpos =  groundPos.z + (m_grabbedObj.gameObject.transform.localScale.z / 2);
        Vector3 position = pos + rot * m_grabbedObjectPosOff;
        Vector3 lockedPosition = new Vector3(position.x, 0.5f, position.z);

        //Debug.Log("X" + lockedPosition.x + " Y" + lockedPosition.y + " Z" + lockedPosition.z);
        m_grabbedObj.transform.position = lockedPosition;


        Quaternion grabbableRotation = rot * m_grabbedObjectRotOff;
        grabbedRigidbody.MoveRotation(grabbableRotation);
    }
}
