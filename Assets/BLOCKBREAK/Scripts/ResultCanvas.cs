using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ResultCanvas : MonoBehaviour
{
    public Button continueButton;
    public Button exitButton;
    private CanvasManager CanvasManager;
    void Start()
    {
        CanvasManager = GetComponentInParent<CanvasManager>();
        continueButton.onClick.AddListener(CanvasManager.OnContinueButtonClick);
        exitButton.onClick.AddListener(CanvasManager.OnExitButtonClick);
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
