using System;
using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DG.Tweening;

public class VNManager : MonoBehaviour
{
    public TextMeshProUGUI speakerName;
    //public TextMeshProUGUI speakingContent;
    public TyperwriterEffect typerwriterEffect;

    [FormerlySerializedAs("avatorImage")] public Image avatarImage;
    public AudioSource vocalAudio;
    public Image backgroundImage;
    public AudioSource bgmAudio;
    public Image cha1Image;
    public Image cha2Image;

    public GameObject choicePanel;
    public Button choiceButton1;
    public Button choiceButton2;

    public GameObject bottomButtons;
    public Button autoPlay;
    public Button skip;
    
    private readonly string filePath = Constants.STORY_PATH;
    private readonly string defaultStoryName = Constants.DEFAULT_STORY_NAME;
    private readonly string excelExtension = Constants.STORY_EXTENSION;
    private List<ExcelReader.ExcelData> storyData;

    private int currentLine = Constants.DEFAULT_START_LINE_COUNT;
    private float fadeDuration = Constants.DEFAULT_FADE_DURATION;
    private float autoPlayWaitingTime = Constants.AUTO_PLAY_WAIT_TIME;
    
    private bool isAutoPlay = false;
    private bool isSkip = false;
    private int maxReachedLine;
    private string currentStoryName;
    private Dictionary<string, int> globalMaxReachedLines = new();
    
    private void Start()
    {
        InitializeAndLoadStory(defaultStoryName);
        BottomButtonsAddListener();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (!IsHittingBottomButtons())
            {
                DisplayNextLine();
            }
        }
    }

    void InitializeAndLoadStory(string filePath)
    {
        currentLine = Constants.DEFAULT_START_LINE_COUNT;
        
        backgroundImage.gameObject.SetActive(false);
        cha1Image.gameObject.SetActive(false);
        cha2Image.gameObject.SetActive(false);
        avatarImage.gameObject.SetActive(false);
        choicePanel.SetActive(false);
        LoadStoryFromFile(filePath);
        //DisplayNextLine();
        //Debug.Log("Initializing story...");
        
        //autoPlay.onClick.AddListener(OnClickAutoPlay);
    }

    void BottomButtonsAddListener()
    {
        autoPlay.onClick.AddListener(OnClickAutoPlay);
        skip.onClick.AddListener(OnClickSkip);
    }
    
    private void LoadStoryFromFile(string storyPath)
    {
        currentStoryName = storyPath;
        var story = filePath + storyPath + excelExtension; 
        storyData = ExcelReader.ReadExcel(story);
        if (storyData.Count == 0 || storyData == null)
        {
            Debug.LogError("No story data");
        }

        if (globalMaxReachedLines.ContainsKey(currentStoryName))
        {
            maxReachedLine = globalMaxReachedLines[currentStoryName];
        }
        else
        {
            maxReachedLine = 0;
            globalMaxReachedLines.Add(currentStoryName, maxReachedLine);
        }
    }

    private void DisplayNextLine()
    {
        if (currentLine > maxReachedLine)
        {
            maxReachedLine = currentLine;
            globalMaxReachedLines[currentStoryName] = maxReachedLine;
        }
        if (currentLine >= storyData.Count-1)
        {
            if (isAutoPlay)
            {
                isAutoPlay = false;
                UpdateButtonImage(Constants.AUTO_OFF,autoPlay);
            }
            if (storyData[currentLine].speaker == Constants.END_OF_STORY)
            {
                Debug.Log($"{Constants.END_OF_STORY}");
                return;
            }

            if (storyData[currentLine].speaker == Constants.CHOICE)
            {
                //currentLine = Constants.DEFAULT_START_LINE_COUNT;
                ShowChoices();
                return;
            }

            return;
        }

        if (typerwriterEffect.IsTyping)
        {
            typerwriterEffect.CompleteLine();
        }
        else
        {
            DisplayThisLine();
        }
        
    }

    void ShowChoices()
    {
        var data = storyData[currentLine];
        
        choiceButton1.onClick.RemoveAllListeners();
        choiceButton2.onClick.RemoveAllListeners();
        
        choicePanel.SetActive(true);
        
        choiceButton1.GetComponentInChildren<TextMeshProUGUI>().text = data.content;
        choiceButton1.onClick.AddListener(()=>InitializeAndLoadStory(data.avatorImageFileName));
        choiceButton2.GetComponentInChildren<TextMeshProUGUI>().text = data.vocalAudioFileName;
        choiceButton2.onClick.AddListener(()=>InitializeAndLoadStory(data.backgroundImageFileName));
        
        
    }
    
    void DisplayThisLine()
    {
        var data = storyData[currentLine];
        speakerName.text = data.speaker;
        //显示说话内容
        typerwriterEffect.StartTyping(data.content);
        
        //显示说话人头像
        if (NotNullNorEmpty(data.avatorImageFileName))
        {
            UpdateAvatarImage(data.avatorImageFileName);
        }
        else
        {
            avatarImage.gameObject.SetActive(false);
        }

        //播放语音
        if (NotNullNorEmpty(data.vocalAudioFileName))
        {
            PlayVocalAudio(data.vocalAudioFileName);
        }
        
        //显示背景
        if (NotNullNorEmpty(data.backgroundImageFileName))
        {
            UpdateBackgroundImage(data.backgroundImageFileName);
        }
        
        //显示背景音乐
        if (NotNullNorEmpty(data.bgmFileName))
        {
            PlayBackgroundMusic(data.bgmFileName);
        }
        
        //更新立绘角色行动
        if (NotNullNorEmpty(data.cha1Action))
        {
            UpdateCharacterImage(data.cha1Action,data.cha1Image,cha1Image,data.cha1CoorX);
        }

        if (NotNullNorEmpty(data.cha2Action))
        {
            UpdateCharacterImage(data.cha2Action,data.cha2Image,cha2Image,data.cha2CoorX);
        }
        
        currentLine++;
    }

    void UpdateImage(string imageFilePath, Image image)
    {
        Sprite sprite = Resources.Load<Sprite>(imageFilePath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.gameObject.SetActive(true);
            
        }
        else
        {
            Debug.Log($"Unable to load image in: {imageFilePath}");
        }
    }

    void PlayAudio(string audioFilePath, AudioSource audioSource, bool isLoop = false)
    {
        AudioClip audioClip = Resources.Load<AudioClip>(audioFilePath);
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.loop = isLoop;
            audioSource.Play();
        }
        else
        {
            Debug.Log($"Unable to load audio in: {audioFilePath}");
        }
    }

    void UpdateCharacterImage(string imageAction, string imageFilePath, Image image, string coorX = "")
    {
        if (imageAction.StartsWith(Constants.CHARACTER_IMAGE_APPEAR))
        {
            String imagePath = Constants.CHARACTER_IMAGE_PATH + imageFilePath;
            if (NotNullNorEmpty(coorX))
            {
                UpdateImage(imagePath, image);
                Vector2 newPosition = new Vector2(float.Parse(coorX),image.rectTransform.anchoredPosition.y);
                image.rectTransform.anchoredPosition = newPosition;
                image.DOFade(1f,fadeDuration).From(0);
            }
            else
            {
                Debug.LogError($"No X coordinate for: {imageFilePath} to appear");
            }
            //image.gameObject.SetActive(true);
        }else if (imageAction.StartsWith(Constants.CHARACTER_IMAGE_MOVETO))
        {
            //实现角色立绘的移动
            if (NotNullNorEmpty(coorX))
            {
                image.rectTransform.DOAnchorPosX(float.Parse(coorX),fadeDuration);
            }
            else
            {
                Debug.LogError($"No X coordinate for: {imageFilePath} to move");
            }
        }else if (imageAction.StartsWith(Constants.CHARACTER_IMAGE_DISAPPEAR))
        {
            //image.gameObject.SetActive(false);
            image.DOFade(0,fadeDuration).OnComplete(()=>image.gameObject.SetActive(false));
        }
    }

    private void UpdateAvatarImage(string dataAvatorImageFileName)
    {
        string imagePath = Constants.AVATAR_PATH + dataAvatorImageFileName;
        UpdateImage(imagePath, avatarImage);
    }

    private void UpdateBackgroundImage(string dataBackgroundImageFileName)
    {
        string backgroundImageFileName = Constants.BACKGROUND_IMAGE_PATH + dataBackgroundImageFileName;
        UpdateImage(backgroundImageFileName, backgroundImage);
        backgroundImage.gameObject.SetActive(true);
        Debug.Log("Updated Background Image");
    }

    void UpdateButtonImage(string buttonImageName, Button button)
    {
        string imagePath = Constants.BUTTON_IMAGE_PATH + buttonImageName;
        UpdateImage(imagePath, button.image);
    }
    
    private void PlayVocalAudio(string dataVocalAudioFileName)
    {
        string vocalAudioPath = Constants.VOCAL_AUDIO_PATH + dataVocalAudioFileName;
        PlayAudio(vocalAudioPath, vocalAudio);
    }

    private void PlayBackgroundMusic(string dataBGMFileName)
    {
        string bgmPath = Constants.BGM_PATH + dataBGMFileName;
        PlayAudio(bgmPath, bgmAudio, true);
    }

    bool IsHittingBottomButtons()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(bottomButtons.GetComponent<RectTransform>(),Input.mousePosition,null);
    }
    
    Coroutine autoPlayCoroutine;
    
    void OnClickAutoPlay()
    {
        isAutoPlay = !isAutoPlay;
        UpdateButtonImage((isAutoPlay ? Constants.AUTO_ON : Constants.AUTO_OFF), autoPlay);
        if (isAutoPlay)
        {
            autoPlayCoroutine = StartCoroutine(StartAutoPlay());
        }
        else
        {
            StopCoroutine(autoPlayCoroutine);
        }
    }

    void OnClickSkip()
    {
        if (!isSkip && CanSkip())
        {
            StartSkip();
        }else if (isSkip)
        {
            StopCoroutine(SkipToMaxReachedLine());
            EndSkip();
        }
    }

    bool CanSkip()
    {
        return currentLine  < maxReachedLine;
    }

    void StartSkip()
    {
        isSkip = true;
        UpdateButtonImage(Constants.SKIP_ON, skip);
        typerwriterEffect.waitingSeconds = Constants.SKIP_MODE_TYPING_SPEED;
        StartCoroutine(SkipToMaxReachedLine());
    }

    void EndSkip()
    {
        isSkip = false;
        typerwriterEffect.waitingSeconds = Constants.DEFAULT_TYPING_SPEED;
        UpdateButtonImage(Constants.SKIP_OFF, skip);
    }

    public IEnumerator StartAutoPlay()
    {
        while (isAutoPlay)
        {
            if (!typerwriterEffect.IsTyping)
            {
                yield return new WaitForSeconds(1f);
                DisplayNextLine();
            }

            yield return new WaitForSeconds(autoPlayWaitingTime);
        }
    }
    
    IEnumerator SkipToMaxReachedLine()
    {
        while (isSkip)
        {
            if (CanSkip())
            {
                DisplayThisLine();
            }
            else
            {
                EndSkip();
            }
            yield return new WaitForSeconds(Constants.DEFAULT_SKIP_WAITING_SECONDS);
        }
    }
    bool NotNullNorEmpty(string str)
    {
        return !string.IsNullOrEmpty(str);
    }
    
}
