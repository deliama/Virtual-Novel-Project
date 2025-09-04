using System;
using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json;


public class VNManager : Singleton<VNManager>
{
    #region Variables
    
    public GameObject gamePanel;
    public GameObject dialoguePanel;
    
    public TextMeshProUGUI speakerName;
    //public TextMeshProUGUI speakingContent;
    public TyperwriterEffect typerwriterEffect;
    public ScreenShotter screenShotter;
    
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
    [FormerlySerializedAs("autoPlay")] public Button autoPlayButton;
    [FormerlySerializedAs("skip")] public Button skipButton;
    public Button saveButton;
    public Button loadButton;
    public Button historyButton;
    public Button settingsButton;
    public Button homeButton;
    public Button closeButton;
    
    private readonly string filePath = Constants.STORY_PATH;
    private readonly string defaultStoryName = Constants.DEFAULT_STORY_NAME;
    private readonly string excelExtension = Constants.STORY_EXTENSION;
    private readonly int defaultStartLine = Constants.DEFAULT_START_LINE_COUNT;
    private List<ExcelReader.ExcelData> storyData;

    //存档相关数据
    private string saveFolderPath;
    private byte[] screenshotData;  //保存截图数据
    private string currentSpeakingContent;  //保存截图时对话
    
    private int currentLine = Constants.DEFAULT_START_LINE_COUNT;
    private float fadeDuration = Constants.DEFAULT_FADE_DURATION;
    private float autoPlayWaitingTime = Constants.AUTO_PLAY_WAIT_TIME;
    private float currentTypeInterval = Constants.DEFAULT_TYPING_INTERVAL;
    
    private bool isAutoPlay = false;
    private bool isSkip = false;
    private bool isLoad = false;
    private int maxReachedLine;
    private string currentStoryName;
    private Dictionary<string, int> globalMaxReachedLines = new();
    
    #endregion

    #region LifeCycle
    private void Start()
    {
        InitializeSaveFilePath();
        BottomButtonsAddListener();
        //gamePanel.SetActive(false);
    }

    private void Update()
    {
        if (!MenuManager.Instance.m_menuPanel.activeSelf
            && !SaveLoadManager.Instance.m_saveLoadPanel.activeSelf
            && gamePanel.activeSelf && Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (!dialoguePanel.activeSelf)
            {
                OpenUI();
            }
            else if(!IsHittingBottomButtons())
            {
                DisplayNextLine();
            }
        }
    }

    #endregion
    
    #region Initialization

    void InitializeSaveFilePath()
    {
        saveFolderPath = Path.Combine(Application.persistentDataPath, Constants.SAVE_FILE_PATH);
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
        }
    }
    void InitializeAndLoadStory(string filePath,int lineNumber)
    {
        currentLine = lineNumber;
        
        backgroundImage.gameObject.SetActive(false);
        bgmAudio.gameObject.SetActive(false);
        
        cha1Image.gameObject.SetActive(false);
        cha2Image.gameObject.SetActive(false);
        
        vocalAudio.gameObject.SetActive(false);
        avatarImage.gameObject.SetActive(false);
        choicePanel.SetActive(false);
        LoadStoryFromFile(filePath);

        if (isLoad)
        {
            RecoverLastBGAndCharacter();
            isLoad = false;
        }
        
        DisplayNextLine();
    }

    void BottomButtonsAddListener()
    {
        autoPlayButton.onClick.AddListener(OnClickAutoPlay);
        skipButton.onClick.AddListener(OnClickSkip);
        saveButton.onClick.AddListener(OnClickSave);
        loadButton.onClick.AddListener(OnClickLoad);
        
        
        homeButton.onClick.AddListener(OnClickHome);
        closeButton.onClick.AddListener(OnClickClose);
    }

    public void StartGame()
    {
        InitializeAndLoadStory(defaultStoryName,defaultStartLine);
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

    #endregion
    
    #region Display
    private void DisplayNextLine()
    {
        //如果当前行比之前到过的最远行还远，就更新最大到达行，并更新全局最大行中当前故事的最大行
        if (currentLine > maxReachedLine)
        {
            maxReachedLine = currentLine;
            globalMaxReachedLines[currentStoryName] = maxReachedLine;
        }
        
        //当前故事文本到达结尾执行下列判断
        if (currentLine >= storyData.Count-1)
        {
            //停止自动播放
            if (isAutoPlay)
            {
                isAutoPlay = false;
                UpdateButtonImage(Constants.AUTO_OFF,autoPlayButton);
            }
            //如果是故事结尾，输出故事结尾
            if (storyData[currentLine].speaker == Constants.END_OF_STORY)
            {
                Debug.Log($"{Constants.END_OF_STORY}");
                return;
            }
            //如果是选项，显示选项
            if (storyData[currentLine].speaker == Constants.CHOICE)
            {
                
                ShowChoices();
                return;
            }
            //做完判断后直接返回
            return;
        }

        //如果处于打字过程中，则直接完成该次打字；否则就直接显示这一行
        if (typerwriterEffect.IsTyping)
        {
            typerwriterEffect.CompleteLine();
        }
        else
        {
            DisplayThisLine();
        }
        
    }

    void DisplayThisLine()
    {
        var data = storyData[currentLine];
        speakerName.text = data.speaker;

        currentSpeakingContent = data.content;
        //显示说话内容
        typerwriterEffect.StartTyping(currentSpeakingContent,currentTypeInterval);
        
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
    
    bool NotNullNorEmpty(string str)
    {
        return !string.IsNullOrEmpty(str);
    }

    void RecoverLastBGAndCharacter()
    {
        var data  = storyData[currentLine];
        if (NotNullNorEmpty(data.lastBackgrouondImage))
        {
            UpdateBackgroundImage(data.lastBackgrouondImage);
        }

        if (NotNullNorEmpty(data.lastBackgroundMusic))
        {
            PlayBackgroundMusic(data.lastBackgroundMusic);
        }

        if (data.cha1Action != Constants.CHARACTER_IMAGE_APPEAR
            && NotNullNorEmpty(data.cha1Image))
        {
            UpdateCharacterImage(Constants.CHARACTER_IMAGE_APPEAR,data.cha1Image,cha1Image,data.lastCoordinateX1);
        }

        if (data.cha2Action != Constants.CHARACTER_IMAGE_APPEAR
            && NotNullNorEmpty(data.cha2Image))
        {
            UpdateCharacterImage(Constants.CHARACTER_IMAGE_APPEAR,data.cha2Image,cha2Image,data.lastCoordinateX2);
        }
    }
    
    #endregion
    
    #region Choices
    void ShowChoices()
    {
        var data = storyData[currentLine];
        
        //先移除所有监听者
        choiceButton1.onClick.RemoveAllListeners();
        choiceButton2.onClick.RemoveAllListeners();
        
        //激活选择面板
        choicePanel.SetActive(true);
        
        //重新添加监听者，按照excel表格中存储的顺序的列的名称读取
        choiceButton1.GetComponentInChildren<TextMeshProUGUI>().text = data.content;
        choiceButton1.onClick.AddListener(()=>InitializeAndLoadStory(data.avatorImageFileName,defaultStartLine));
        choiceButton2.GetComponentInChildren<TextMeshProUGUI>().text = data.vocalAudioFileName;
        choiceButton2.onClick.AddListener(()=>InitializeAndLoadStory(data.backgroundImageFileName,defaultStartLine));
        
        
    }
    
    #endregion
    
    #region Images
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
            Debug.Log($"{Constants.IMAGE_LOAD_ERROR} {imageFilePath}");
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
                image.DOFade(1f, (isLoad?0:Constants.DEFAULT_FADE_DURATION)).From(0);
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
        
    }

    void UpdateButtonImage(string buttonImageName, Button button)
    {
        string imagePath = Constants.BUTTON_IMAGE_PATH + buttonImageName;
        UpdateImage(imagePath, button.image);
    }
    
    #endregion
    
    #region Audio
    void PlayAudio(string audioFilePath, AudioSource audioSource, bool isLoop = false)
    {
        AudioClip audioClip = Resources.Load<AudioClip>(audioFilePath);
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.gameObject.SetActive(true);
            audioSource.loop = isLoop;
            audioSource.Play();
        }
        else
        {
            Debug.Log($"{Constants.AUDIO_LOAD_ERROR} {audioFilePath}");
        }
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

    #endregion
    
    #region Buttons
    #region Variables
    Coroutine autoPlayCoroutine;
    #endregion
    
    #region AutoPlay
    void OnClickAutoPlay()
    {
        isAutoPlay = !isAutoPlay;
        UpdateButtonImage((isAutoPlay ? Constants.AUTO_ON : Constants.AUTO_OFF), autoPlayButton);
        if (isAutoPlay)
        {
            autoPlayCoroutine = StartCoroutine(StartAutoPlay());
        }
        else
        {
            StopCoroutine(autoPlayCoroutine);
        }
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
    #endregion
    
    #region Skip
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
        UpdateButtonImage(Constants.SKIP_ON, skipButton);
        currentTypeInterval = Constants.SKIP_MODE_TYPING_INTERVAL;
        StartCoroutine(SkipToMaxReachedLine());
    }

    void EndSkip()
    {
        //改变状态量
        isSkip = false;
        //恢复正常的打字机速度
        currentTypeInterval = Constants.DEFAULT_TYPING_INTERVAL;
        //更新UI
        UpdateButtonImage(Constants.SKIP_OFF, skipButton);
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

    #endregion
    
    #region SaveAndLoad
    void OnClickSave()
    {
        CloseUI();
        Texture2D screenshot = screenShotter.CaptureScreenShot();
        screenshotData = screenshot.EncodeToPNG();  //将Texture2D转换为PNG格式的数组
        SaveLoadManager.Instance.ShowSavePanel(SaveGame);
        OpenUI();
    }

    void SaveGame(int slotIndex)
    {
        var saveData = new SaveData
        {
            saveCurrentLine = currentLine,
            saveCurrentStory = currentStoryName,
            saveCurrentSpeakingContent = currentSpeakingContent,
            saveScreenshotData = screenshotData,
        };
        string savePath = Path.Combine(saveFolderPath,slotIndex+Constants.SAVE_FILE_EXTENSION);
        string json =  JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(savePath, json);
    }

    public class SaveData
    {
        public int saveCurrentLine;
        public string saveCurrentStory;
        public string saveCurrentSpeakingContent;
        public byte[] saveScreenshotData;
    }

    void OnClickLoad()
    {
        //SaveLoadManager.Instance.ShowSavePanel(LoadGame);
        ShowLoadPanel(null);
    }

    public void ShowLoadPanel(Action action)
    {
        SaveLoadManager.Instance.ShowLoadPanel(LoadGame,action);
    }

    void LoadGame(int slotIndex)
    {
        string savePath = Path.Combine(saveFolderPath, slotIndex+Constants.SAVE_FILE_EXTENSION);
        if (File.Exists(savePath))
        {
            isLoad = true;
            string json  = File.ReadAllText(savePath);
            var saveData = JsonConvert.DeserializeObject<SaveData>(json);
            var lineNumber = saveData.saveCurrentLine - 1;
            InitializeAndLoadStory(saveData.saveCurrentStory,lineNumber);
        }
    }

    #endregion
    
    #region Home
    void OnClickHome()
    {
        gamePanel.SetActive(false);
        MenuManager.Instance.m_menuPanel.SetActive(true);
    }
    #endregion
    
    #region Close
    void OnClickClose()
    {
        CloseUI();
    }

    void OpenUI()
    {
        dialoguePanel.SetActive(true);
        bottomButtons.SetActive(true);
    }

    void CloseUI()
    {
        dialoguePanel.SetActive(false);
        bottomButtons.SetActive(false);
    }
    
    #endregion

    #region Bottom
    bool IsHittingBottomButtons()
    {
        //判断鼠标位置是否在按钮上
        return RectTransformUtility.RectangleContainsScreenPoint(bottomButtons.GetComponent<RectTransform>(),Input.mousePosition,Camera.main);
    }
    #endregion
    #endregion
    
}
