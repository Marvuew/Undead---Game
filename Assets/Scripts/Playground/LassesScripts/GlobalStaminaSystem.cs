using UnityEngine;

public static class GlobalStaminaSystem
{
    private const string CurrentKey = "GlobalStamina_Current";
    private const string MaxKey = "GlobalStamina_Max";

    private const int DefaultMaxStamina = 10;

    public static int GetMaxStamina()
    {
        int max = PlayerPrefs.GetInt(MaxKey, DefaultMaxStamina);
        return Mathf.Max(1, max);
    }

    public static int GetCurrentStamina()
    {
        int current = PlayerPrefs.GetInt(CurrentKey, GetMaxStamina());
        return Mathf.Clamp(current, 0, GetMaxStamina());
    }

    public static void InitializeIfNeeded(int defaultMax = DefaultMaxStamina)
    {
        if (!PlayerPrefs.HasKey(MaxKey))
            PlayerPrefs.SetInt(MaxKey, Mathf.Max(1, defaultMax));

        if (!PlayerPrefs.HasKey(CurrentKey))
            PlayerPrefs.SetInt(CurrentKey, PlayerPrefs.GetInt(MaxKey));

        ClampAndSave();
    }

    public static void SetMaxStamina(int newMax, bool fillToMax = false)
    {
        newMax = Mathf.Max(1, newMax);
        PlayerPrefs.SetInt(MaxKey, newMax);

        int current = GetCurrentStamina();
        if (fillToMax)
            current = newMax;

        PlayerPrefs.SetInt(CurrentKey, Mathf.Clamp(current, 0, newMax));
        PlayerPrefs.Save();
    }

    public static void SetCurrentStamina(int amount)
    {
        PlayerPrefs.SetInt(CurrentKey, Mathf.Clamp(amount, 0, GetMaxStamina()));
        PlayerPrefs.Save();
    }

    public static bool HasEnough(int amount)
    {
        amount = Mathf.Max(0, amount);
        return GetCurrentStamina() >= amount;
    }

    public static bool TrySpend(int amount)
    {
        amount = Mathf.Max(0, amount);

        if (!HasEnough(amount))
            return false;

        SetCurrentStamina(GetCurrentStamina() - amount);
        return true;
    }

    public static void Restore(int amount)
    {
        amount = Mathf.Max(0, amount);
        SetCurrentStamina(GetCurrentStamina() + amount);
    }

    public static void RefillToMax()
    {
        SetCurrentStamina(GetMaxStamina());
    }

    public static void ResetAll(int defaultMax = DefaultMaxStamina)
    {
        PlayerPrefs.SetInt(MaxKey, Mathf.Max(1, defaultMax));
        PlayerPrefs.SetInt(CurrentKey, Mathf.Max(1, defaultMax));
        PlayerPrefs.Save();
    }

    private static void ClampAndSave()
    {
        int max = Mathf.Max(1, PlayerPrefs.GetInt(MaxKey, DefaultMaxStamina));
        int current = Mathf.Clamp(PlayerPrefs.GetInt(CurrentKey, max), 0, max);

        PlayerPrefs.SetInt(MaxKey, max);
        PlayerPrefs.SetInt(CurrentKey, current);
        PlayerPrefs.Save();
    }
}