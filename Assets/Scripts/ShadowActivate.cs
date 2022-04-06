using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShadowActivate : MonoBehaviour
{
    
    private void Awake()
    {
        GetComponent<TilemapRenderer>().receiveShadows = true;
        GetComponent<TilemapRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }
}
