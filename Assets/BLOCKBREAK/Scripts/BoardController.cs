using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class BoardController : MonoBehaviour
{
    Rigidbody rigid;
    public Vector3 Velocity => rigid.velocity;
    [HideInInspector] public float maxAccelation = 0f;
    private float Accel { get { return maxAccelation * Time.deltaTime; } }
    
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }
    public void AddForce(float force) //x = vt + Ft^2
    {
        var dt = Time.deltaTime;
        var v = rigid.velocity.x;
        var np = v * dt + dt + force * maxAccelation * dt;
        rigid.velocity = new Vector3(np, 0, 0);
    }

}