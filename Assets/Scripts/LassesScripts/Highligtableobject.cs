using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TMP_Text))]
public class HighlightableTMP : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("ID (must be unique per text block)")]
    [Tooltip("Example: 'char_vampire_intro' or 'char_fae_notes_1'")]
    [SerializeField] private string highlightId = "character_text_1";

    [Header("Highlight style (RGBA hex: #RRGGBBAA)")]
    [Tooltip("More saturated + see-through default. Example: #FFEB3B66 (yellow, ~40% alpha)")]
    [SerializeField] private string markColorHex = "#FFEB3B66";          // saturated yellow, transparent

    [Tooltip("Selected highlight color (when clicked). Example: #FFC10780 (~50% alpha)")]
    [SerializeField] private string selectedMarkColorHex = "#FFC10780";  // amber, slightly less transparent

    [Header("Input")]
    [Tooltip("If true, must hold Shift while dragging to create highlights. Clicking still selects existing highlights.")]
    [SerializeField] private bool requireShiftToHighlight = false;

    [Header("Page stacking support")]
    [Tooltip("Call Refresh() when your page becomes visible if you use stacked pages (not SetActive).")]
    [SerializeField] private bool warnIfRaycastLikelyBlocked = true;

    private TMP_Text tmp;
    private string baseText; // text without <mark> tags (what we apply highlights onto)

    private bool isDragging;
    private int dragStartCharIndex = -1;   // character-order index
    private int dragCurrentCharIndex = -1; // character-order index

    [Serializable]
    private struct Range
    {
        public int start; // inclusive (character-order)
        public int end;   // exclusive (character-order)
    }

    [Serializable]
    private class SaveData
    {
        public List<Range> ranges = new List<Range>();
    }

    private List<Range> ranges = new List<Range>();
    private int selectedRangeIndex = -1;

    private string SaveKey => $"TMP_Highlights_{highlightId}";

    private void Awake()
    {
        tmp = GetComponent<TMP_Text>();
        baseText = StripMarkTags(tmp.text);

        Load();
        ApplyHighlights();

        if (warnIfRaycastLikelyBlocked)
        {
            // This doesn't guarantee anything, but it catches the common “stacked layers block clicks” mistake:
            // If this is a TextMeshProUGUI and Raycast Target is off, you will NEVER get pointer events.
            var ui = tmp as TextMeshProUGUI;
            if (ui != null && ui.raycastTarget == false)
                Debug.LogWarning($"HighlightableTMP: '{name}' has Raycast Target OFF. Pointer events won't reach this text.");
        }
    }

    private void Update()
    {
        // Delete highlight: click highlight first, then Backspace
        if (selectedRangeIndex >= 0 && Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            ranges.RemoveAt(selectedRangeIndex);
            selectedRangeIndex = -1;

            Save();
            ApplyHighlights();
        }
    }

    /// <summary>
    /// Call this when you open the book / show this page if pages are stacked and not toggled active.
    /// It reloads saved highlights and re-applies them.
    /// </summary>
    public void Refresh()
    {
        // In case the designer edited the underlying text in TMP:
        baseText = StripMarkTags(tmp.text);

        Load();
        selectedRangeIndex = -1;
        ApplyHighlights();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (tmp == null) return;

        // If require shift, only gate creating NEW highlights (still allow selecting existing highlight)
        if (requireShiftToHighlight && Keyboard.current != null)
        {
            bool shift = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            if (!shift)
            {
                TrySelectHighlight(eventData);
                return;
            }
        }

        int charIndex = GetCharacterOrderIndexUnderPointer(eventData);
        if (charIndex < 0) return;

        // If click is inside existing highlight, select it instead of creating new
        if (TrySelectHighlightAtChar(charIndex))
            return;

        isDragging = true;
        dragStartCharIndex = charIndex;
        dragCurrentCharIndex = charIndex;
        selectedRangeIndex = -1;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        int charIndex = GetCharacterOrderIndexUnderPointer(eventData);
        if (charIndex < 0) return;

        dragCurrentCharIndex = charIndex;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        int a = dragStartCharIndex;
        int b = dragCurrentCharIndex;

        dragStartCharIndex = -1;
        dragCurrentCharIndex = -1;

        if (a < 0 || b < 0) return;

        int start = Mathf.Min(a, b);
        int endInclusive = Mathf.Max(a, b);
        int endExclusive = endInclusive + 1;

        // Too small highlight (single click) -> do nothing
        if (endExclusive - start < 2) return;

        AddRangeAndMerge(start, endExclusive);
        Save();
        ApplyHighlights();
    }

    private bool TrySelectHighlight(PointerEventData eventData)
    {
        int charIndex = GetCharacterOrderIndexUnderPointer(eventData);
        if (charIndex < 0) return false;
        return TrySelectHighlightAtChar(charIndex);
    }

    private bool TrySelectHighlightAtChar(int charIndex)
    {
        for (int i = 0; i < ranges.Count; i++)
        {
            if (charIndex >= ranges[i].start && charIndex < ranges[i].end)
            {
                selectedRangeIndex = i;
                ApplyHighlights();
                return true;
            }
        }

        selectedRangeIndex = -1;
        ApplyHighlights();
        return false;
    }

    private int GetCharacterOrderIndexUnderPointer(PointerEventData eventData)
    {
        Camera cam = eventData.pressEventCamera;

        // Finds the character index in TMP's laid-out text (character order, not raw string index)
        int charIndex = TMP_TextUtilities.FindIntersectingCharacter(tmp, eventData.position, cam, true);
        if (charIndex == -1) return -1;

        // Clamp to baseText length just in case
        tmp.ForceMeshUpdate();
        int visibleCount = tmp.textInfo.characterCount;
        if (visibleCount <= 0) return -1;

        // Treat this as "character order index" into baseText.
        return Mathf.Clamp(charIndex, 0, Mathf.Max(0, baseText.Length - 1));
    }

    private void AddRangeAndMerge(int start, int end)
    {
        int max = baseText.Length;
        start = Mathf.Clamp(start, 0, max);
        end = Mathf.Clamp(end, 0, max);
        if (end <= start) return;

        ranges.Add(new Range { start = start, end = end });
        ranges = MergeRanges(ranges);
        selectedRangeIndex = -1;
    }

    private static List<Range> MergeRanges(List<Range> input)
    {
        var cleaned = new List<Range>();
        for (int i = 0; i < input.Count; i++)
        {
            var r = input[i];
            if (r.end <= r.start) continue;
            cleaned.Add(r);
        }

        cleaned.Sort((a, b) => a.start.CompareTo(b.start));

        var merged = new List<Range>();
        for (int i = 0; i < cleaned.Count; i++)
        {
            var r = cleaned[i];
            if (merged.Count == 0)
            {
                merged.Add(r);
                continue;
            }

            var last = merged[merged.Count - 1];

            // overlap or touch
            if (r.start <= last.end)
            {
                last.end = Mathf.Max(last.end, r.end);
                merged[merged.Count - 1] = last;
            }
            else
            {
                merged.Add(r);
            }
        }

        return merged;
    }

    private void ApplyHighlights()
    {
        // Build final TMP text by inserting <mark> tags into baseText
        string finalText = InsertMarkTags(baseText, ranges, selectedRangeIndex, markColorHex, selectedMarkColorHex);
        tmp.text = finalText;
        tmp.ForceMeshUpdate();
    }

    private static string InsertMarkTags(string text, List<Range> ranges, int selectedIndex, string normalColor, string selectedColor)
    {
        if (string.IsNullOrEmpty(text) || ranges == null || ranges.Count == 0)
            return text;

        // Work on a StringBuilder for fewer allocations
        // Insert tags from back to front so earlier indices remain valid.
        var rs = new List<Range>(ranges);
        rs.Sort((a, b) => a.start.CompareTo(b.start));

        for (int i = rs.Count - 1; i >= 0; i--)
        {
            var r = rs[i];

            int start = Mathf.Clamp(r.start, 0, text.Length);
            int end = Mathf.Clamp(r.end, 0, text.Length);
            if (end <= start) continue;

            string color = (i == selectedIndex) ? selectedColor : normalColor;

            text = text.Insert(end, "</mark>");
            text = text.Insert(start, $"<mark={color}>");
        }

        return text;
    }

    private void Save()
    {
        var data = new SaveData { ranges = ranges };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        ranges.Clear();

        string json = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrWhiteSpace(json))
            return;

        try
        {
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data != null && data.ranges != null)
                ranges = MergeRanges(data.ranges);
        }
        catch
        {
            ranges.Clear();
        }
    }

    /// <summary>
    /// Clears all highlights on this text block.
    /// </summary>
    public void ClearAllHighlights()
    {
        ranges.Clear();
        selectedRangeIndex = -1;
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
        ApplyHighlights();
    }

    // --- Utilities ---

    private static string StripMarkTags(string text)
    {
        // Very simple removal for our own tags.
        // If your baseText includes other rich tags, leave them — just avoid embedding <mark> manually.
        if (string.IsNullOrEmpty(text)) return text;
        return text.Replace("</mark>", "").Replace("<mark=", "<mark=").Replace("<mark", "<mark"); // safe no-op-ish

        // NOTE: We don't regex-strip because TMP rich tags could be complex.
        // Best practice: keep base text plain; highlights are added by this script.
    }
}