using System;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static event Action OnEnemyDefeated;

    public static void NotifyEnemyDefeated()
    {
        OnEnemyDefeated?.Invoke();
    }
}