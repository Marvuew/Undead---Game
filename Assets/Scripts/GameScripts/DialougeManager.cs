using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Rendering.MaterialUpgrader;

namespace Assets.Scripts.GameScripts
{
    internal class DialougeManager : MonoBehaviour
    {
        GameObject dialogueBox;
        TextMeshProUGUI speakerTxt;
        TextMeshProUGUI dialougeTxt;
        GameObject optionsBox;
        [SerializeField] private GameObject optionButtonPrefab;
        private Transform optionsContainer;
        private Button exitDialogueBtn;
        [SerializeField] Button startConvo;
        [SerializeField] Dialogue StartNode;
        public static DialougeManager instance { get; private set; }
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            setUpDialougeUI();
        }
        public void TestConvo() 
        {
            Debug.Log("Starting...");
            StartDialogue(StartNode);
        }
        private void setUpDialougeUI() 
        {
            dialogueBox = GameObject.Find("DialogueBox");
            speakerTxt = GameObject.Find("SpeakerTxt").GetComponent<TextMeshProUGUI>();
            dialougeTxt = GameObject.Find("DialougeTxt").GetComponent<TextMeshProUGUI>();
            optionsBox = GameObject.Find("OptionsBox");
            optionsContainer = GameObject.Find("OptionsContainer").GetComponent<Transform>();
            exitDialogueBtn = GameObject.Find("ExitDialogueBtn").GetComponent<Button>();
            dialogueBox.SetActive(false);
            optionsBox.SetActive(false);
            exitDialogueBtn.onClick.AddListener(ExitDialogue);
            exitDialogueBtn.interactable = false;
        }
        public void StartDialogue(Dialogue dialouge) 
        {
            Debug.Log($"Player humanity: {Player.instance.humanity}");
            optionsBox.SetActive(false);
            dialogueBox.SetActive(true);
            speakerTxt.text = $"{dialouge.speaker} :";
            StartCoroutine(TypeTextCoroutine(dialougeTxt, dialouge.text, dialouge.typingDelay));

            ClearOptions();

            if (dialouge.choices.Count > 0) 
            {
                optionsBox.SetActive(true);
                foreach (Choice choice in dialouge.choices) 
                { 
                    GameObject button = Instantiate(optionButtonPrefab, optionsContainer);
                    button.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;
                    button.GetComponent<Button>().onClick.AddListener(() => {
                        Player.instance.ChangeHumanity(choice.humanityChange);
                        StartDialogue(choice.nextDialogue);
                    });
                }
            }
            exitDialogueBtn.interactable = true;
        }

        private IEnumerator TypeTextCoroutine(TextMeshProUGUI textBox, string text, float typeSpeed) 
        {
            textBox.maxVisibleCharacters = 0;
            textBox.text = text;
            for (int i = 0; i <= text.Length; i++) 
            {
                textBox.maxVisibleCharacters = i;
                yield return new WaitForSeconds(typeSpeed);
            }
        }
        private void ClearOptions()
        {
            foreach (Transform child in optionsContainer)
            {
                Destroy(child.gameObject);
            }
        }
        private void ExitDialogue() 
        { 
            dialogueBox.SetActive(false);
            optionsBox.SetActive(false);
            exitDialogueBtn.interactable = false;
        }

    }
}
