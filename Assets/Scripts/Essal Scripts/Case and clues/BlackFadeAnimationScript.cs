using UnityEngine;

public class BlackFadeAnimationScript : MonoBehaviour
{
   public void SetAnimatingBool()
   {
        Debug.Log("BOOL!");
        AnimationManager.instance.fadeHappening = false;
   }
}
