using UnityEngine;
using System.Collections.Generic;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] List<GameObject> ActivateWall;
    [SerializeField] List<GameObject> DeactivateWall;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            foreach (GameObject obj in ActivateWall) 
            {
                obj.SetActive(true);
            }
            foreach (GameObject obj in DeactivateWall)
            {
                obj.SetActive(false);
            }
        }
    }
}
