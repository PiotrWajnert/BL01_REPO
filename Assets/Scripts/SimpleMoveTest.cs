using UnityEngine;

public class SimpleMoveTest : MonoBehaviour
{
    public Transform targetPoint; // Punkt docelowy
    public float speed = 2f;      // Prêdkoœæ ruchu

    void Update()
    {
        if (targetPoint != null)
        {
            // Ruch klatka po klatce
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPoint.position,
                speed * Time.deltaTime
            );

            // Rysowanie linii pomocniczej w oknie Scene
            Debug.DrawLine(transform.position, targetPoint.position, Color.green);
        }
    }
}