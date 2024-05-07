using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainsGlb : MonoBehaviour
{
    public event Action<BrainsGlb> OnDestroyed = delegate { };

    public string Url;

    public void OnDestroy()
    {
        OnDestroyed?.Invoke(this);
    }

}
