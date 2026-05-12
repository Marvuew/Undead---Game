using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.GameScripts
{
    public class CompassTargetSystem : MonoBehaviour
    {
        public static CompassTargetSystem Instance { get; private set; }

        [System.Serializable]
        public class CompassTarget
        {
            public string targetName;
            public string worldObjectName;
            public RectTransform uiDot;

            [Header("Interaction")]
            public bool hideAfterInteraction = true;

            [HideInInspector] public bool hasBeenInteractedWith = false;
            [HideInInspector] public Transform worldTarget;
        }

        private class RuntimeInteractableDot
        {
            public InteractableScriptableObject interactable;
            public RectTransform uiDot;
        }

        [Header("References")]
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private RectTransform maskArea;
        [SerializeField] private TMP_Dropdown locationDropdown;

        [Header("Compass Root")]
        [SerializeField] private CanvasGroup compassCanvasGroup;

        [Header("Hide Compass In These Scenes")]
        [SerializeField] private List<string> hiddenScenes = new();

        [Header("Normal Targets")]
        [SerializeField] private List<CompassTarget> targets = new();

        [Header("Interactable Compass Targets")]
        [SerializeField] private InteractableScriptableObject[] interactableTargets;
        [SerializeField] private RectTransform interactableDotParent;
        [SerializeField] private GameObject interactableDotPrefab;

        [Header("Settings")]
        [SerializeField] private float edgePadding = 12f;

        private int selectedTargetIndex = 0;
        private bool hasAvailableTargets = false;

        private readonly List<RuntimeInteractableDot> runtimeInteractableDots = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SetupDropdown();
            CreateInteractableDots();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            RefreshCompassTargets();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(RefreshAfterSceneLoad());
        }

        private IEnumerator RefreshAfterSceneLoad()
        {
            yield return null;
            yield return null;

            ReconnectSceneReferences();
            RefreshCompassTargets();
        }

        private void LateUpdate()
        {
            if (!hasAvailableTargets)
                return;

            if (selectedTargetIndex >= 0 && selectedTargetIndex < targets.Count)
            {
                CompassTarget target = targets[selectedTargetIndex];

                if (minimapCamera != null && maskArea != null && target.worldTarget != null && target.uiDot != null)
                    UpdateTarget(target);
            }

            UpdateInteractableDots();
        }

        private void SetupDropdown()
        {
            if (locationDropdown == null)
                return;

            locationDropdown.onValueChanged.RemoveListener(SelectTarget);
            locationDropdown.ClearOptions();

            List<string> options = new List<string>();

            foreach (CompassTarget target in targets)
                options.Add(target.targetName);

            locationDropdown.AddOptions(options);
            locationDropdown.SetValueWithoutNotify(0);
            locationDropdown.RefreshShownValue();

            locationDropdown.onValueChanged.AddListener(SelectTarget);
        }

        private void CreateInteractableDots()
        {
            runtimeInteractableDots.Clear();

            if (interactableTargets == null || interactableDotParent == null || interactableDotPrefab == null)
                return;

            foreach (InteractableScriptableObject interactable in interactableTargets)
            {
                if (interactable == null)
                    continue;

                if (!interactable.showOnCompass)
                    continue;

                GameObject dotObject = Instantiate(interactableDotPrefab, interactableDotParent);
                dotObject.name = interactable.compassDisplayName + " Compass Dot";

                RectTransform dotRect = dotObject.GetComponent<RectTransform>();
                Image image = dotObject.GetComponent<Image>();

                if (image != null && interactable.minimapSprite != null)
                    image.sprite = interactable.minimapSprite;

                dotObject.SetActive(false);

                runtimeInteractableDots.Add(new RuntimeInteractableDot
                {
                    interactable = interactable,
                    uiDot = dotRect
                });
            }
        }

        private void RefreshCompassTargets()
        {
            if (ShouldHideCompassForCurrentScene())
            {
                HideCompass();
                return;
            }

            ShowCompass();
            ReconnectSceneReferences();

            hasAvailableTargets = HasAnyAvailableTarget() || runtimeInteractableDots.Count > 0;

            if (!hasAvailableTargets)
            {
                HideAllDots();

                if (locationDropdown != null)
                    locationDropdown.interactable = false;

                return;
            }

            if (locationDropdown != null)
                locationDropdown.interactable = targets.Count > 0;

            if (targets.Count > 0)
            {
                if (selectedTargetIndex < 0 || selectedTargetIndex >= targets.Count)
                    selectedTargetIndex = 0;

                if (targets[selectedTargetIndex].worldTarget == null || IsTargetHidden(targets[selectedTargetIndex]))
                    selectedTargetIndex = GetFirstAvailableTargetIndex();

                SelectTarget(selectedTargetIndex);
            }
        }

        private bool ShouldHideCompassForCurrentScene()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            return hiddenScenes.Contains(currentScene);
        }

        private void HideCompass()
        {
            HideAllDots();
            HideInteractableDots();

            hasAvailableTargets = false;

            if (compassCanvasGroup == null)
                return;

            compassCanvasGroup.alpha = 0f;
            compassCanvasGroup.interactable = false;
            compassCanvasGroup.blocksRaycasts = false;
        }

        private void ShowCompass()
        {
            if (compassCanvasGroup == null)
                return;

            compassCanvasGroup.alpha = 1f;
            compassCanvasGroup.interactable = true;
            compassCanvasGroup.blocksRaycasts = true;
        }

        private bool HasAnyAvailableTarget()
        {
            foreach (CompassTarget target in targets)
            {
                if (target.worldTarget != null && !IsTargetHidden(target))
                    return true;
            }

            return false;
        }

        private int GetFirstAvailableTargetIndex()
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].worldTarget != null && !IsTargetHidden(targets[i]))
                    return i;
            }

            return -1;
        }

        private bool IsTargetHidden(CompassTarget target)
        {
            return target.hideAfterInteraction && target.hasBeenInteractedWith;
        }

        private void SelectTarget(int index)
        {
            HideAllDots();

            selectedTargetIndex = index;

            if (selectedTargetIndex < 0 || selectedTargetIndex >= targets.Count)
                return;

            CompassTarget target = targets[selectedTargetIndex];

            if (target.worldTarget == null)
                return;

            if (IsTargetHidden(target))
                return;

            if (target.uiDot != null)
                target.uiDot.gameObject.SetActive(true);
        }

        private void HideAllDots()
        {
            foreach (CompassTarget target in targets)
            {
                if (target.uiDot != null)
                    target.uiDot.gameObject.SetActive(false);
            }
        }

        private void HideInteractableDots()
        {
            foreach (RuntimeInteractableDot runtimeDot in runtimeInteractableDots)
            {
                if (runtimeDot.uiDot != null)
                    runtimeDot.uiDot.gameObject.SetActive(false);
            }
        }

        private void ReconnectSceneReferences()
        {
            if (minimapCamera == null)
            {
                GameObject cameraObject = GameObject.FindGameObjectWithTag("MinimapCamera");

                if (cameraObject != null)
                    minimapCamera = cameraObject.GetComponent<Camera>();
            }

            foreach (CompassTarget target in targets)
            {
                GameObject foundObject = GameObject.Find(target.worldObjectName);

                if (foundObject != null)
                    target.worldTarget = foundObject.transform;
                else
                    target.worldTarget = null;
            }
        }

        private void UpdateTarget(CompassTarget target)
        {
            if (IsTargetHidden(target))
            {
                if (target.uiDot != null)
                    target.uiDot.gameObject.SetActive(false);

                return;
            }

            UpdateDotPosition(target.worldTarget.position, target.uiDot);
        }

        private void UpdateInteractableDots()
{
    if (minimapCamera == null || maskArea == null)
        return;

    string currentScene = SceneManager.GetActiveScene().name;

    foreach (RuntimeInteractableDot runtimeDot in runtimeInteractableDots)
    {
        if (runtimeDot.interactable == null || runtimeDot.uiDot == null)
            continue;

        Vector3 viewport = minimapCamera.WorldToViewportPoint(runtimeDot.interactable.position);

        bool isInCameraView =
            viewport.z > 0f &&
            viewport.x >= 0f &&
            viewport.x <= 1f &&
            viewport.y >= 0f &&
            viewport.y <= 1f;

        bool shouldShow =
            runtimeDot.interactable.showOnCompass &&
            runtimeDot.interactable.homeScene.ToString() == currentScene &&
            isInCameraView;

        runtimeDot.uiDot.gameObject.SetActive(shouldShow);

        if (!shouldShow)
            continue;

        UpdateDotPosition(runtimeDot.interactable.position, runtimeDot.uiDot);
    }
}
        private void UpdateDotPosition(Vector3 worldPosition, RectTransform dot)
        {
            Vector3 viewport = minimapCamera.WorldToViewportPoint(worldPosition);

            float x = (viewport.x - 0.5f) * maskArea.rect.width;
            float y = (viewport.y - 0.5f) * maskArea.rect.height;

            Vector2 position = new Vector2(x, y);

            float dotRadius = Mathf.Max(dot.rect.width, dot.rect.height) * 0.5f;

            float compassRadius =
                Mathf.Min(maskArea.rect.width, maskArea.rect.height) * 0.5f
                - edgePadding
                - dotRadius;

            bool outsideCamera =
                viewport.x < 0f ||
                viewport.x > 1f ||
                viewport.y < 0f ||
                viewport.y > 1f ||
                viewport.z < 0f;

            bool outsideCompass = position.magnitude > compassRadius;

            if (outsideCamera || outsideCompass)
                position = position.normalized * compassRadius;

            dot.anchoredPosition = position;
        }

        public void MarkTargetAsInteracted(string targetName)
        {
            foreach (CompassTarget target in targets)
            {
                if (target.targetName != targetName)
                    continue;

                target.hasBeenInteractedWith = true;

                if (target.uiDot != null)
                    target.uiDot.gameObject.SetActive(false);

                break;
            }

            RefreshCompassTargets();
        }

        public void MarkTargetAsInteractedByWorldObjectName(string worldObjectName)
        {
            foreach (CompassTarget target in targets)
            {
                if (target.worldObjectName != worldObjectName)
                    continue;

                MarkTargetAsInteracted(target.targetName);
                break;
            }
        }
    }
}