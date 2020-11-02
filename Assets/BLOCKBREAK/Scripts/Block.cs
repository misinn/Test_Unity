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
    public float speed;
    public bool enablemove;
    public Vector3 Velocity => rigid.velocity;

    public Observation Observation;
    public override void Initialize()
    {
        if (alreadyInitalized) return;
        gamemanager = GetComponentInParent<GameManager>();
        rigid = GetComponent<Rigidbody>();
        collid = GetComponent<Collider>();
        Observation = new Observation(8);
        alreadyInitalized = true;
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
        var action = vectorAction[0];
        var speed = this.speed*Time.deltaTime;
        var pos = Zero;
        if (action == 1) pos = Right * speed;
        else if (action == 2) pos = Left * speed;
        else if (action == 3) pos = Up * speed;
        else if (action == 4) pos = Down * speed;
        lastaction = (int)action;
        rigid.AddForce(pos, ForceMode.VelocityChange);
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
            rigid.AddForce(Up * speed * 20 * Time.deltaTime, ForceMode.VelocityChange);
        }
    }
    [HideInInspector]public int moveDir { get; set; }
    Vector3 Zero = Vector3.zero;
    Vector3 Right = Vector3.right;
    Vector3 Left = Vector3.left;
    Vector3 Up = Vector3.forward;
    Vector3 Down = Vector3.back;
}
