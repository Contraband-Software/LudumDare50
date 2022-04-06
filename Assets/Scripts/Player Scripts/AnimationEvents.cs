using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Important References")]
    public PlayerController pCon;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AttackFinished()
    {
        pCon.attacking = false;
        pCon.attackAnimationTimeLeft = 0f;
    }
}
