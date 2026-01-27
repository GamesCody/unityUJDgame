using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DEPRECATED - Use DzialkoLaserowe spawner script instead.
/// This file is kept for backward compatibility only.
/// </summary>
[System.Obsolete("Use DzialkoLaserowe script directly as spawner instead.")]
public class DzialkoLaseroweSpawner : MonoBehaviour
{
    void Start()
    {
        Debug.LogWarning("[DzialkoLaseroweSpawner] This spawner is deprecated. Use DzialkoLaserowe script as spawner on your GameManager/spawner object instead.", this);
    }
}