using System;
using System.Collections;
using LLMUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestGame : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public GameObject npc;

    PlayerMovement playerMovement;
    PlayerCamera playerCamera;
    LLMCharacter npcLLMCharacter;

    [Header("Conversation State")]
    public bool isTalking = false;
    public bool hasInput = false;
    public bool hasResponse = false;
    public bool hasFinishedResponse = false;

    [Header("UI Elements")]
    public TMP_InputField playerInputField;
    public GameObject inputFieldGameObject;

    public TMP_Text responseUI;
    public GameObject responseGameObject;
    

    [Header("Typing Effect Settings")]
    public float typingInterval = 0.05f;
    private Coroutine typingCoroutine;
    string inputText = "";
    string responseText = "";

    void Start()
    {
        inputFieldGameObject.SetActive(false);
        responseGameObject.SetActive(false);

        playerMovement = player.GetComponent<PlayerMovement>();
        playerCamera = player.GetComponent<PlayerCamera>();
        npcLLMCharacter = npc.GetComponent<LLMCharacter>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Start conversation
            if (!isTalking)
            {
                isTalking = true;

                // Show input field and focus
                inputFieldGameObject.SetActive(true);
                playerInputField.ActivateInputField();
            }

            else if (isTalking && playerInputField.text == "" && !hasInput && !hasResponse && !hasFinishedResponse)
            {
                isTalking = false;
                inputFieldGameObject.SetActive(false);
                responseGameObject.SetActive(false);
            }

            // Get input and send to LLM
            else if (isTalking && !hasInput && !hasResponse)
            {
                // Get input and hide input field
                inputText = playerInputField.text;
                playerInputField.text = "";
                inputFieldGameObject.SetActive(false);
                responseGameObject.SetActive(true);
                hasInput = true;

                hasResponse = false;
                hasFinishedResponse = false;
                _ = npcLLMCharacter.Chat(inputText, SaveResponse, ShowResponse);
                responseUI.text = "[Thinking...]";
            }

            // Continue conversation
            else if (isTalking && hasResponse && hasFinishedResponse)
            {
                inputFieldGameObject.SetActive(true);
                playerInputField.ActivateInputField();
                responseGameObject.SetActive(false);

                hasInput = false;
                hasResponse = false;
                hasFinishedResponse = false;
                playerInputField.text = "";
                responseUI.text = "";
                responseText = "";
            }
        }

        playerMovement.enabled = !isTalking;
        playerCamera.enabled = !isTalking;
    }

    void SaveResponse(string response)
    {
        responseText = response;
    }

    void ShowResponse()
    {
        hasResponse = true;
        // Cancel any existing typing and start typing the new response
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        typingCoroutine = StartCoroutine(TypeResponse(responseText));
    }

    IEnumerator TypeResponse(string response)
    {
        responseUI.text = string.Empty;

        for (int i = 0; i < response.Length; i++)
        {
            responseUI.text += response[i];
            yield return new WaitForSeconds(typingInterval);
        }

        typingCoroutine = null;
        hasFinishedResponse = true;
    }
}
