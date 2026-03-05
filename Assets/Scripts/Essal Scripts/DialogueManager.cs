using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

    }

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogue;

    public Animator animator;

    private Queue<string> sentences;

    private void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogue)
    {

        animator.SetBool("IsOpen", true);

        


        nameText.text = dialogue.name;

        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        if (sentence.Contains("!"))
        {
            StartCoroutine(Angry());
        }
        dialogue.text = "";
        foreach(char letter in sentence.ToCharArray())
        {
            dialogue.text += letter;
            yield return null;
        }

    }

    void EndDialogue()
    {
        animator.SetBool("IsOpen", false);
        Debug.Log("End of conversation");
    }

    IEnumerator Angry(int seconds = 2)
    {
        animator.SetBool("IsAngry", true);
        yield return new WaitForSeconds(seconds);
        animator.SetBool("IsAngry", false);
    }

}
