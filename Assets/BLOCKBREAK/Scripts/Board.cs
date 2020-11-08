using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
[RequireComponent(typeof(Rigidbody))]
public class Board : Agent
{
    GameManager gamemanager;
    BoardController boardController;
    BehaviorParameters BehaviorParameters;
    public Vector3 Velocity => boardController.Velocity;
    public Observation Observation;
    public float defaultAccel = 400f;
    public float defaultMaxSpeed = 9f;
    [Range(0,1)] public float friction = 0f;
    [HideInInspector] public float accel;
    [HideInInspector] public float maxSpeed;
    public float actionRange = 2f;
    public float speedAffectRange = 0.5f;
    public Operator _operator { get; set; }
    [HideInInspector] public int session = 2;
    public override void Initialize()
    {
        boardController = GetComponentInChildren<BoardController>();
        gamemanager = GetComponentInParent<GameManager>();
        BehaviorParameters = GetComponent<BehaviorParameters>();
        Observation = new Observation(8);
        session = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("session", 2);
        accel = defaultAccel;
        maxSpeed = defaultMaxSpeed;
        boardController.friction = friction;
    }
    public void SetUp(Vector3 pos, Operator _operator)
    {
        accel = defaultAccel;
        maxSpeed = defaultMaxSpeed;
        Position = pos;
        this._operator = _operator;
        boardController.Init();
        switch (_operator)
        {
            case Operator.Player:
                BehaviorParameters.BehaviorType = BehaviorType.HeuristicOnly;
                break;
            case Operator.ML:
                BehaviorParameters.BehaviorType = BehaviorType.Default;
                break;
            default:
                BehaviorParameters.BehaviorType = BehaviorType.InferenceOnly;
                break;
        }
    }
    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        float[] obs = Observation;
        for (int i = 0; i < obs.Length; i++)
        {
            sensor.AddObservation(obs[i]);
        }
    }
    public float lastaction = 0;
    public override void OnActionReceived(float[] vectorAction)
    {
        var action = vectorAction[0];
        float forcex;
        if (_operator != Operator.Player)
        {
            var ballpos = gamemanager.ball.gameObject.transform.localPosition.x;
            var boardpos = boardController.gameObject.transform.localPosition.x;
            var dif = boardpos - ballpos - action * actionRange;
            forcex = -CalcForce(dif, speedAffectRange);
            boardController.Move(forcex, accel, maxSpeed);
            lastaction = action;
            return;
        }
        forcex = action;
        boardController.Move(forcex, accel, maxSpeed);
        lastaction = 0f;
    }
    public override void Heuristic(float[] actionsOut)
    {
        var action = 0;
        if (_operator == Operator.Player)
        {
            if (Input.GetKey(KeyCode.RightArrow)) action = 1;
            else if (Input.GetKey(KeyCode.LeftArrow)) action = -1;
        }
        actionsOut[0] = action;
    }

    public Vector3 LocalPos
    {
        get
        {
            return boardController.gameObject.transform.localPosition;
        }
    }
    public Vector3 Position
    {
        get
        {
            return boardController.gameObject.transform.position;
        }
        set
        {
            boardController.gameObject.transform.position = value;
        }
    }
    /// <summary>
    /// 最大値を定め、返す。 [-1 ,1]f
    /// </summary>
    /// <param name="dif"></param>
    /// <param name="maxRange"></param>
    /// <returns></returns>
    private float CalcForce(float dif, float maxRange)
    {
        if (Mathf.Abs(dif) > maxRange)
        {
            dif /= Mathf.Abs(dif);
        }
        else
        {
            dif /= maxRange;
        }
        return dif;
    }
    public enum Operator
    {
        Player,
        AI,
        ML
    }
    readonly Vector3 Zero = Vector3.zero;
    readonly Vector3 Right = Vector3.right;
    readonly Vector3 Left = Vector3.left;
}