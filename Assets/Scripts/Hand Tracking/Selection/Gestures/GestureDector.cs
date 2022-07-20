using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;


[System.Serializable]
public struct Gesture
{
    public string name;
    public List<Vector3> fingerData;
    public UnityEvent onRecognized;
    public UnityEvent onUnrecognized;
}


public class GestureDector : MonoBehaviour
{
    [Header("Threshold value")]
    public float threshold = 0.1f;

    [Header("Hand Skeleton")]
    public OVRSkeleton skeleton;

    [Header("List of Gestures")]
    public List<Gesture> gestures;

    [Header("Debug Mode")]
    public bool debugMode;


    bool hasStarted = false;
    bool hasRecognized = false;
    bool done = false;

    [Header("Unrecognized Event")]
    public UnityEvent notRecognize;

    [SerializeField]
    private List<OVRBone> fingerBones;
    Gesture previousGesture;
    Gesture currentGesture;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DelayRoutine(2.5f, Initialize));
        previousGesture = new Gesture();
    }

    public IEnumerator DelayRoutine(float delay, Action actionToDo)
    {
        yield return new WaitForSeconds(delay);
        actionToDo.Invoke();
    }

    public void Initialize()
    {
        fingerBones = new List<OVRBone>(skeleton.Bones);
        hasStarted = true;
        Debug.Log("[VR Gestures] Initialized -------------------------------------------");
    }
    // Update is called once per frame
    void Update()
    {
        // if in debug mode and we press Space, we will save a gesture
        if (debugMode && Input.GetKeyDown(KeyCode.Space))
        {
            Save();
        }

        //if the initialization was successful
        if (hasStarted.Equals(true))
        {
            // start to Recognize every gesture we make
            Gesture currentGesture = Recognize();

            // we will associate the recognize to a boolean to see if the Gesture
            // we are going to make is one of the gesture we already saved

            hasRecognized = !currentGesture.Equals(new Gesture());
            if (hasRecognized)
            {
                done = true;

               // Debug.Log("Current Gesture " + currentGesture.name);
                currentGesture.onRecognized?.Invoke();
                previousGesture = currentGesture;
            }
            else
            {
                if (done)
                {
                    //Debug.Log("Not Recognized");
                    done = false;
                    previousGesture.onUnrecognized?.Invoke();
                    notRecognize?.Invoke();
                }
            }
        }
    }

    void Save()
    {
        //Creates a new Gesture when called
        Gesture g = new Gesture();
        g.name = "New Gesture";
        List<Vector3> data = new List<Vector3>();
        foreach (var bone in fingerBones)
        {
            //Gives the finger position based on the transform.end
            data.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
        }

        g.fingerData = data;
        gestures.Add(g);
    }

    Gesture Recognize()
    {
        Gesture currentGesture = new Gesture();

        float currentMin = Mathf.Infinity;

        foreach (var gesture in gestures)
        {
            float sumDistance = 0;
            bool isDiscarded = false;
            for (int i = 0; i < fingerBones.Count; i++)
            {
                Vector3 currentData = skeleton.transform.InverseTransformPoint(fingerBones[i].Transform.position);

                float distance = Vector3.Distance(currentData, gesture.fingerData[i]);
                if (distance > threshold)
                {
                    isDiscarded = true;
                    break;
                }
                sumDistance += distance;
            }

            if (!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentGesture = gesture;
            }
        }
        return currentGesture;

    }
}
