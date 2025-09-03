using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuManager : Singleton<MenuManager>
{
    [FormerlySerializedAs("m_memuPanel")] public GameObject m_menuPanel;
    public Button m_startGameButton;
    public Button m_continueGameButton;
    public Button m_loadGameButton;
    public Button m_gameSettingsButton;
    public Button m_exitGameButton;
    
    //判断游戏是否已经开始
    private bool m_hasStarted = false;
    

    void Start()
    {
        MenuButtonsAddListener();
    }

    void MenuButtonsAddListener()
    {
        m_startGameButton.onClick.AddListener(StartGame);
        m_continueGameButton.onClick.AddListener(ContinueGame);
    }

    void StartGame()
    {
        m_hasStarted = true;
        VNManager.Instance.StartGame();
        m_menuPanel.SetActive(false);
        VNManager.Instance.gamePanel.SetActive(true);
    }

    void ContinueGame()
    {
        if (m_hasStarted)
        {
            m_menuPanel.SetActive(false);
            VNManager.Instance.gamePanel.SetActive(true);
        }
    }
}
