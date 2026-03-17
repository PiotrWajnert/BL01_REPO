using UnityEngine;
using System.Collections;

public class NinjaSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject ninjaPrefab;
    public Transform spawnPointA;
    public Transform spawnPointB;
    public float initialSpawnDelay = 3f;
    public float respawnDelay = 3f;

    private int ninjasSpawnedCount = 0;
    private GameObject currentNinja;
    private Transform bruceTransform;
    private bool isWaitingForRespawn = false;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) bruceTransform = player.transform;

        StartCoroutine(SpawnSequence());
    }

    void Update()
    {
        // KLUCZOWA LOGIKA:
        // Jeœli nie ma aktualnie Ninjy na ekranie i nie czekamy ju¿ na respawn...
        if (currentNinja == null && !isWaitingForRespawn && ninjasSpawnedCount > 0)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator SpawnSequence()
    {
        yield return new WaitForSeconds(initialSpawnDelay);
        SpawnNinja();
    }

    IEnumerator RespawnRoutine()
    {
        isWaitingForRespawn = true;

        Debug.Log("Ninja pokonany. Nastêpny za: " + respawnDelay + "s");

        yield return new WaitForSeconds(respawnDelay);

        SpawnNinja();

        isWaitingForRespawn = false;
    }

    void SpawnNinja()
    {
        // Wybieramy punkt: A dla pierwszego, B dla ka¿dego kolejnego
        Transform selectedPoint = (ninjasSpawnedCount == 0) ? spawnPointA : spawnPointB;

        currentNinja = Instantiate(ninjaPrefab, selectedPoint.position, Quaternion.identity);
        ninjasSpawnedCount++;

        NinjaAI ai = currentNinja.GetComponent<NinjaAI>();
        if (ai != null)
        {
            ai.target = bruceTransform;
        }

        Debug.Log("Zespawnowano Ninjê nr " + ninjasSpawnedCount + " w " + selectedPoint.name);
    }
}