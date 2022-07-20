using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct Options
{
    public string name;
    public UnityEvent onRecognized;
    public UnityEvent onUnrecognized;
}

public class OptionsManager : MonoBehaviour
{

}
