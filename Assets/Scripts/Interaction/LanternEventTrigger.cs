using UnityEngine;
using UnityEngine.SceneManagement;

public enum TriggerAction { Show, Hide, Move }

public class LanternEventTrigger : MonoBehaviour
{
    [Header("Warunek")]
    public int lanternsRequired; // Ile lampionów trzeba zebraæ?
    public string requiredSceneName;

    [Header("Akcja")]
    public TriggerAction action;
    public GameObject targetObject; // Co ma siê pojawiæ/znikn¹æ?

    [Header("Ruch obiektu")]
    public Transform moveTarget; // obiekt do ktorego ma dojechac zapadnia
    public float moveSpeed = 2f;

    [Header("Unikalne ID")]
    public string eventID; // Wpisz tu np. "Zapadnia_Poziom2"

    private bool hasTriggered = false;
    private bool isMoving = false;

    void Start()
    {
        // Jeœli to ID jest ju¿ zapisane, od razu ustaw platformê u celu
        if (GameControl.instance != null && GameControl.instance.IsItemCollected(eventID))
        {
            hasTriggered = true;
            isMoving = false; // Nie chcemy, ¿eby jecha³a

            if (action == TriggerAction.Move && targetObject != null && moveTarget != null)
            {
                targetObject.transform.position = moveTarget.position;
            }
            else if (action == TriggerAction.Show && targetObject != null)
            {
                targetObject.SetActive(true);
            }
            else if (action == TriggerAction.Hide && targetObject != null)
            {
                targetObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        //sprawdzenie sceny
        if (!string.IsNullOrEmpty(requiredSceneName))
        {
            if (SceneManager.GetActiveScene().name != requiredSceneName) return;
        }


        // Ruch
        if (isMoving)
        {
            if (targetObject != null && moveTarget != null)
            {
                float distance = Vector3.Distance(targetObject.transform.position, moveTarget.position);

                // P³ynny ruch
                targetObject.transform.position = Vector3.MoveTowards(
                    targetObject.transform.position,
                    moveTarget.position,
                    moveSpeed * Time.deltaTime
                );

                if (distance < 0.05f)
                {
                    targetObject.transform.position = moveTarget.position;
                    isMoving = false;
                }
            }
        }

        if (hasTriggered) return;   // Jeœli ju¿ raz zadzia³a³o, nie sprawdzaj wiêcej

        if (GameControl.instance != null && GameControl.instance.lanternsCollected >= lanternsRequired)
        {
            ExecuteAction();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (targetObject != null && moveTarget != null)
        {
            Debug.DrawLine(targetObject.transform.position, moveTarget.position, Color.red);

        }
    }
    void ExecuteAction()
    {
        hasTriggered = true;

        // Zapisujemy w GameControl, ¿e to zdarzenie mia³o miejsce
        if (!string.IsNullOrEmpty(eventID) && GameControl.instance != null)
        {
            GameControl.instance.RegisterCollection(eventID);
        }

        switch (action)
        {
            case TriggerAction.Show:
                if (targetObject != null) targetObject.SetActive(true);
                break;
            case TriggerAction.Hide:
                if (targetObject != null) targetObject.SetActive(false);
                break;
            case TriggerAction.Move:
                if (targetObject != null && moveTarget != null) isMoving = true;
                break;
        }
    }
}