using UnityEngine;
using System.Collections;

public class ProjectileSpawner : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float fireRate = 2f; // Co ile sekund strza³
    public float startDelay = 0f;

    void OnEnable() // Uruchamia siê, gdy obiekt zostanie w³¹czony
    {
        StartCoroutine(SpawnRoutine());
    }

    void OnDisable() // Zatrzymuje siê, gdy obiekt zostanie wy³¹czony
    {
        StopAllCoroutines();
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(startDelay);
        while (true)
        {
            // Tworzy pocisk w miejscu spawnera z jego rotacj¹
            GameObject newProjectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
            newProjectile.transform.SetParent(this.transform);
            yield return new WaitForSeconds(fireRate);
        }
    }
}