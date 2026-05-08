using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        [Header("References")]
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private RectTransform maskArea;
        [SerializeField] private TMP_Dropdown locationDropdown;

        [Header("Compass Root")]
        [SerializeField] private CanvasGroup compassCanvasGroup;

        [Header("Hide Compass In These Scenes")]
        [SerializeField] private List<string> hiddenScenes = new();

        [Header("Targets")]
        [SerializeField] private List<CompassTarget> targets = new();

        [Header("Settings")]
        [SerializeField] private float edgePadding = 12f;

        private int selectedTargetIndex = 0;
        private bool hasAvailableTargets = false;

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

            if (selectedTargetIndex < 0 || selectedTargetIndex >= targets.Count)
                return;

            CompassTarget target = targets[selectedTargetIndex];

            if (minimapCamera == null || maskArea == null || target.worldTarget == null || target.uiDot == null)
                return;

            UpdateTarget(target);
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

        private void RefreshCompassTargets()
        {
            if (ShouldHideCompassForCurrentScene())
            {
                HideCompass();
                return;
            }

            ShowCompass();
            ReconnectSceneReferences();

            hasAvailableTargets = HasAnyAvailableTarget();

            if (!hasAvailableTargets)
            {
                HideAllDots();

                if (locationDropdown != null)
                    locationDropdown.interactable = false;

                return;
            }

            if (locationDropdown != null)
                locationDropdown.interactable = true;

            if (selectedTargetIndex < 0 || selectedTargetIndex >= targets.Count)
                selectedTargetIndex = 0;

            if (targets[selectedTargetIndex].worldTarget == null || IsTargetHidden(targets[selectedTargetIndex]))
                selectedTargetIndex = GetFirstAvailableTargetIndex();

            SelectTarget(selectedTargetIndex);
        }

        private bool ShouldHideCompassForCurrentScene()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            return hiddenScenes.Contains(currentScene);
        }

        private void HideCompass()
        {
            HideAllDots();
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

            Vector3 viewport = minimapCamera.WorldToViewportPoint(target.worldTarget.position);

            float x = (viewport.x - 0.5f) * maskArea.rect.width;
            float y = (viewport.y - 0.5f) * maskArea.rect.height;

            Vector2 position = new Vector2(x, y);

            float dotRadius =
                Mathf.Max(target.uiDot.rect.width, target.uiDot.rect.height) * 0.5f;

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

            target.uiDot.anchoredPosition = position;
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