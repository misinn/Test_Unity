using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TitleCanvas : MonoBehaviour
{
    public Button blockbutton;
    public Button boardbutton;
    private CanvasManager CanvasManager;
    void Start()
    {
        CanvasManager = GetComponentInParent<CanvasManager>();
        blockbutton.onClick.AddListener(CanvasManager.OnBlockButtonClick);
        boardbutton.onClick.AddListener(CanvasManager.OnBoardButtonClick);
    }
    public void Close()
    {
        this.gameObject.SetActive(false);
    }
    public void Open()
    {
        this.gameObject.SetActive(true);
    }
}
