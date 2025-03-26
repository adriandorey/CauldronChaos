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
        Actions.OnMoveCage += MoveCage;
    }

    private void OnDisable()
    {
        Actions.OnMoveCage -= MoveCage;
    }

    private void OnDestroy()
    {
        Actions.OnMoveCage -= MoveCage;
    }

    // Should tip the cage when told to, if not. it will set the cage back to it's normal rotation.
    private void MoveCage(bool tipped)
    {
        if (tipped)
            _cage.SetLocalPositionAndRotation(new Vector3(-6.25f, 1, 5.864f), Quaternion.Euler(-90, 0, 0));
        else
            _cage.SetLocalPositionAndRotation(new Vector3(-6.25f, 0.5f, 6.35f), Quaternion.identity);
    }
}