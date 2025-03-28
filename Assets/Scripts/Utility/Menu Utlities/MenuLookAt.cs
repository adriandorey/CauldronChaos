using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuLookAt : MonoBehaviour
{
    [SerializeField] private GameObject lookAtTarget;

    private void Update()
    {
        transform.LookAt(lookAtTarget.transform);
    }
}
