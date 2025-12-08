using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    public virtual void Init() { }

    public virtual void ActiveOff()
    {
        gameObject.SetActive(false);
    }

    public virtual void ActiveOn()
    {
        gameObject.SetActive(true);
    }

    public virtual void ShowUI()
    {
        gameObject.SetActive(true);
    }

    public virtual void HideUI()
    {
        gameObject.SetActive(false);
    }
}
