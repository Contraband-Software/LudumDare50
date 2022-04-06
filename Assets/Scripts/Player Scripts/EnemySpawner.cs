using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Parameters")]
    public Vector2 bottomLeftMap;
    public Vector2 topRightMap;
    public float minSpawnDistance;
    public int maxEnemies;
    public float timeDelay;
    private float countdown;
    private bool startedEnemySpawn;

    [Header("Human Prefabs")]
    public List<GameObject> humans = new List<GameObject>();

    [Header("Important References")]
    public GameObject humansParentObject;

    private bool initialMapPopulation = false;
    private float tempTimeDelay;
    private float tempMinSpawnDistance;

    // Start is called before the first frame update
    void Start()
    {
        startedEnemySpawn = false;
        initialMapPopulation = false;

        tempTimeDelay = timeDelay;
        tempMinSpawnDistance = minSpawnDistance;
        //SpawnSurge();
    }

    // Update is called once per frame
    void Update()
    {

        if (!initialMapPopulation)
        {
            SpawnSurge();
        }
        else
        {
            SpawnCountdown();
        }
    }

    //initially randomly place all the blokes
    private void SpawnSurge()
    {

        
        if(shouldSpawnEnemy() && initialMapPopulation == false)
        {
            timeDelay = 0f;
            minSpawnDistance = 0f;
            SpawnEnemy();
            SpawnSurge();
        }
        if (!shouldSpawnEnemy())
        {
            minSpawnDistance = tempMinSpawnDistance;
            timeDelay = tempTimeDelay;
            initialMapPopulation = true;
        }

       
        

        //print("Spawned Enemies: " + humansParentObject.transform.childCount.ToString());
    }

    public void SpawnCountdown()
    {
        if (shouldSpawnEnemy() && !startedEnemySpawn)
        {
            startedEnemySpawn = true;
            countdown = timeDelay;
        }

        if (startedEnemySpawn)
        {
            countdown -= Time.deltaTime;

            if(countdown <= 0f)
            {
                SpawnEnemy();
                startedEnemySpawn = false;
            }
        }
    }


    public void SpawnEnemy()
    {

        float ranX = Random.Range(bottomLeftMap.x, topRightMap.x);
        float ranY = Random.Range(bottomLeftMap.y, topRightMap.y);
        Vector3 spawnPos = new Vector3(ranX, ranY);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(spawnPos, out hit, 1f, NavMesh.AllAreas))
        {
            Vector2 diff = (hit.position - gameObject.transform.position);

            if(diff.magnitude >= minSpawnDistance)
            {
                InstatiateRandomEnemy(hit.position);
            }
            else
            {
                SpawnEnemy();
            }

        }
        else
        {
            SpawnEnemy();
        }

    }

    public void SpawnEnemyClose()
    {

        float ranX = Random.Range(bottomLeftMap.x, topRightMap.x);
        float ranY = Random.Range(bottomLeftMap.y, topRightMap.y);
        Vector3 spawnPos = new Vector3(ranX, ranY);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(spawnPos, out hit, 1f, NavMesh.AllAreas))
        {
            InstatiateRandomEnemy(hit.position);

        }
        else
        {
            SpawnEnemyClose();
        }

    }

    public void InstatiateRandomEnemy(Vector3 spawnPos)
    {
        int humanIndex = Mathf.FloorToInt(Random.Range(0, humans.Count));
        GameObject humanToSpawn = humans[humanIndex];
        GameObject spawnedHuman = Instantiate(humanToSpawn, spawnPos, Quaternion.identity, null);
        spawnedHuman.transform.SetParent(humansParentObject.transform);
    }

    private bool shouldSpawnEnemy()
    {
        int currentEnemies = 0;
        foreach(Transform child in humansParentObject.transform)
        {
            if(child.gameObject.layer == LayerMask.NameToLayer("AliveEnemy"))
            {
                currentEnemies++;
            }
        }

        if(currentEnemies < maxEnemies)
        {
            return true;
        }
        return false;
    }
}
