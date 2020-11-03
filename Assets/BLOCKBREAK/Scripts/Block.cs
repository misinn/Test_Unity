using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Block : Agent
{
    private GameManager gamemanager;
    private Rigidbody rigid;
    private Collider collid;
    public bool enablemove;
    public bool IsPlayer { get; private set; }
    public float defaultAccel = 5000f;
    public float defaultMaxSpeed = 3f;
    [Range(0,1)]public float friction = 0.34f;
    public Vector3 Velocity { get { return velocity; } }
    Vector3 velocity = Zero;
    
    public Observation Observation;
    public override void Initialize()
    {
        if (alreadyInitalized) return;
        gamemanager = GetComponentInParent<GameManager>();
        rigid = GetComponent<Rigidbody>();
        collid = GetComponent<Collider>();
        Observation = new Observation(8);
        alreadyInitalized = true;
        IsPlayer = this.gameObject.name == "PlayerBlockAgent";
        velocity = Zero;
        lasttime =  Time.time-0.004f;
        deltatime = 0.004f;
        friction = friction < 1e-5f ? 1e-5f : friction;
    }
    private bool alreadyInitalized = false;
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
        rigid.velocity = Zero;
        Move(force, defaultAccel, defaultMaxSpeed);
        lastaction = (int)action;
    }
    public override void Heuristic(float[] actionsOut)
    {
        var action = 0;
        if (enablemove)
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
    public void SetTrigger(bool b)
    {
        collid.isTrigger = b;
    }
    public bool IsActive
    {
        get { return this.gameObject.activeSelf; }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("bottom"))
        {
            gamemanager.OnBallHitVirtualBlock(this);
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
            gamemanager.OnBlockStayBlockTrigger(this);
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
        rigid.velocity += velocity;
    }
    float lasttime = 0f;
    float deltatime = 0.004f;
    private void TimerUpdate()
    {
        deltatime = Time.time - lasttime;
        lasttime = Time.time;
    }
    [HideInInspector]public int moveDir { get; set; }
    static Vector3 Zero = Vector3.zero;
    static Vector3 Right = Vector3.right;
    static Vector3 Left = Vector3.left;
    static Vector3 Up = Vector3.forward;
    static Vector3 Down = Vector3.back;
}
