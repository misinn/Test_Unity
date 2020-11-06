using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Barracuda;
using Unity.MLAgents.Sensors;
using static UnityEngine.Mathf;

public class GameManager : MonoBehaviour
{
    //ゲームループ関連
    public int MaxStep;
    public int targetFrameRate = 60;
    [HideInInspector]public int session = 0;
    BlockBreakManager BlockBreakManager;
    bool IsML;
    bool IsGameStart = false;
    int activeblockcount = 0;
    //ボール
    public Ball ball;
    Vector3 defaltBallPos;
    //ボード
    public Board board;
    public NNModel boardModel;
    Vector3 defaltBoardPos;
    float boardlastaction;
    //ブロック
    [SerializeField]
    Block block;
    public NNModel blockModel;
    public int blockcount;
    public Color[] Colors;
    public Block playerBlock;
    Block[] blocks;
    Renderer[] blocksColor;
    Vector3[] defaltBlocksPos = new Vector3[60];
    Color[] defaltBlocksColor = new Color[60];
    int[] blockslastaction;
    
    public void Start()
    {
        Application.targetFrameRate = targetFrameRate;
        BlockBreakManager = GetComponentInParent<BlockBreakManager>();
        IsML = BlockBreakManager.IsML;
        IsGameStart = false;
        //ボール
        defaltBallPos = ball.transform.position;
        //ボード
        board.Initialize();
        defaltBoardPos = board.Position;
        BoardObservation = board.Observation;
        //ブロック関連
        blocks = new Block[blockcount];
        blocksColor = new Renderer[blockcount];
        BlockObservation = new Observation[blockcount];
        blockslastaction = new int[blockcount];
        for (int i = 0; i < blockcount; i++)
        {
            if (i == 0)
            {
                if (BlockBreakManager.GameMode == GameMode.block)
                {
                    playerBlock.Enable();
                    blocks[i] = playerBlock;
                    blocksColor[i] = playerBlock.gameObject.GetComponent<Renderer>();
                    blocks[i].Initialize();
                    block.Disable();
                }
                else
                {
                    block.Enable();
                    blocks[i] = block;
                    blocksColor[i] = block.gameObject.GetComponent<Renderer>();
                    blocks[i].Initialize();
                    playerBlock.Disable();
                }
                continue;
            }
            var obj = Instantiate(block, transform);
            blocks[i] = obj.GetComponent<Block>();
            obj.GetComponent<RayPerceptionSensorComponent3D>().SensorName = ("BlockSensor" + i).ToString();
            blocksColor[i] = obj.GetComponent<Renderer>();
            blocks[i].Initialize();
        }
        for (int i = 0; i < 60; i++)
        {
            int xc = i % 12;
            int zc = i / 12;
            defaltBlocksPos[i] = new Vector3(-5.5f + xc, 0.25f, 1.5f + zc * 0.5f);
            defaltBlocksColor[i] = Colors[zc];
        }
        for (int i = 0; i < blockcount; i++)
        {
            BlockObservation[i] = blocks[i].Observation;
        }
    }
    public void GameSetUp(GameMode gameMode) //ゲームを準備する。
    {
        //gameloop
        IsGameStart = false;
        
        //ball
        ball.transform.position = defaltBallPos;
        ball.speed = ball.DefaultSpeed;
        ball.velocity = Vector3.zero;
        if (session == 0) ball.velocity.x += Random.Range(-1.5f, 1.5f);
        //board
        board.Position = defaltBoardPos;
        if (gameMode == GameMode.board)
        {
            board.IsPlayer = true;
        }
        else board.IsPlayer = false;
        board.SetUp();
        //blocks
        for (int i = 0; i < blockcount; i++)
        {
            if (i == 0)
            {
                if (gameMode == GameMode.block)
                {
                    playerBlock.Enable();
                    blocks[i] = playerBlock;
                    blocksColor[i] = playerBlock.gameObject.GetComponent<Renderer>();
                    blocks[i].Initialize();
                    block.Disable();
                }
                else
                {
                    block.Enable();
                    blocks[i] = block;
                    blocksColor[i] = block.gameObject.GetComponent<Renderer>();
                    blocks[i].Initialize();
                    blocks[i].UseModel = false;
                    playerBlock.Disable();
                }
                continue;
            }
            blocks[i].Enable();
        }
        int[] nums = Enumerable.Range(0, 60).OrderBy(l => System.Guid.NewGuid()).ToArray();
        for (int i = 0; i < blockcount; i++)
        {
            blocks[i].Stop();
            blocks[i].transform.localPosition = defaltBlocksPos[nums[i]];
            blocksColor[i].material.color = defaltBlocksColor[nums[i]];
        }
        SetEnvParameter();
        for (int i = 0; i < blockcount; i++)
        {
            blocks[i].SetIsTrigger(session == 0);
        }
    }

    public void GameStart() //準備したゲームを開始するトリガー
    {
        IsGameStart = true;
        ball.velocity = new Vector3(0f, 0f, -1f) * ball.DefaultSpeed;
        ball.GameStart();
        ball.Skip = true;
    }
    public void Update()
    {
        if (!IsGameStart) return;
        ManageGameLoop();
        ObjParameterUpdate();
        ObservationUpdate();
        RewardUpdate();
        ActionUpdate();
    }
    private void ManageGameLoop()
    {
        int step = board.StepCount;
        activeblockcount = ActiveBlockCount();
        if ((BlockBreakManager.GameMode==GameMode.ML && step > MaxStep) || activeblockcount == 0)
        {
            AddRewardBoard(boardreward.clear);
            GameEnd();
        }
    }
    private float rate = 0f;
    private void ObjParameterUpdate()
    {
        rate = ball.speed / ball.DefaultSpeed;
        board.accel = board.defaultAccel * rate;
        board.maxSpeed = board.defaultMaxSpeed * rate;
    }
    private void ObservationUpdate()
    {
        //情報の正規化
        //ボール
        var ballpos = ball.transform.localPosition;
        var ballvec = ball.velocity;
        {
            var pos = ball.transform.localPosition;
            var vec = ball.velocity;
            ballpos = new Vector3((pos.x - wallR) / (wallL - wallR), .5f, (pos.z - wallD) / (wallU - wallD));
        }
        //ボード
        var boardpos = board.LocalPos;
        var boardvec = board.Velocity;
        {
            var pos = board.LocalPos;
            var vec = board.Velocity;
            boardpos = new Vector3((pos.x - wallR) / (wallL - wallR), .5f, (pos.z - wallD) / (wallU - wallD));
        }
        //ブロック
        var blockposvec = new (Vector3 pos, Vector3 vec)[blockcount];
        for (int i = 0; i < blockcount; i++)
        {
            var pos = blocks[i].transform.localPosition;
            var vec = blocks[i].transform.localPosition;
            blockposvec[i].pos = new Vector3((pos.x - wallR) / (wallL - wallR), .5f, (pos.z - wallD) / (wallU - wallD));
        }
        System.Array.Sort(blockposvec, (a, b) => -a.pos.z.CompareTo(b.pos.z));
        //情報の記録・伝達
        //ボード
        BoardObservation.Init();
        BoardObservation.AddObservation(rate);
        BoardObservation.AddObservation(ballpos.x);
        BoardObservation.AddObservation(ballpos.z);
        BoardObservation.AddObservation(ballvec.x);
        BoardObservation.AddObservation(ballvec.z);
        BoardObservation.AddObservation(boardpos.x);
        BoardObservation.AddObservation(boardvec.x);
        BoardObservation.AddObservation(boardlastaction);
        board.Observation = BoardObservation;
        //ブロック
        for (int i = 0; i < blockcount; i++)
        {
            ref var obs = ref BlockObservation[i];
            var blockpos = blockposvec[i].pos;
            var blockvec = blockposvec[i].vec;
            obs.Init();
            obs.AddObservation(rate);
            obs.AddObservation(ballpos.x);
            obs.AddObservation(ballpos.z);
            obs.AddObservation(ballvec.x);
            obs.AddObservation(ballvec.z);
            obs.AddObservation(boardpos.x);
            obs.AddObservation(blockpos.x);
            obs.AddObservation(blockpos.z);
            obs.AddObservation(blockslastaction[i]);
            blocks[i].Observation = BlockObservation[i];
        }
    }
    public Observation BoardObservation;
    public Observation[] BlockObservation;
    private void RewardUpdate()
    {
        //ボード
        if (session == 0)
        {
            AddRewardBoard(boardreward.time);
            return;
        }
        AddRewardBoard(boardreward.actionchange * Abs(board.lastaction - boardlastaction));
        AddRewardBoard(boardreward.time * Log(activeblockcount + 1, 2));
        boardlastaction = board.lastaction;
        //ブロック
        for (int i = 0; i < blockcount; i++)
        {
            if (!blocks[i].IsActive) continue;
            if (blocks[i].Velocity.magnitude > blocks[i].defaultMaxSpeed * 0.8f) { AddRewardBlock(blocks[i], blockreward.move); }
            if (blocks[i].lastaction != 0)
            {
                if (blocks[i].lastaction != blockslastaction[i])
                {
                    AddRewardBlock(blocks[i], blockreward.actionchange);
                    blockslastaction[i] = blocks[i].lastaction;
                }
            }
            AddRewardBlock(blocks[i], blockreward.time);
        }
    }
    private void ActionUpdate()
    {
        //ボード
        board.RequestDecision();
        //ブロック
        for (int i = 0; i < blockcount; i++)
        {
            if (!blocks[i].IsActive) continue;
            blocks[i].RequestDecision();
        }
        //ボール

    }
    private void GameEnd()
    {
        if (IsML)
        {
            EndEpisode();
            GameSetUp(GameMode.ML);
            GameStart();
            return;
        }
        ball.GameEnd();
        BlockBreakManager.GameEnd();
    }
    public BoardRewards boardreward;
    public BlockRewards blockreward;
    private void SetEnvParameter()
    {
        session = board.session;
        if (session == 0)
        {
            boardreward = new BoardRewards
            {
                ballhit = 0.2f,
                blockhit = 0f,
                drop = -1f,
                clear = 0f,
                time = -0.0008f,
                actionchange = 0f
            };
        }
        else if (session == 1)
        {
            boardreward = new BoardRewards
            {
                ballhit = 0.15f,
                blockhit = 0.01f,
                drop = -1f,
                clear = 0.25f,
                time = -0.001f,
                actionchange = 0f
            };
        }
        else if (session == 2)
        {
            boardreward = new BoardRewards
            {
                ballhit = 0.01f,
                blockhit = 0.15f,
                drop = -1f,
                clear = 0.5f,
                time = -0.0002f,
                actionchange = -0.001f
            };
        }

        blockreward = new BlockRewards
        {
            ballhit = -0.3f,
            time = 0.0005f,
            actionchange = -0.0025f,
            move = 0.0015f
        };
    }
    public Vector2[] GetBlocksPos
    {
        get
        {
            var ans = new Vector2[blockcount];
            for (int i = 0; i < blockcount; i++)
            {
                if (block.IsActive)
                {
                    var pos = block.transform.localPosition;
                    ans[i] = new Vector2(pos.x, pos.z);
                }
                else
                {
                    ans[i] = new Vector2(0f, -10f);
                }
            }
            return ans;
        }
    }
    public void OnBallHitBoard()
    {
        board.AddReward(boardreward.ballhit);
    }
    public void OnBallHitBlock(Block block)
    {
        board.AddReward(boardreward.blockhit);
        block.AddReward(blockreward.ballhit);
        block.gameObject.SetActive(false);
    }
    public void OnBallHitVirtualBlock(Block block)
    {
        float prirew = (ActiveBlockCount() - 1f) / blockcount * blockreward.ballhit;
        float pubrew = 1f / blockcount * blockreward.ballhit;
        AddRewardBlock(block, prirew);
        for (int i = 0; i < blockcount; i++)
        {
            if (blocks[i].IsActive)
            {
                AddRewardBlock(blocks[i], pubrew);
            }
        }
        block.gameObject.SetActive(false);
    }
    public void OnBallDroped()
    {
        AddRewardBoard(boardreward.drop);
        //TODO 自分がブロックの時とボードの時の場合分け
        GameEnd();
    }
    public void OnBlockStayBlockTrigger(Block block)
    {
        AddRewardBlock(block, blockreward.time * -0.5f);
    }
    public void OnBlockHitBlockLimit(Block block)
    {
        AddRewardBlock(block, blockreward.move * -2f);
    }
    public void AddRewardBlock(Block block, float reward)
    {
        block.AddReward(reward);
    }
    public void AddRewardBoard(float reward)
    {
        board.AddReward(reward);
    }
    public void EndEpisode()
    {
        board.EndEpisode();
        for (int i = 0; i < blockcount; i++)
        {
            blocks[i].EndEpisode();
        }
    }
    public struct BoardRewards
    {
        public float ballhit;
        public float blockhit;
        public float drop;
        public float clear;
        public float time;
        public float actionchange;
    }
    public struct BlockRewards
    {
        public float ballhit;
        public float time;
        public float actionchange;
        public float move;
    }
    public int ActiveBlockCount()
    {
        int ans = 0;
        for (int i = 0; i < blockcount; i++)
        {
            if (blocks[i].IsActive) ans++;
        }
        return ans;
    }
    const float wallR = -6.5f;
    const float wallL = 6.5f;
    const float wallD = -5.5f;
    const float wallU = 4.5f;
}