using UnityEngine;
using UnityEngine.UI;

public class UISpriteFrameAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image targetImage;
    [SerializeField] private GameObject buttonsPanel;

    [Header("Animation Frames")]
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 12f;

    [Header("Input")]
    [SerializeField] public KeyCode toggleKey = KeyCode.Escape;

    [Header("Optional")]
    [SerializeField] private bool pauseGameWhenOpen = true;

    private int currentFrame = 0;
    private float timer = 0f;

    private bool isOpen = false;
    private bool isAnimating = false;
    private bool isReversing = false;

    private void Reset()
    {
        targetImage = GetComponent<Image>();
    }

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        HideInstant();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }

        if (!isAnimating || targetImage == null || frames == null || frames.Length == 0)
            return;

        float frameDuration = 1f / framesPerSecond;
        timer += Time.unscaledDeltaTime;

        while (timer >= frameDuration)
        {
            timer -= frameDuration;

            if (!isReversing)
            {
                currentFrame++;

                if (currentFrame >= frames.Length)
                {
                    currentFrame = frames.Length - 1;
                    targetImage.sprite = frames[currentFrame];
                    isAnimating = false;
                    isOpen = true;

                    if (buttonsPanel != null)
                        buttonsPanel.SetActive(true);

                    return;
                }
            }
            else
            {
                currentFrame--;

                if (currentFrame < 0)
                {
                    currentFrame = 0;
                    isAnimating = false;
                    isOpen = false;

                    if (buttonsPanel != null)
                        buttonsPanel.SetActive(false);

                    targetImage.sprite = frames[0];
                    targetImage.enabled = false;

                    if (pauseGameWhenOpen)
                        Time.timeScale = 1f;

                    return;
                }
            }

            targetImage.sprite = frames[currentFrame];
        }
    }

    public void Toggle()
    {
        if (frames == null || frames.Length == 0 || targetImage == null)
            return;

        timer = 0f;

        if (!isOpen && !isAnimating)
        {
            Open();
        }
        else if (isOpen && !isAnimating)
        {
            Close();
        }
        else if (isAnimating)
        {
            if (!isReversing)
            {
                Close();
            }
            else
            {
                Open();
            }
        }
    }

    public void Open()
    {
        if (frames == null || frames.Length == 0 || targetImage == null)
            return;

        if (buttonsPanel != null)
            buttonsPanel.SetActive(false);

        if (pauseGameWhenOpen)
            Time.timeScale = 0f;

        targetImage.enabled = true;
        isAnimating = true;
        isReversing = false;
        isOpen = false;

        currentFrame = Mathf.Clamp(currentFrame, 0, frames.Length - 1);
        targetImage.sprite = frames[currentFrame];
        timer = 0f;
    }

    public void Close()
    {
        if (frames == null || frames.Length == 0 || targetImage == null)
            return;

        if (buttonsPanel != null)
            buttonsPanel.SetActive(false);

        isAnimating = true;
        isReversing = true;
        timer = 0f;
    }

    public void HideInstant()
    {
        currentFrame = 0;
        timer = 0f;
        isOpen = false;
        isAnimating = false;
        isReversing = false;

        if (buttonsPanel != null)
            buttonsPanel.SetActive(false);

        if (targetImage != null)
        {
            if (frames != null && frames.Length > 0)
                targetImage.sprite = frames[0];

            targetImage.enabled = false;
        }

        if (pauseGameWhenOpen)
            Time.timeScale = 1f;
    }

    public void ResumeGame()
    {
        if (isOpen || isAnimating)
            Close();
    }

    public void QuitGame()
    {
        if (pauseGameWhenOpen)
            Time.timeScale = 1f;

        Application.Quit();
    }
}