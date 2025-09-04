using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    public GameObject m_saveLoadPanel;
    public TextMeshProUGUI m_panelTitle;
    public Button[] m_saveLoadButtons;
    public Button m_nextPageButton;
    public Button m_backButton;
    public Button m_previousPageButton;
    
    private bool m_isSave;
    private int m_currentPage = Constants.DEFAULT_SAVE_START_INDEX;
    private int m_slotsPerPage = Constants.SLOT_PER_PAGE;
    private int m_totalSlots = Constants.TOTAL_SLOTS;

    //当前执行保存还是加载的操作
    private Action<int> m_currentAction;
    private Action m_menuAction;

    void Start()
    {
        m_previousPageButton.onClick.AddListener(PrevPage);
        m_nextPageButton.onClick.AddListener(NextPage);
        m_backButton.onClick.AddListener(GoBack);
        m_saveLoadPanel.SetActive(false);
    }

    public void ShowSavePanel(Action<int> action)
    {
        m_isSave = true;
        //m_panelTitle.text = m_isSave ? Constants.SAVE_GAME : Constants.LOAD_GAME;
        m_panelTitle.text = Constants.SAVE_GAME;
        m_currentAction = action;
        UpdateSaveLoadUI();
        m_saveLoadPanel.SetActive(true);
        
        //LoadStorylineAndScreenshots();
    }
    
    public void ShowLoadPanel(Action<int> action,Action menuAction)
    {
        m_isSave = false;
        //m_panelTitle.text = m_isSave ? Constants.SAVE_GAME : Constants.LOAD_GAME;
        m_panelTitle.text = Constants.LOAD_GAME;
        m_currentAction = action;
        m_menuAction = menuAction;
        UpdateSaveLoadUI();
        m_saveLoadPanel.SetActive(true);
        
        //LoadStorylineAndScreenshots();
    }

    //将显示文本按照语言设置进行转换
    // string GetLocalized(string key)
    // {
    //     return LocalizationManager.Instance.GetLocalizedValue(key);
    // }
    
    void UpdateSaveLoadUI()
    {
        for (int i = 0; i < m_slotsPerPage; i++)
        {
            int slotIndex = m_currentPage * m_slotsPerPage + i;
            if (slotIndex < m_totalSlots)
            {
                //VITAL:先将按钮置为没有存档的样子，再读取数据，如果有存档，他就更新该按钮组件
                UpdateSaveLoadButtons(m_saveLoadButtons[i],slotIndex);
                LoadStorylineAndScreenshots(m_saveLoadButtons[i],slotIndex);
                // m_saveLoadButtons[i].gameObject.SetActive(true);
                // m_saveLoadButtons[i].interactable = true;
                
                
            }
            else
            {
                m_saveLoadButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void UpdateSaveLoadButtons(Button button, int index)
    {
        button.gameObject.SetActive(true);
        button.interactable = true;

        var savePath = GenerateDataPath(index);
        var fileExist = File.Exists(savePath);

        //若在加载模式下并且存档不存在，则不可交互
        if (!m_isSave && !fileExist)
        {
            button.interactable = false;
        }
        
        var textComponents = button.GetComponentsInChildren<TextMeshProUGUI>();
                
        textComponents[0].text = null;
        textComponents[1].text = (index + 1) + Constants.COLON + Constants.EMPTY_SLOT;
        button.GetComponentInChildren<RawImage>().texture = null;
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(()=>OnButtonClick(button,index));
    }

    void OnButtonClick(Button button, int index)
    {
        //m_currentAction(index);
        m_menuAction?.Invoke();
        m_currentAction?.Invoke(index);
        if (m_isSave)
        {
            LoadStorylineAndScreenshots(button, index);
        }
        else
        {
            GoBack();
        }
    }
    
    void PrevPage()
    {
        if (m_currentPage > 0)
        {
            m_currentPage--;
            UpdateSaveLoadUI();
            //LoadStorylineAndScreenshots();
        }
    }

    void NextPage()
    {
        if ((m_currentPage + 1) * m_slotsPerPage < m_totalSlots)
        {
            m_currentPage++;
            UpdateSaveLoadUI();
            //LoadStorylineAndScreenshots();
        }
    }

    void GoBack()
    {
        m_saveLoadPanel.SetActive(false);
    }

    void LoadStorylineAndScreenshots(Button button, int slotIndex)
    {
        var savePath = GenerateDataPath(slotIndex);
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            var saveData = JsonConvert.DeserializeObject<VNManager.SaveData>(json);
            if (saveData.saveScreenshotData != null)
            {
                Texture2D screenshot = new Texture2D(2, 2);
                screenshot.LoadImage(saveData.saveScreenshotData);
                button.GetComponentInChildren<RawImage>().texture = screenshot;
            }

            if (saveData.saveCurrentSpeakingContent != null)
            {
                var textComponents = button.GetComponentsInChildren<TextMeshProUGUI>();
                textComponents[0].text = saveData.saveCurrentSpeakingContent;
                textComponents[1].text = File.GetLastWriteTime(savePath).ToString("G");
            }
        }
    }

    private string GenerateDataPath(int index)
    {
        return Path.Combine(Application.persistentDataPath,Constants.SAVE_FILE_PATH,index+Constants.SAVE_FILE_EXTENSION);
    }
}
