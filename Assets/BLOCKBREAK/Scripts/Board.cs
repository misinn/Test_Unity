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
    public bool automove = false;
    [HideInInspector]public int session = 0;
    public override void Initialize()
    {
        boardController = GetComponentInChildren<BoardController>();
        gamemanager = GetComponentInParent<GameManager>();
        Observation = new Observation(7);
        session = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("session", 2);
    }
    public override void OnEpisodeBegin()
    {
        session = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("session", 2);
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
        var pos = Zero;
        const float num = 2f;
        if (automove)
        {
            var ballpos = gamemanager.ball.gameObject.transform.localPosition.x;
            var boardpos = boardController.gameObject.transform.localPosition.x;
            if (boardpos - ballpos > action) pos = Left*num;
            else pos = Right*num;
            boardController.AddForce(pos);
            lastaction = action;
            return;
        }
        /*  Discrete BranchSize=1 0.Size=3
        if (action == 1) pos = Right;
        else if (action == 2) pos = Left;
        boardController.AddForce(pos);
        */
    }
    public override void Heuristic(float[] actionsOut)
    {
        var action = 0;
        if (Input.GetKey(KeyCode.RightArrow)) action = 1;
        else if (Input.GetKey(KeyCode.LeftArrow)) action = 2;
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
    readonly Vector3 Zero = Vector3.zero;
    readonly Vector3 Right = Vector3.right;
    readonly Vector3 Left = Vector3.left;
}

