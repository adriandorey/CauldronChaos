using System;
using UnityEngine;

public class GoblinCage : MonoBehaviour
{
    private Transform _cage;

    private void Start()
    {
        _cage = GetComponent<Transform>();
    }

    private void OnEnable()
    {
        Actions.OnMoveCage += TipCage;
        Actions.OnResetValues += ResetCage;
    }

    private void OnDisable()
    {
        Actions.OnMoveCage -= TipCage;
        Actions.OnResetValues -= ResetCage;
    }

    private void OnDestroy()
    {
        Actions.OnMoveCage -= TipCage;
        Actions.OnResetValues -= ResetCage;
    }


    private void TipCage()
    {
            _cage.SetLocalPositionAndRotation(new Vector3(-6.25f, 1, 5.864f), Quaternion.Euler(-90, 0, 0));
    }

    private void ResetCage()
    {
            _cage.SetLocalPositionAndRotation(new Vector3(-6.25f, 0.5f, 6.35f), Quaternion.identity);
    }
}