using UnityEngine;

public static class ClueSaveSystem
{
    private const string Prefix = "ClueUnlocked_";

    public static bool Unlock(string clueId)
    {
        if (string.IsNullOrWhiteSpace(clueId))
            return false;

        string key = Prefix + clueId;
        bool alreadyUnlocked = PlayerPrefs.GetInt(key, 0) == 1;

        if (alreadyUnlocked)
            return false;

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        Debug.Log($"Unlocked clue: {clueId}");
        return true;
    }

    public static bool IsUnlocked(string clueId)
    {
        if (string.IsNullOrWhiteSpace(clueId))
            return false;

        return PlayerPrefs.GetInt(Prefix + clueId, 0) == 1;
    }

    public static void Lock(string clueId)
    {
        if (string.IsNullOrWhiteSpace(clueId))
            return;

        PlayerPrefs.DeleteKey(Prefix + clueId);
        PlayerPrefs.Save();

        Debug.Log($"Locked clue: {clueId}");
    }
}