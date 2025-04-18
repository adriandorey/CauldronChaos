using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wind", menuName = "Challenge/Windy Day")]
public class WindSo : ScriptableObject
{
   public float windStrength = 5f;
   public float windMaxChangeTime = 30;
   public float windMinChangeTime = 15;
}
