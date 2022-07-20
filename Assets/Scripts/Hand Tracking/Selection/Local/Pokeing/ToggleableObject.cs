using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleableObject : MonoBehaviour
{

    [SerializeField]
    private Material activeMat;
    [SerializeField]
    private Material holdMat;
    [SerializeField]
    private Material inactiveMat;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BoneTriggerLogic>() != null)
        {
            Debug.Log("Entered hover Trigger " + other.name);
            //GetComponent<MeshRenderer>().material = holdMat;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<BoneTriggerLogic>() != null)
        {
           // GetComponent<MeshRenderer>().material = inactiveMat;
        }
    }
}
