using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems; 

public class DialogueManager : MonoBehaviour
{
    //all this is to create a singleton class
     
    private static DialogueManager instance;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    public GameObject choiceObject; 

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;
    public Animator anim; 
    private Story currentStory;
    //Other scripts can only get the info of public bool, but can't change it. 
    public bool dialogueIsPlaying { get; private set; } 

    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogWarning("Found more than one DialogueManager"); 
        }

        instance = this; 
    }

    public static DialogueManager GetInstance()
    {
        return instance; 
    }

    private void Start()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);

        //get all of the choices text: initialize choices text to be an array
        //of same length of our choices. Then for each choice in the choice array
        //we'll initialize the corresponding text using an index for that choice
        //so that they match. 
        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0; 
        foreach( GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++; 
        }
    }

    private void Update()
    {
        if (!dialogueIsPlaying)
        {
            return; 
        }

        if (InputManager.GetInstance().GetSubmitPressed())
        {
            ContinueStory(); 
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);
        choiceObject.SetActive(true);

        ContinueStory(); 
    }

    private IEnumerator ExitDialogueMode()
    {
        //set coroutine to avoid player jump right after the dialogue ends. 
        yield return new WaitForSeconds(0.2f); 

        dialogueIsPlaying = false; 
        dialoguePanel.SetActive(false);
        dialogueText.text = ""; 
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            dialogueText.text = currentStory.Continue();
            DisplayChoices(); 
        }
        else
        {
           StartCoroutine(ExitDialogueMode());
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices; 
        //defensive check to make sure our UI can support the number of choices coming in
        if(currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices were given than the UI can support. NUmber of choices given "
                + currentChoices.Count); 
        }

        int index = 0; 
        //enable and initialize the choices up to the amount of choices for this line of dialogue
        foreach(Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            index++; 
        }

        //go through the remaining choices the UI supports and make sure they are hiding
        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);             
        }

        StartCoroutine(SelectFirstChoice()); 
    }

    private IEnumerator SelectFirstChoice()
    {
        //event system requires we clear it first, then wait
        //for at least one frame before we set the current selected object. 
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
        
    } 

    public void MakeChoice(int choiceIndex)
    {
        currentStory.ChooseChoiceIndex(choiceIndex);
        if (choiceIndex == 1)
        {
            anim.SetTrigger("Happy");
        }
        if (choiceIndex == 2)
        {
            anim.SetTrigger("Sad");
        }
        choiceObject.SetActive(false);
    }
}
