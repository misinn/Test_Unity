using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class BoardController : MonoBehaviour
{
    Rigidbody rigid;
    public Vector3 Velocity => new Vector3(velocity,0,0);
    float velocity = 0f;
    [HideInInspector]public float friction = 0f;
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }
    public void Init()
    {
        velocity = 0f;
        lasttime = Time.time;
    }
    
    public void Move(float force,float accel,float maxspeed)
    {
        var dt = Time.time-lasttime;
        lasttime = Time.time;
        var v = velocity;
        var a = force * accel;
        velocity = v * (1 - friction * dt * 25f) + a * dt;
        if (Mathf.Abs(velocity) > maxspeed)
            velocity = velocity / Mathf.Abs(velocity) * maxspeed;
        rigid.velocity = new Vector3(velocity, 0, 0);
    }
    float lasttime = 0f;
    float deltatime = 0.004f;
    private void TimerUpdate()
    {
        deltatime = Time.time - lasttime;
        lasttime = Time.time;
        Debug.Log(deltatime + "board");
    }
}