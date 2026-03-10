using UnityEngine;

 [CreateAssetMenu(menuName = "Dialogue/Speaker Database")]
    public class SpeakerDatabase : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public Speakers speaker;
            public Sprite portrait;
        }

        [SerializeField] private static Entry[] entries;

        public static Sprite GetSprite(Speakers speaker)
        {
            foreach (Entry entry in entries)
            {
                if (entry.speaker == speaker)
                {
                    return entry.portrait;
                }
            }
            Debug.LogWarning($"No sprite found for {speaker}");
            return null;
        }
    }

