using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateGameObject : MonoBehaviour
{
    public float timer = 2f;

    void Start()
    {
        Invoke("DeactivateAfterTime", timer);
    }

    void DeactivateAfterTime()
    {
        gameObject.SetActive(false);
    }
}
