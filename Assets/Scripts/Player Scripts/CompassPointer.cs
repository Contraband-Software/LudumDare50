using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassPointer : MonoBehaviour
{
    [Header("Details")]
    public float rotationSpeed;
    public string closestEnemy;

    [Header("Important References")]
    public List<Transform>enemyPositions = new List<Transform>();

    private void Awake()
    {
        enemyPositions = new List<Transform>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        rotateCompass(FindClosestEnemy());
    }

    private Vector2 FindClosestEnemy()
    {
        Vector2 closest = new Vector2(Mathf.Infinity, Mathf.Infinity);
        float closestDistanceSquared = Mathf.Infinity;

        foreach(Transform t in enemyPositions)
        {
            float distanceSquared = Mathf.Pow((t.position.x - transform.position.x), 2) + Mathf.Pow((t.position.y - transform.position.y), 2);
            if(distanceSquared < closestDistanceSquared)
            {
                closest = t.position;
                closestDistanceSquared = distanceSquared;
                closestEnemy = t.gameObject.name;
            }
        }

        return closest;
    }

    private void rotateCompass(Vector2 closestPosition)
    {
        float angle = Mathf.Atan2(closestPosition.y - transform.position.y, closestPosition.x - transform.position.x) * Mathf.Rad2Deg - 90f;
        Quaternion reference = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, reference, Time.deltaTime * rotationSpeed);
    }
}
