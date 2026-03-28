using UnityEngine;

[CreateAssetMenu(fileName = "NewCase", menuName = "Cases/Case")]
public class CaseData : ScriptableObject
{
    public string caseName;
    public Sprite caseImage;
    [TextArea] public string caseDescription;


}
