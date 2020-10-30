using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents.Sensors;
using static UnityEngine.Mathf;
public class GameManager : MonoBehaviour
{
    public Ball ball;
    public Board board;
    public Block block;
    public int blockcount;
    public int MaxStep;
    private Block[] blocks;
    private Renderer[] blocksColor;
    public Color[] Colors;
    private Vector3 defaltBallPos;
    private Vector3 defaltBoardPos;
    private Vector3[] defaltBlocksPos = new Vector3[60];
    private Color[] defaltBlocksColor = new Color[60];
    private int session = 0;
    private int[] blockslastaction;
    public void Start()
    {
        Application.targetFrameRate = 60;
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
        blocks[0] = block;
        blocksColor[0] = block.gameObject.GetComponent<Renderer>();
        blocks[0].Initialize();
        blockslastaction = new int[blockcount];
        for (int i = 1; i < blockcount; i++)
        {
            var obj = Instantiate(block,transform);
            blocks[i] = obj.GetComponent<Block>();
            obj.GetComponent<RayPerceptionSensorComponent3D>().SensorName = ("BlockSensor"+i).ToString();
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
        Reset();
    }
    public void Reset()
    {
        //ball
        ball.transform.position = defaltBallPos;
        ball.velocity = new Vector3(0f, 0f, -1f) * ball.speed;
        ball.Skip = true;
        if (session == 0) ball.velocity.x += Random.Range(-1.5f, 1.5f);
        //board
        board.Position = defaltBoardPos;
        //blocks
        for (int i = 0; i < blockcount; i++)
        {
            blocks[i].Enable();
        }
        int[] nums = Enumerable.Range(0, 60).OrderBy(l => System.Guid.NewGuid()).ToArray();
        for (int i = 0; i < blockcount; i++)
        {
            blocks[i].transform.localPosition = defaltBlocksPos[nums[i]];
            blocksColor[i].material.color = defaltBlocksColor[nums[i]];
        }
        SetEnvParameter();

        for (int i = 0; i < blockcount; i++)
        {
            blocks[i].SetTrigger(session == 0);
        }
        
    }
    public void Update()
    {
        ManageGameLoop();
        ObservationUpdate();
        RewardUpdate();
        ActionUpdate();
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
            ballvec = vec / ballmaxvec;
        }
        //ボード
        var boardpos = board.LocalPos;
        var boardvec = board.Velocity;
        {
            var pos = board.LocalPos;
            var vec = board.Velocity;
            boardpos = new Vector3((pos.x - wallR) / (wallL - wallR), .5f, (pos.z - wallD) / (wallU - wallD));
            boardvec = vec / boardmaxvec;
        }
        //ブロック
        var blockposvec = new (Vector3 pos, Vector3 vec)[blockcount];
        for (int i = 0; i < blockcount; i++)
        {
            var pos = blocks[i].transform.localPosition;
            var vec = blocks[i].transform.localPosition;
            blockposvec[i].pos = new Vector3((pos.x - wallR) / (wallL - wallR), .5f, (pos.z - wallD) / (wallU - wallD));
            blockposvec[i].vec = vec / blocksmaxvec;
        }
        System.Array.Sort(blockposvec, (a, b) => -a.pos.z.CompareTo(b.pos.z));
        //情報の記録・伝達
        //ボード
        BoardObservation.Init();
        BoardObservation.AddObservation(ballpos.x);
        BoardObservation.AddObservation(ballpos.z);
        BoardObservation.AddObservation(ballvec.x);
        BoardObservation.AddObservation(ballvec.z);
        BoardObservation.AddObservation(boardpos.x);
        BoardObservation.AddObservation(boardvec.x);

        board.Observation = BoardObservation;
        //ブロック
        for (int i = 0; i < blockcount; i++)
        {
            ref var obs =ref BlockObservation[i];
            var blockpos = blockposvec[i].pos;
            var blockvec = blockposvec[i].vec;
            obs.Init();
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
        if (GetActiveBlockCount() == 0)
        {
            AddRewardBoard(boardreward.clear);
            EndEpisode();
        }
        AddRewardBoard(boardreward.time);
        //ブロック
        for (int i = 0; i < blockcount; i++)
        {
            if (!blocks[i].IsActive) continue;
            if (blocks[i].Velocity.magnitude > 1f) AddRewardBlock(blocks[i], blockreward.move);
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
        board.RequestDecision();
        for (int i = 0; i < blockcount; i++)
        {
            if (!blocks[i].IsActive) continue;
            blocks[i].RequestDecision();
        }
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
                time = -0.0008f
            };
        }
        else if(session==1)
        {
            boardreward = new BoardRewards
            {
                ballhit = 0.15f,
                blockhit = 0.01f,
                drop = -1f,
                clear = 0.25f,
                time = 0.001f
            };
        }
        else if(session == 2)
        {
            boardreward = new BoardRewards
            {
                ballhit = 0.025f,
                blockhit = 0.1f,
                drop = -1f,
                clear = 0.5f,
                time = -0.0002f
            };
        }
        
        blockreward = new BlockRewards
        {
            ballhit = -0.3f,
            time = 0.0005f,
            actionchange = -0.0025f,
            move = 0.001f
        };
    }
    private void ManageGameLoop()
    {
        int step = board.StepCount;
        if (step <= MaxStep) return;
        EndEpisode();
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
        AddRewardBlock(block,blockreward.ballhit);
        block.gameObject.SetActive(false);
    }
    public void OnBallDroped()
    {
        AddRewardBoard(boardreward.drop);
        EndEpisode();
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
        Reset();
    }
    public struct BoardRewards
    {
        public float ballhit;
        public float blockhit;
        public float drop;
        public float clear;
        public float time;
    }
    public struct BlockRewards
    {
        public float ballhit;
        public float time;
        public float actionchange;
        public float move;
    }
    public int GetActiveBlockCount()
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

    const float blocksmaxvec = 1.5f;
    const float boardmaxvec = 9f;
    const float ballmaxvec = 8f;
}
