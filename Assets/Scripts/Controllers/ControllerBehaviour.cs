using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ControllerBehaviour : OVRGrabber
{
    public enum currentHand
    { 
        Right,
        Left
    }

    [SerializeField]
    public currentHand controllerSide;
    [SerializeField]
    GameObject rayCastPoint;
    [SerializeField]
    GameObject go_XRRig;
    bool teleportDoOnce = false;
    [SerializeField]
    float grabRadius = 0.1f;

    private int grabbableLayerMask;
    bool previousTriggerValue = false;
    bool previousPrimaryButtonValue = false;
    bool previousSecondaryButtonValue = false;
    bool isAtADistanceSelection = false;


    [SerializeField]
    LineRenderer line;
    int lengthOfLine;

    [SerializeField]
    TimerTest timer;
    [SerializeField]
    GameObject menu;

    UILocalInteractions UIHit;
    UILocalInteractions previousHit;
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
        //Initizilize Controllers
        var controller = new List<UnityEngine.XR.InputDevice>();

        if (controllerSide == currentHand.Right)
        {
            var controllerDesiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller;
            GetController(controller, controllerDesiredCharacteristics, this.gameObject);
        }
        else if (controllerSide == currentHand.Left)
        {
            var controllerDesiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller;
            GetController(controller, controllerDesiredCharacteristics, this.gameObject);
        }
    }

    void GetController(List<UnityEngine.XR.InputDevice> handSide, UnityEngine.XR.InputDeviceCharacteristics characteristics, GameObject hand)
    {
        var handControllers = handSide;
        var desiredCharacteristics = characteristics;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, handControllers);

        foreach (var device in handControllers)
        {
            TriggerLogic(device, hand);
            ButtonLogic(device);
            JoyStickLogic(device);
        }
    }

    public void TriggerLogic(InputDevice device, GameObject hand)
    {
        Debug.DrawLine(transform.position, rayCastPoint.transform.forward);

        bool triggerValue;
        //Stops the player from teleporting when they have a item.
        if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue)
        {
            VisualizeAtADistanceSelection();

            RaycastHit hit;
            if (Physics.Raycast(transform.position, rayCastPoint.transform.forward, out hit, Mathf.Infinity))
            {
                if (hit.transform.GetComponent<UILocalInteractions>() != null)
                {                    
                    if (previousHit == null)
                    {
                        previousHit = hit.transform.GetComponent<UILocalInteractions>();
                    }

                    if (previousHit != null && previousHit != hit.transform.GetComponent<UILocalInteractions>())
                    {
                        UIHit = hit.transform.GetComponent<UILocalInteractions>();
                        UIHit.OnSelect();
                    }
                }
                else if (hit.transform.GetComponent<OVRGrabbable>() != null)
                {
                    if (m_grabbedObj == null)
                    {
                        Debug.Log("isCalled");
                        m_grabbedObj = hit.transform.GetComponent<OVRGrabbable>();
                        Collider grabbableCollider = null;
                        for (int i = 0; i < m_grabbedObj.grabPoints.Length; i++)
                        {
                            grabbableCollider = m_grabbedObj.grabPoints[i];
                        }
                        isAtADistanceSelection = true;
                        SetUpGrabbedObject(grabbableCollider);

                        //Haptics
                        UnityEngine.XR.HapticCapabilities capabilities;
                        if (device.TryGetHapticCapabilities(out capabilities))
                        {
                            if (capabilities.supportsImpulse)
                            {
                                uint channel = 0;
                                float amplitude = 0.25f;
                                float duration = 0.5f;
                                device.SendHapticImpulse(channel, amplitude, duration);
                            }
                        }
                    }
                }
            }
            else
            {
                if (previousHit != null)
                {
                    previousHit.OnDeselect();
                    previousHit = null;
                }
            }
            previousTriggerValue = triggerValue;
        }
        else if (triggerValue != previousTriggerValue)
        {
            if (controllerSide == currentHand.Right && m_grabbedObj == null)
            {
                Vector3 teleportPos = TeleportationLogic();
                go_XRRig.transform.position = new Vector3(teleportPos.x, go_XRRig.transform.position.y, teleportPos.z);
            }
            if (m_grabbedObj != null)
            {            
                GrabEnd();
            }
            else if (UIHit != null)
            {
                UIHit.OnDeselect();
                UIHit = null;
            }
            //When the trigger is realsed.
            previousTriggerValue = triggerValue;
            if (line != null)
            {
                line.gameObject.SetActive(false);
            }
        }
    }

    public void ButtonLogic(InputDevice device)
    {
        if (controllerSide == currentHand.Right)
            return;
        bool primaryButtonValue;
        if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out primaryButtonValue) && primaryButtonValue)
        {
            previousPrimaryButtonValue = primaryButtonValue;
            menu.SetActive(true);
        }
        else if (primaryButtonValue != previousPrimaryButtonValue)
        {
            menu.SetActive(false);
            previousPrimaryButtonValue = primaryButtonValue;
        }

        bool secondaryButtonValue;
        if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out secondaryButtonValue) && secondaryButtonValue)
        {
            timer.StartTimer();
            previousSecondaryButtonValue = secondaryButtonValue;
        }
        else if (secondaryButtonValue != previousSecondaryButtonValue)
        {
            timer.StopTimer();
            previousSecondaryButtonValue = secondaryButtonValue;
        }
    }

    public void JoyStickLogic(InputDevice device)
    {
        Vector2 stickMovement;
        if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out stickMovement))
        {
            if (m_grabbedObj != null && m_grabbedObj.GetComponent<BoundingBox_RotateAround>() != null)
            {
                m_grabbedObj.GetComponent<BoundingBox_RotateAround>().IsUsingController(Mathf.Round(stickMovement.x));
            }
        }
    }

    public Vector3 TeleportationLogic()
    {
        RaycastHit hit;
        if (Physics.Raycast(this.gameObject.transform.position, this.gameObject.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            if (hit.transform.gameObject.tag == "Ground")
            {
                return hit.point;
            }
        }
        return go_XRRig.transform.localPosition;
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
                SnapToGround(grabbedRigidbody, pos, rot);
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

    void SnapToGround(Rigidbody grabbedRigidbody, Vector3 pos, Quaternion rot)
    {
        //Cast a ray that gets the ground position
        //snap the object to the ground only following the user hands X and Y 
        RaycastHit hit;
        Debug.DrawRay(m_grabbedObj.transform.position, -m_grabbedObj.transform.TransformDirection(Vector3.forward), Color.blue);
        AtADistanceMoveObject(grabbedRigidbody, Vector3.zero, pos, rot);
        if (Physics.Raycast(m_grabbedObj.transform.position, -m_grabbedObj.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            //AtADistanceMoveObject(grabbedRigidbody, hit.point, pos, rot);
        }

        //AtADistanceMoveObject();
    }

    void AtADistanceMoveObject(Rigidbody grabbedRigidbody, Vector3 groundPos, Vector3 pos, Quaternion rot)
    {
        float Zpos = groundPos.z + (m_grabbedObj.gameObject.transform.localScale.z / 2);
        Vector3 position = pos + rot * m_grabbedObjectPosOff;
        Vector3 lockedPosition = new Vector3(position.x, 0.5f, position.z);

        //Debug.Log("X" + lockedPosition.x + " Y" + lockedPosition.y + " Z" + lockedPosition.z);
        m_grabbedObj.transform.position = lockedPosition;


        Quaternion grabbableRotation = rot * m_grabbedObjectRotOff;
        grabbedRigidbody.MoveRotation(grabbableRotation);
    }

    private void VisualizeAtADistanceSelection()
    {
        //Debug.Log("Visualising Line");
        if (line != null)
        {
            line.gameObject.SetActive(true);
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
                    points[i] = (rayCastPoint.transform.position + rayCastPoint.transform.forward);
                    //Debug.Log(hitLocation);
                }
            }
            line.SetPositions(points);

            //Debug.Log(" " + rayCastPoint.transform.position);
        }
    }
}
