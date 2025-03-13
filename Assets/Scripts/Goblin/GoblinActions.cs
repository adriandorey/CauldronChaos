using System;
using UnityEngine;

[Serializable]
public struct GoblinActions
{
    public string actionName;
    [Tooltip("Needs to be from 0 to 1. All actions together should equal 1.")]
    public float weight;
    public Action ActionToExecute;
}
