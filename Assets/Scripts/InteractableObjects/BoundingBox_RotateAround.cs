using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBox_RotateAround : GrabbleObject
{
    public Vector3 Pivot;
    public bool DebugInfo = true;
    public float angleToForward;

    public bool RotateX = false;
    public bool RotateY = true;
    public bool RotateZ = false;

    public GameObject parentObject;
    public GameObject testObj;
    public float speed = 5f;
    public bool isObjectGrabbed;
    public GameObject handToFollow;
    public Transform localTransfrom;

    public Vector3 handPosition;
    IEnumerator co;
    public void Start()
    {
        localTransfrom = this.gameObject.transform;
    }

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        m_grabbedBy = hand;
        m_grabbedCollider = grabPoint;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;

        Debug.Log(grabPoint.gameObject.transform.name);
        GetComponent<Renderer>().material = selectedMat;
        
        if (co != null)
        {
            StopCoroutine(co);
        }

        //handToFollow = hand.gameObject;
        isObjectGrabbed = true;

        handToFollow = hand.gameObject;
        handPosition = handToFollow.transform.position;
    }

    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        //base.GrabEnd(linearVelocity, angularVelocity);
        DelayedStart();
        Debug.Log("Grab Ended");
        isObjectGrabbed = false;
        handToFollow = null;

        this.gameObject.transform.position = localTransfrom.position;
    }

    private void FixedUpdate()
    {
        if (handToFollow == null || !isObjectGrabbed)
        {
            return;
        }

        if (handToFollow.transform.position.x > handPosition.x)
        {
            var test = Mathf.Abs(handToFollow.transform.position.x - handPosition.x);
            float newRotationSpeed = test * (speed * Time.time);
            //var test = handToFollow.transform.position.y * speed;
            //Debug.LogWarning("Rotation Speed is : " + newRotationSpeed);
            //Debug.LogWarning("Move Left");
        }
        //Debug.Log("Hand Starting Pos = " + (handPosition.x + 0.5f) + "CurrentPos = " + handToFollow.transform.position.x);
        if (handToFollow.transform.position.x <= handPosition.x + 0.2f && handToFollow.transform.position.x >= handPosition.x - 0.2f)
        {
            //Dont Rotate
            //Debug.LogWarning("Dont Rotate");
        }
        else if (handToFollow.transform.position.x < handPosition.x)
        {
            /*            Quaternion rotation = Quaternion.AngleAxis((10 - angleToForward) * Time.deltaTime, (parentObject.transform.up + Pivot));

                        parentObject.transform.rotation = Quaternion.Slerp(parentObject.transform.rotation, rotation, speed * Time.deltaTime);
                        Debug.LogWarning("Move Left");*/

            parentObject.transform.Rotate(0, 10 * Time.deltaTime, 0);
        }
        else if (handToFollow.transform.position.x > handPosition.x)
        {
            Debug.LogWarning("Move Right");
            parentObject.transform.Rotate(0, -10 * Time.deltaTime, 0);
        }
    }

    public void IsUsingController(float rotDir)
    {
        Debug.Log(rotDir);
        if (rotDir <= -1.0)
        {
            parentObject.transform.Rotate(0, -10 * Time.deltaTime, 0);
        }
        if (rotDir >= 1.0)
        {
            parentObject.transform.Rotate(0, 10 * Time.deltaTime, 0);
        }
    }

    public void Rotation()
    {
        //Debug.Log("Rotation");

 //       parentObject.transform.position += (parentObject.transform.rotation * Pivot);



/*        if (RotateZ)
        {
            Vector3 direction = handToFollow.transform.position - parentObject.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angle = angle - angleToForward;
            Quaternion rotation = Quaternion.AngleAxis(angle, (parentObject.transform.up + Pivot));
            //Debug.Log((parentObject.transform.forward + Pivot));
            parentObject.transform.rotation = Quaternion.Slerp(parentObject.transform.rotation, rotation, speed * Time.deltaTime);
        }
        parentObject.transform.position -= (parentObject.transform.rotation * Pivot);
*/
    }

    public void DelayedStart()
    {
        co = WaitForSeconds();
        StartCoroutine(co);
    }
    public void StopDelay()
    {
        if (co != null)
        {
            StopCoroutine(co);
        }
    }

    IEnumerator WaitForSeconds()
    {
        yield return new WaitForSeconds(3f);
        GetComponent<Renderer>().material = deselectedMat;
    }
}
