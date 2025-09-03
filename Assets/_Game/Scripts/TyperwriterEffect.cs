using UnityEngine;
using TMPro;
using System.Collections;
public class TyperwriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textToDisplay;
    public float waitingSeconds = Constants.DEFAULT_TYPING_SPEED;
    
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    public void StartTyping(string text)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeLine(text));
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;
        textToDisplay.text = text;
        textToDisplay.maxVisibleCharacters = 0;

        while (textToDisplay.maxVisibleCharacters < textToDisplay.text.Length)
        {
            textToDisplay.maxVisibleCharacters++;
            yield return new WaitForSeconds(waitingSeconds);
        }
        
        isTyping = false;
    }

    public void CompleteLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        textToDisplay.maxVisibleCharacters = textToDisplay.text.Length;
        isTyping = false;
    }
    
    public bool IsTyping { get { return isTyping; } }
}
