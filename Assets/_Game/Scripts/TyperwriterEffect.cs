using UnityEngine;
using TMPro;
using System.Collections;
public class TyperwriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textToDisplay;
    private float waitingSeconds = Constants.DEFAULT_TYPING_INTERVAL;
    
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    public void StartTyping(string text,float newWaitingSeconds)
    {
        waitingSeconds = newWaitingSeconds;
        //如果有打字协程正在进行，则先将其停止再启动新的协程
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeLine(text));
    }

    private IEnumerator TypeLine(string text)
    {
        //1.改变状态量
        isTyping = true;
        //2.更新文本框变量
        textToDisplay.text = text;
        textToDisplay.maxVisibleCharacters = 0;

        //3.通过循环递增实现打字机效果
        while (textToDisplay.maxVisibleCharacters < textToDisplay.text.Length)
        {
            textToDisplay.maxVisibleCharacters++;
            yield return new WaitForSeconds(waitingSeconds);
        }
        
        //4.完成打字后改变状态量
        isTyping = false;
    }

    /// <summary>
    /// 直接完成当前行的显示（跳过打字机效果）
    /// </summary>
    public void CompleteLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        //直接让文本框的最大显示字符数等于文本长度
        textToDisplay.maxVisibleCharacters = textToDisplay.text.Length;
        isTyping = false;
    }
    
    public bool IsTyping { get { return isTyping; } }
}
