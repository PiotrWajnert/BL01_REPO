using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("Portal Settings")]
    public string sceneToLoad;
    public string spawnPointToSet;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            GameControl.instance.targetSpawnPointID = spawnPointToSet;      //gdzie mamy sie pojawic

            SceneManager.LoadScene(sceneToLoad);        //ladujemy nowa scene
        }
    }

}
