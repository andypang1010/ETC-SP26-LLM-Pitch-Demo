using System;
using LLMUnity;
using TMPro;
using UnityEngine;

public class TestGame : MonoBehaviour
{
    public string input = "Hello, how are you?";
    public TMP_Text responseUI;
    public LLMCharacter llmCharacter;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            _ = llmCharacter.Chat(input, HandleResponse);
        }
    }

    void HandleResponse(string response)
    {
        responseUI.text = response;
    }
}
