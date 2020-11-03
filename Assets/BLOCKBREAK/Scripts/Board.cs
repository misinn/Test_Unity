using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Board : Agent
{
    private GameManager gamemanager;
    private BoardController boardController;
    public Vector3 Velocity => boardController.Velocity;
    public Observation Observation;
    public float defaultAccel = 400f;
    public float defaultMaxSpeed = 9f;
    [Range(0,1)] public float friction = 0f;
    [HideInInspector] public float accel;
    [HideInInspector] public float maxSpeed;
    public bool automove = false;
    public float actionRange = 2f;
    public float speedAffectRange = 0.5f;
    [HideInInspector] public int session = 0;
    public override void Initialize()
    {
        boardController = GetComponentInChildren<BoardController>();
        gamemanager = GetComponentInParent<GameManager>();
        Observation = new Observation(7);
        session = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("session", 2);
        accel = defaultAccel;
        maxSpeed = defaultMaxSpeed;
        boardController.friction = friction;
    }
    public override void OnEpisodeBegin()
    {
        session = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("session", 2);
        accel = defaultAccel;
        maxSpeed = defaultMaxSpeed;
        boardController.Init();
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
        var forcex = 0f;
        if (automove)
        {
            var ballpos = gamemanager.ball.gameObject.transform.localPosition.x;
            var boardpos = boardController.gameObject.transform.localPosition.x;
            var dif = boardpos - ballpos - action * actionRange;
            forcex = -CalcForce(dif, speedAffectRange);
            boardController.Move(forcex, accel, maxSpeed);
            lastaction = action;
            return;
        }
        //if not auto 使わない
        forcex = action;
        boardController.Move(forcex, accel, maxSpeed);
    }
    public override void Heuristic(float[] actionsOut)
    {
        var action = 0;
        if (Input.GetKey(KeyCode.RightArrow)) action = 1;
        else if (Input.GetKey(KeyCode.LeftArrow)) action = -1;
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
    readonly Vector3 Zero = Vector3.zero;
    readonly Vector3 Right = Vector3.right;
    readonly Vector3 Left = Vector3.left;
}