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
    public Operator _operator { get; set; }
    public Type _type { get; set; }
    public float defaultAccel = 5000f;
    public float defaultMaxSpeed = 3f;
    [Range(0,1)]public float friction = 0.34f;
    public Material UndamagedMat;
    public Material damagedMat;
    public Material HardlydamagedMat;
    [HideInInspector] public Vector3 Velocity;
    public Observation Observation { get; set; }
    public int MaxHealth { get; private set; }
    public int Health { get; private set; }
    Color Color;
    /// <summary>
    /// 初期化の一度のみ。
    /// </summary>
    public override void Initialize()
    {
        GameManager = GetComponentInParent<GameManager>();
        Rigidbody = GetComponent<Rigidbody>();
        Collider = GetComponent<Collider>();
        BehaviorParameters = GetComponent<BehaviorParameters>();
        Renderer = GetComponent<Renderer>();
        Observation = new Observation(9);
        Velocity = Zero;
        lasttime =  Time.time-0.004f;
        deltatime = 0.004f;
        friction = friction < 1e-5f ? 1e-5f : friction;
    }
    public void SetUp(Color color,Vector3 pos,Operator _operator,Type _type,int health = 3)
    {
        Renderer.material = UndamagedMat;
        this.Renderer.material.color = color;
        transform.localPosition = pos;
        this._operator = _operator;
        this._type = _type;
        MaxHealth = health;
        Health = health;
        Color = color;
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
    public void OnCollisionEnter(Collision collision)
    {
        var hitgameobj = collision.gameObject;
        if (hitgameobj.CompareTag("ball"))
        {
            --Health;
            if (Health <= 0)
            {
                GameManager.OnBallHitBlock(this);
            }
            else
            {
                ChangeMaterial(Health);
            }
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
        else if (other.CompareTag("bottom"))
        {
            GameManager.OnBallHitVirtualBlock(this);
        }
    }
    public void Move(Vector3 force,float accel,float maxspeed)
    {
        var dt = deltatime * 50f;
        var v = Velocity;
        var a = force * accel;
        Velocity = (v - a / friction) * Mathf.Pow(1 - friction, dt) + a / friction;
        Velocity.y = 0;
        var mag = Velocity.magnitude;
        if (mag > maxspeed)
            Velocity = Velocity / mag * maxspeed;
        Rigidbody.velocity += Velocity;
    }
    public void Stop()
    {
        Velocity = Zero;
        Rigidbody.velocity = Zero;
    }
    private void ChangeMaterial(int health)
    {
        if (health == 1 || health < MaxHealth / 3)
        {
            Renderer.material = HardlydamagedMat;
            Renderer.material.color = Color;
        }
        else if(health== 2 || health < MaxHealth * 2 / 3)
        {
            Renderer.material = damagedMat;
            Renderer.material.color = Color;
        }
        else
        {
            if(Renderer.material != UndamagedMat)
            {
                Renderer.material = UndamagedMat;
                Renderer.material.color = Color;
            }
        }
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

