using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnRefreshUI();
public enum UIState
{
    _InGameUI,
    _ResultUI, 
}
public class UIManager : MonoBehaviour
{
    private static UIManager m_instance;
    public static UIManager Instance
    {
        get
        {
            if (m_instance != null) { return m_instance; }

            m_instance = FindObjectOfType<UIManager>();

            if (m_instance == null) { m_instance = new GameObject(name: "UIManager").AddComponent<UIManager>(); }
            return m_instance;
        }
    }
    public UIState curState { private set; get; }
    public List<UIBase> uiDataLists;

    public event OnRefreshUI onRefreshUserInfoUI;


    public void Init()
    {
        for (int i = 0; i < uiDataLists.Count; i++)
        {
            if (uiDataLists[i] != null)
                uiDataLists[i].Init();
        }
    }

    public void RefreshUserInfo()
    {
        onRefreshUserInfoUI?.Invoke();
    }

    public void ShowUI(UIState state)
    {
        curState = state;

        if (uiDataLists.Count >= (int)state && uiDataLists[(int)state] != null)
        {
            uiDataLists[(int)state].ShowUI();
        }
    }
    public void HideUI(UIState state)
    {
        if (uiDataLists.Count >= (int)state && uiDataLists[(int)state] != null)
            uiDataLists[(int)state].HideUI();
    }
    public UIBase GetUI(UIState state)
    {
        return uiDataLists[(int)state];
    }
}
