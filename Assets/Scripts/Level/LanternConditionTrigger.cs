using UnityEngine;

public class LanternConditionTrigger : MonoBehaviour
{
    public string requiredItemID; // ID lampionu, na który czekamy
    public GameObject objectToDisable; // Stary spawner
    public GameObject objectToEnable;  // Nowy spawner

    private bool hasSwapped = false;

    void Update()
    {
        if (hasSwapped) return;

        // Sprawdzamy w GameControl, czy lampion o tym ID jest ju¿ w HashSet
        if (GameControl.instance != null && GameControl.instance.IsItemCollected(requiredItemID))
        {
            PerformSwap();
        }
    }

    void PerformSwap()
    {
        if (objectToDisable != null) objectToDisable.SetActive(false);
        if (objectToEnable != null) objectToEnable.SetActive(true);

        hasSwapped = true;
        Debug.Log("Lampion zebrany - pociski zmieni³y kierunek!");
    }
}