using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TickClockSystem : MonoBehaviour
{
    public static TickClockSystem Instance;

    [System.Serializable]
    public class TrackedConversation
    {
        public string conversationId;
    }

    [Header("Tracked Conversations")]
    [Tooltip("Only these conversation IDs count toward the clock.")]
    [SerializeField] private TrackedConversation[] trackedConversations;

    [Header("Clock Setup")]
    [SerializeField] private Transform clockHand;
    [SerializeField] private bool rotateClockwise = true;
    [SerializeField] private float degreesPerPhase = 90f;
    [SerializeField] private bool smoothRotation = true;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Progress Rules")]
    [Tooltip("How many finished clue-giving conversations are needed before the clock moves once.")]
    [SerializeField] private int conversationsPerPhase = 3;

    [Tooltip("How many finished clue-giving conversations end the game.")]
    [SerializeField] private int maxCompletedConversations = 9;

    [Header("Checking")]
    [Tooltip("How often the system checks whether new conversations have been completed.")]
    [SerializeField] private float checkInterval = 0.25f;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text phaseText;

    [Header("Events")]
    public UnityEvent onPhaseAdvanced;
    public UnityEvent onGameOver;

    private Quaternion baseRotation;
    private Quaternion targetRotation;

    private int completedConversationCount;
    private int currentPhase;
    private bool gameOverTriggered;
    private float checkTimer;

    public int CompletedConversationCount => completedConversationCount;
    public int CurrentPhase => currentPhase;
    public bool IsGameOver => gameOverTriggered;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (clockHand != null)
        {
            baseRotation = clockHand.localRotation;
            targetRotation = baseRotation;
        }

        RefreshFromConversationProgress(instantRotate: true);
    }

    private void Update()
    {
        checkTimer += Time.deltaTime;

        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            RefreshFromConversationProgress(instantRotate: false);
        }

        if (clockHand != null && smoothRotation)
        {
            clockHand.localRotation = Quaternion.RotateTowards(
                clockHand.localRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    public void ForceRefresh()
    {
        RefreshFromConversationProgress(instantRotate: false);
    }

    public void ResetClockVisualOnly()
    {
        completedConversationCount = 0;
        currentPhase = 0;
        gameOverTriggered = false;

        if (clockHand != null)
        {
            targetRotation = baseRotation;
            clockHand.localRotation = baseRotation;
        }

        RefreshUI();
    }

    private void RefreshFromConversationProgress(bool instantRotate)
    {
        int newCompletedCount = CountCompletedTrackedConversations();
        newCompletedCount = Mathf.Clamp(newCompletedCount, 0, maxCompletedConversations);

        int newPhase = Mathf.FloorToInt((float)newCompletedCount / conversationsPerPhase);

        bool phaseAdvanced = newPhase > currentPhase;

        completedConversationCount = newCompletedCount;
        currentPhase = newPhase;

        UpdateTargetRotation(instantRotate);
        RefreshUI();

        if (phaseAdvanced)
            onPhaseAdvanced?.Invoke();

        if (!gameOverTriggered && completedConversationCount >= maxCompletedConversations)
        {
            gameOverTriggered = true;
            onGameOver?.Invoke();
        }
    }

    private int CountCompletedTrackedConversations()
    {
        if (trackedConversations == null || trackedConversations.Length == 0)
            return 0;

        int count = 0;
        HashSet<string> seenIds = new HashSet<string>();

        for (int i = 0; i < trackedConversations.Length; i++)
        {
            if (trackedConversations[i] == null)
                continue;

            string id = trackedConversations[i].conversationId;

            if (string.IsNullOrWhiteSpace(id))
                continue;

            if (seenIds.Contains(id))
                continue;

            seenIds.Add(id);

            if (MessageChainDialogue.HasCompletedConversation(id))
                count++;
        }

        return count;
    }

    private void UpdateTargetRotation(bool instantRotate)
    {
        if (clockHand == null)
            return;

        float direction = rotateClockwise ? -1f : 1f;
        float zRotation = currentPhase * degreesPerPhase * direction;
        targetRotation = baseRotation * Quaternion.Euler(0f, 0f, zRotation);

        if (instantRotate || !smoothRotation)
            clockHand.localRotation = targetRotation;
    }

    private void RefreshUI()
    {
        if (progressText != null)
            progressText.text = $"{completedConversationCount}/{maxCompletedConversations} Clues found";

        if (phaseText != null && currentPhase == 0)
        {
            phaseText.text = $"Morning";
        }

        if (phaseText != null && currentPhase == 1)
        {
            phaseText.text = $"Mid-day";
        }
    
        if (phaseText != null && currentPhase == 2)
        {
            phaseText.text = $"Evening";
        }
           if (phaseText != null && currentPhase == 3)
        {
            phaseText.text = $"Late Night";
        }
        if (progressText != null && completedConversationCount == +1)
        {
            progressText.text = $"{completedConversationCount}/{maxCompletedConversations} Clues found";
        }
    }
}