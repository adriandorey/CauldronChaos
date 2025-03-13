using System;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

public class CounterBoundary : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ingredient")) return;
        
        var startPos = other.gameObject.transform.position;
        var randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(0.5f, 2f));
        var endPos = startPos + randomDirection * 3f;
        other.gameObject.transform.DOJump(endPos, 2, 1, 1);
    }
}
