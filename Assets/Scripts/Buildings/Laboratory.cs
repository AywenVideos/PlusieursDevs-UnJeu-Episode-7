using System.Collections.Generic;
using UnityEngine;

public class Laboratory : MonoBehaviour
{
    public static List<Laboratory> All = new();
    
    private void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
    }
}