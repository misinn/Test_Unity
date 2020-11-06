using System.Collections;
using UnityEngine;

public class BlockBreakManager : MonoBehaviour
{
    public bool IsML = false;
    public GameManager GameManager;
    public CanvasManager CanvasManager;
    [SerializeField] GameMode gameMode;
    public GameMode GameMode => gameMode;
    private GameStatus GameStatus;
    void Start()
    {
        CanvasManager.TitleCanvas.Close();
        CanvasManager.ResultCanvas.Close();
        if (IsML)
        {
            GameStatus = GameStatus.ML;
            GameManager.GameSetUp(GameMode.ML);
            GameManager.GameStart();
        }
        else
        {
            GameStatus = GameStatus.title;
            CanvasManager.TitleCanvas.Open();
        }
    }
    public void GameSetUp(GameMode gameMode)
    {
        GameStatus = GameStatus.standby;
        this.gameMode = gameMode;
        CanvasManager.TitleCanvas.Close();
        GameManager.GameSetUp(gameMode);
        StartCoroutine(GameReady());
    }
    private IEnumerator GameReady()
    {
        while (true)
        {
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow) ||
                Input.GetKey(KeyCode.RightArrow)|| Input.GetKey(KeyCode.LeftArrow))
            {
                GameStart();
                yield break;
            }
            yield return null;
        }
    }
    private void GameStart()
    {
        if (GameStatus != GameStatus.standby) return;
        GameStatus = GameStatus.play;
        GameManager.GameStart();
    }
    public void GameEnd()
    {
        if (GameStatus != GameStatus.play) return;
        GameStatus = GameStatus.result;
        CanvasManager.ResultCanvas.Open();
    }
    public void GameExit()
    {
        if (GameStatus != GameStatus.result) return;
        GameStatus = GameStatus.title;
        CanvasManager.ResultCanvas.Close();
        CanvasManager.TitleCanvas.Open();
    }
    public void GameContinue()
    {
        if (GameStatus != GameStatus.result) return;
        GameStatus = GameStatus.standby;
        CanvasManager.ResultCanvas.Close();
        GameSetUp(this.gameMode);
    }
    
}
