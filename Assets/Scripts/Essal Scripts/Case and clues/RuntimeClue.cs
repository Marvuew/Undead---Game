using UnityEngine;
[RequireComponent (typeof(Collider2D))]
public class RuntimeClue : MonoBehaviour
{
    public Clue clue;

    public void OnMouseDown()
    {
        CaseManager.instance.AddClue(clue);
    }
}
