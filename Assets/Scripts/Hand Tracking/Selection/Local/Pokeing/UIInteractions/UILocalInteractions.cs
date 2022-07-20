using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UILocalInteractions : MonoBehaviour
{

    [SerializeField]
    Items thisItem;

    [SerializeField]
    Image iconGO;
    [SerializeField]
    GameObject infomationPanel;
    bool isCoroutineExecuting = false;
    bool hasPlacedItem;
    IEnumerator co;
    public GameObject XR_Rig;

    private void Awake()
    {
        AssignIcon();
    }

    public void AssignIcon()
    {
        if (thisItem == null )
            return;

        iconGO.sprite = thisItem.itemIcon;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BoneTriggerLogic>() != null)
        {
            OnSelect();
            //GetComponent<MeshRenderer>().material = holdMat;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<BoneTriggerLogic>() != null)
        {
            if (infomationPanel != null)
            {
                OnDeselect();
            }
            // GetComponent<MeshRenderer>().material = inactiveMat;
        }
    }

    public void OnSelect()
    {
        UpdateInformationPannel();
        if (!isCoroutineExecuting && !hasPlacedItem)
        {
            co = ExecuteAfterTime();
            StartCoroutine(co);
        }
    }

    public void OnDeselect()
    {
        if (co != null)
        {
            StopCoroutine(co);
            isCoroutineExecuting = false;
        }
        SetInfomationPannel(false);
        hasPlacedItem = false;
    }

    public void UpdateInformationPannel()
    {
        if (infomationPanel != null)
        {
            SetInfomationPannel(true);
            infomationPanel.GetComponent<InformationPannel>().UpdatePannel(thisItem.itemName, thisItem.itemCost.ToString(), thisItem.itemIcon);
        }
    }

    public void SetInfomationPannel(bool setTo)
    {
        if (infomationPanel != null)
        {
            infomationPanel.SetActive(setTo);
        }
    }

    public IEnumerator ExecuteAfterTime()
    {
        Debug.LogWarning("Coroutine Started : ");
        isCoroutineExecuting = true;
        yield return new WaitForSeconds(3.0f);
        Debug.LogWarning("Spawn Object : " + thisItem.itemName);
        Instantiate(thisItem.itemPrefab, XR_Rig.transform.position + (XR_Rig.transform.forward * 2), Quaternion.Euler(new Vector3(-90, 0, 0)));
        isCoroutineExecuting = false;
        hasPlacedItem = true;
    }
}
