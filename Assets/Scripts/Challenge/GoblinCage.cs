using UnityEngine;

public class GoblinCage : MonoBehaviour
{
    [SerializeField] private Transform cageModel;

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
            cageModel.SetLocalPositionAndRotation(new Vector3(0f, 0.5f, -1.5f), Quaternion.Euler(-184, 0, 0));
        else
            cageModel.SetLocalPositionAndRotation(new Vector3(0, 0, 0), Quaternion.Euler(-90f, 0, 0));
    }
}