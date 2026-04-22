using UnityEngine;

public class BlackFadeAnimationScript : MonoBehaviour
{
   public void SetAnimatingBool()
   {
        AnimationManager.instance.fadeHappening = false;
   }
}
