using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TimerTest : MonoBehaviour
{
    [SerializeField]
    public OVRGrabber hand;
    bool isCoroutineExecuting = false;
    IEnumerator co;

    [SerializeField]
    Image timerSprite;

    float waitTime = 2f;
    public void StartTimer()
    {
        if (isCoroutineExecuting)
            return;
        timerSprite.fillAmount = 0.0f;
        co = ExecuteAfterTime();
        StartCoroutine(co);
    }

    public void StopTimer()
    {
        StopCoroutine(co);
        isCoroutineExecuting = false;
    }

    public void Update()
    {
        //Debug.LogWarning(hand.m_grabbedObj);
        if (isCoroutineExecuting)
        {
            timerSprite.fillAmount += 1.0f / waitTime * Time.deltaTime;
        }
        else
        {
            timerSprite.fillAmount = 0;
        }
    }

    IEnumerator ExecuteAfterTime()
    {
        if (isCoroutineExecuting)
        {
            yield break;
        }

        isCoroutineExecuting = true;
        yield return new WaitForSeconds(2f);

        if (hand.m_grabbedObj != null)
        {
            Debug.Log("grabbed object Not Null");
            if (hand.m_grabbedObj.GetComponent<OVRGrabbable>())
            {
                Debug.Log("grabbed object is grabbable");
                Destroy(hand.m_grabbedObj.transform.parent.gameObject);
                hand.m_grabbedObj = null;
            }
        }
        //Code to execture
        isCoroutineExecuting = false;
    }
}
