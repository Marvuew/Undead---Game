using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.GraphToolkit.Editor;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private RuntimeDialogueGraph currentGraph;

        public static DialougeManager Instance { get; private set; }
        private void Awake()
        {
            // So its visible before hitting play but invisible when playing, to avoid confusion and bugs
            gameObject.SetActive(false);
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            setUpDialougeUI();
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
        }
        //public void StartDialogue(Dialogue dialouge) 
        //{
        //    Debug.Log($"Player humanity: {Player.Instance.humanity}");
        //    optionsBox.SetActive(false);
        //    dialogueBox.SetActive(true);
        //    speakerTxt.text = $"{dialouge.speaker} :";
        //    StartCoroutine(Speak(dialougeTxt, dialouge.text, dialouge.typingDelay, dialouge));
        //    ClearOptions();
        //}
        //private IEnumerator TypeTextCoroutine(TextMeshProUGUI textBox, string text, float typeSpeed)
        //{
        //    textBox.maxVisibleCharacters = 0;
        //    textBox.text = text;
        //    for (int i = 0; i <= text.Length; i++)
        //    {
        //        textBox.maxVisibleCharacters = i;
        //        yield return new WaitForSeconds(typeSpeed);
        //    }
        //}
        //private IEnumerator Speak(TextMeshProUGUI textBox, string text, float typeSpeed, Dialogue dialogue) 
        //{
        //    yield return (TypeTextCoroutine(textBox, text, typeSpeed));
        //    if (dialogue.choices.Count > 0)
        //    {
        //        optionsBox.SetActive(true);
        //        foreach (Choice choice in dialogue.choices)
        //        {
        //            GameObject button = Instantiate(optionButtonPrefab, optionsContainer);
        //            button.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;
        //            button.GetComponent<Button>().onClick.AddListener(() => {
        //                Player.Instance.ChangeHumanity(choice.humanityChange);
        //                Player.Instance.ChangeUndead(choice.undeadChange);
        //                StartDialogue(choice.nextDialogue);
        //            });
        //        }
        //    }
        //}
        public void StartDialogue(RuntimeDialogueGraph graph)
        {
            this.currentGraph = graph;

            optionsBox.SetActive(false);
            dialogueBox.SetActive(true);

            ShowDialouge(graph.EntryNodeID);
        }
        private void ShowDialouge(string nodeID)
        {
            RuntimeDialogueNode node = currentGraph.AllNodes.FirstOrDefault(n => n.NodeID == nodeID) as RuntimeDialogueNode;
            if (node == null || node.Dialogue == null || node.Dialogue.Count == 0) return; 
            ClearOptions();

            if (node.Speaker == null) speakerTxt.text = "Unknown :";
            else speakerTxt.text = $"{node.Speaker.name} :";

            StartCoroutine(SpeakCoroutine(dialougeTxt, node.Dialogue, 0.05f, node));
        }
        private IEnumerator SpeakCoroutine(TextMeshProUGUI textBox, List<string> text, float typeSpeed, RuntimeDialogueNode node)
        {
            yield return BTypeTextCoroutine(textBox, text, typeSpeed);

            if (node.Choices != null && node.Choices.Count > 0)
            {
                optionsBox.SetActive(true);

                foreach (ChoiceData choice in node.Choices)
                {
                    ChoiceData localChoice = choice;

                    GameObject button = Instantiate(optionButtonPrefab, optionsContainer);
                    button.GetComponentInChildren<TextMeshProUGUI>().text = localChoice.ChoiceText;

                    button.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ShowDialouge(localChoice.DestinationNodeID);
                    });
                }
            }
        }

        private IEnumerator BTypeTextCoroutine(TextMeshProUGUI textBox, List<string> lines, float typeSpeed)
        {
            textBox.maxVisibleCharacters = 0;
            textBox.text = string.Join("\n", lines);

            int totalVisible = textBox.text.Length;

            WaitForSeconds delay = new WaitForSeconds(typeSpeed);

            for (int i = 0; i <= totalVisible; i++)
            {
                textBox.maxVisibleCharacters = i;
                yield return delay;
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
            StopAllCoroutines();
            Debug.Log("Exiting dialouge");
            EventSystem.current.SetSelectedGameObject(null);
            Player.Instance.interacting = false;
            dialogueBox.SetActive(false);
            optionsBox.SetActive(false);
        }

    }
}
