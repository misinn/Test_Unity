using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
[RequireComponent(typeof(Rigidbody))]
public class Block : Agent
{
    GameManager GameManager;
    Rigidbody Rigidbody;
    Collider Collider;
    BehaviorParameters BehaviorParameters;
    Renderer Renderer;
    Operator _operator { get; set; }
    Type _type { get; set; }
    public bool UseModel { get; set; }
    public float defaultAccel = 5000f;
    public float defaultMaxSpeed = 3f;
    [Range(0,1)]public float friction = 0.34f;
    public Vector3 Velocity { get { return velocity; } }
    Vector3 velocity = Zero;
    bool alreadyInitalized = false;
    public Observation Observation;
    public override void Initialize()
    {
        if (alreadyInitalized) return;
        GameManager = GetComponentInParent<GameManager>();
        Rigidbody = GetComponent<Rigidbody>();
        Collider = GetComponent<Collider>();
        BehaviorParameters = GetComponent<BehaviorParameters>();
        Renderer = GetComponent<Renderer>();
        Observation = new Observation(9);
        alreadyInitalized = true;
        velocity = Zero;
        lasttime =  Time.time-0.004f;
        deltatime = 0.004f;
        friction = friction < 1e-5f ? 1e-5f : friction;
    }
    public void SetUp(Color color,Vector3 pos,Operator _operator,Type _type)
    {
        this.Renderer.material.color = color;
        transform.localPosition = pos;
        this._operator = _operator;
        this._type = _type;

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
        this.Stop();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        float[] obs = Observation;
        for (int i = 0; i < obs.Length; i++)
        {
            sensor.AddObservation(obs[i]);
        }
    }
    public int lastaction = 0;
    public override void OnActionReceived(float[] vectorAction)
    {
        
        TimerUpdate();
        var action = vectorAction[0];
        var force = Zero;
        if (action == 1) force = Right;
        else if (action == 2) force = Left;
        else if (action == 3) force = Up;
        else if (action == 4) force = Down;
        Rigidbody.velocity = Zero;
        if (_operator == Operator.None)
        {
            lastaction = 0;
            return;
        }
        Move(force, defaultAccel, defaultMaxSpeed);
        lastaction = (int)action;
    }
    public override void Heuristic(float[] actionsOut)
    {
        var action = 0;
        if (_operator == Operator.Player)
        {
            if (Input.GetKey(KeyCode.RightArrow)) action = 1;
            if (Input.GetKey(KeyCode.LeftArrow)) action = 2;
            if (Input.GetKey(KeyCode.UpArrow)) action = 3;
            if (Input.GetKey(KeyCode.DownArrow)) action = 4;
        }
        actionsOut[0] = action;
    }
    public void Enable()
    {
        this.gameObject.SetActive(true);
    }
    public void Disable()
    {
        this.gameObject.SetActive(false);
    }
    public void SetIsTrigger(bool b)
    {
        Collider.isTrigger = b;
    }
    public bool IsActive
    {
        get { return this.gameObject.activeSelf; }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("bottom"))
        {
            GameManager.OnBallHitVirtualBlock(this);
        }

    }
    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("blocklimit"))
        {
            Move(Up, defaultAccel * 10, defaultMaxSpeed *10);
        }
        else if (other.CompareTag("blocktrigger"))
        {
            GameManager.OnBlockStayBlockTrigger(this);
        }
    }
    public void Move(Vector3 force,float accel,float maxspeed)
    {
        var dt = deltatime * 50f;
        var v = velocity;
        var a = force * accel;
        velocity = (v - a / friction) * Mathf.Pow(1 - friction, dt) + a / friction;
        velocity.y = 0;
        var mag = velocity.magnitude;
        if (mag > maxspeed)
            velocity = velocity / mag * maxspeed;
        Rigidbody.velocity += velocity;
    }
    public void Stop()
    {
        velocity = Zero;
        Rigidbody.velocity = Zero;
    }

    float lasttime = 0f;
    float deltatime = 0.004f;
    private void TimerUpdate()
    {
        deltatime = Time.time - lasttime;
        lasttime = Time.time;
    }
    [HideInInspector]public int moveDir { get; set; }
    public enum Operator
    {
        Player,
        AI,
        None,
        ML
    }
    public enum Type //TODO
    {
        Normal,
        Invincible
    }
    static Vector3 Zero = Vector3.zero;
    static Vector3 Right = Vector3.right;
    static Vector3 Left = Vector3.left;
    static Vector3 Up = Vector3.forward;
    static Vector3 Down = Vector3.back;
}

