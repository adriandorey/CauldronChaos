using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cauldron Movement", menuName = "Challenge/Cauldron Movement")]
public class CauldronMovementSo : ScriptableObject
{
    [Header("Movement Settings")]
    public float cauldronMinDistance = 1f;
    public float wanderRadius = 8f;
    public float minMovementTime = 3f;
    public float maxMovementTime = 10f;

    [Header("Animation Settings")]
    public float liftAmount = 0.4f;
}
