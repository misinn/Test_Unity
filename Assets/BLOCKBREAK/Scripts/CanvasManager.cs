using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public TitleCanvas TitleCanvas;
    //public GameCanvas GameCanvas; TODO
    public ResultCanvas ResultCanvas;
    private BlockBreakManager BlockBreakManager;
    void Start()
    {
        BlockBreakManager = GetComponentInParent<BlockBreakManager>();
    }
    //タイトル画面
    public void OnBlockButtonClick()
    {
        BlockBreakManager.GameSetUp(GameMode.block);
    }
    public void OnBoardButtonClick()
    {
        BlockBreakManager.GameSetUp(GameMode.board);
    }
    //ゲーム画面

    //終了画面
    public void OnContinueButtonClick()
    {
        BlockBreakManager.GameContinue();
    }
    public void OnExitButtonClick()
    {
        BlockBreakManager.GameExit();
    }
}
