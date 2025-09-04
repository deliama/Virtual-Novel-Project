using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HistoryManager : Singleton<HistoryManager>
{
    public GameObject historyScrollView;

    public Transform content;
    public GameObject historyItemPrefab;
    public Button closeButton;
    
    LinkedList<string> historyRecords = new LinkedList<string>();

    private void Start()
    {
        historyScrollView.SetActive(false);
        closeButton.onClick.AddListener(CloseHistory);
    }

    public void ShowHistory(LinkedList<string> records)
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        historyRecords = records;
        LinkedListNode<string> currentRecord = historyRecords.Last;

        while (currentRecord != null)
        {
            AddHistoryItem(currentRecord.Value);
            currentRecord = currentRecord.Previous;
        }
        
        content.GetComponent<RectTransform>().localPosition = Vector3.zero;
        historyScrollView.SetActive(true);
        
    }

    void CloseHistory()
    {
        historyScrollView.SetActive(false);
    }

    void AddHistoryItem(string text)
    {
        GameObject historyItem = Instantiate(historyItemPrefab, content);
        historyItem.GetComponentInChildren<TextMeshProUGUI>().text = text;
        //将当前历史记录放在顶部
        historyItem.transform.SetAsFirstSibling();
    }
}
