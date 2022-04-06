using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Variables")]
    public Transform target;
    [Range(0.0f, 1.0f)]
    public float CameraDrag = 0;

    [Header("World Effects")]
    //layers that can be seen in each world
    public LayerMask livingWorld;
    public LayerMask deadWorld;

    [Header("Important References")]
    public PlayerController pCon;
    public Camera thisCam;
    private GameObject player;
    // Start is called before the first frame update

    void Start()
    {
        thisCam = gameObject.GetComponent<Camera>();
        thisCam.cullingMask = deadWorld;
        player = GameObject.Find("Player");
        target = player.transform;

        pCon = player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CameraFollow();
    }

    private void CameraFollow()
    {
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);
        //transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position += (targetPos - transform.position) * (1-CameraDrag);
    }

    public void SwitchWorld()
    {
        switch (pCon.currentWorld)
        {
            case PlayerController.World.Alive:
                pCon.currentWorld = PlayerController.World.Dead;
                SwitchToDeadWorld();
                break;

            case PlayerController.World.Dead:
                pCon.currentWorld = PlayerController.World.Alive;
                SwitchToLivingWorld();
                break;

            default:
                break;
        }
    }

    private void SwitchToDeadWorld()
    {
        thisCam.cullingMask = deadWorld;
    }
    private void SwitchToLivingWorld()
    {
        thisCam.cullingMask = livingWorld;
    }
}
