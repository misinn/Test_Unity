﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    private GameManager gamemanager;
    private Rigidbody rigid;
    [SerializeField] private float defaltspeed;
    public float DefaultSpeed { get => defaltspeed; private set { defaltspeed = value; } }
    [HideInInspector] public float speed;
    public float accelOnHit;
    [HideInInspector] public Vector3 velocity;
    public bool Skip
    {
        get
        {
            if (skip)
            {
                skip = false;
                return true;
            }
            return false;
        }
        set { skip = value; }
    }
    private bool skip;

    private void Start()
    {
        gamemanager = GetComponentInParent<GameManager>();
        rigid = GetComponent<Rigidbody>();
        velocity = new Vector3(0, 0, -1f);
    }
    private void FixedUpdate()
    {
        if (Skip)
        {
            return;
        }
        this.velocity.z -= 0.0001f;
        var velocity = this.velocity / GetxzMag(this.velocity) * speed * Time.fixedDeltaTime;
        rigid.velocity = velocity;
    }
    private void OnCollisionEnter(Collision collision)
    {
        var hitGameObj = collision.gameObject;
        var normal = collision.contacts[0].normal;
        var nvec = Vector3.Project(velocity, normal);
        var pvec = velocity - nvec;
        var bound = pvec - nvec;
        velocity = bound;
        speed += accelOnHit;
        velocity.y = 0;
        if (hitGameObj.CompareTag("block"))
        {
            gamemanager.OnBallHitBlock(hitGameObj.GetComponent<Block>());
        }
        else if (hitGameObj.CompareTag("board"))
        {
            var boardtrans = hitGameObj.transform;
            var hitpoint = collision.contacts[0].point.x;
            var boardpos = boardtrans.position.x;
            var scale = boardtrans.localScale.x;
            var dif = (hitpoint - boardpos) * 2f / scale;
            var rotaterad = dif * rotateDegreesOnBoard * PI / 180;
            var nextrad = PI / 2 - rotaterad;
            velocity = new Vector3(Cos(nextrad), 0, Sin(nextrad)) * GetxzMag(velocity);
            gamemanager.OnBallHitBoard();
        }
    }

    public float rotateDegreesOnBoard;
    //Bottom
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Bottom")
        {
            gamemanager.OnBallDroped();
        }
        else if (other.gameObject.CompareTag("block"))
        {
            gamemanager.OnBallHitVirtualBlock(other.GetComponent<Block>());
        }
    }
    public void BoardHit()
    {

    }

    private float GetxzMag(Vector3 vec)
    {
        float x = vec.x, z = vec.z;
        return Sqrt(x * x + z * z);
    }
}