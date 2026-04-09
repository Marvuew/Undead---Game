using UnityEngine;

public static class AlignmentSave
{
    // + = Humans, - = Undead
    private const string Key = "AlignmentValue";

    public const float Min = -100f;
    public const float Max = 100f;

    public static float Get() => PlayerPrefs.GetFloat(Key, 0f);

    public static void Set(float value)
    {
        float clamped = Mathf.Clamp(value, Min, Max);
        PlayerPrefs.SetFloat(Key, clamped);
        PlayerPrefs.Save();
    }

    public static void AddHumans(float amount) => Set(Get() + Mathf.Abs(amount)); // move right
    public static void AddUndead(float amount) => Set(Get() - Mathf.Abs(amount)); // move left

    public static void ResetToCenter()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}