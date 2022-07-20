using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : OVRGrabbable
{
    int currentView = 0;

    public GameObject go_boundingBox;
    public GameObject go_UI;

    protected override void Start()
    {
        base.Start();
    }

    public void SwitchView()
    {
        switch (currentView)
        {
            case 1:
                //Show interaction options
                break;
            case 2:
                //Show UI style choices
                break;
            case 3:
                currentView = 0;
                //Show default view
                break;
            default:
                break;
        }
    }

    public void ShowOptions(GameObject optionToShow)
    {
        if (optionToShow == go_boundingBox)
        {
            HideOptions(go_UI);
        }
        else if(optionToShow == go_UI)
        {
            HideOptions(go_boundingBox);
        }
    }
    public void HideOptions(GameObject optionToHide)
    { 
    
    }
}
