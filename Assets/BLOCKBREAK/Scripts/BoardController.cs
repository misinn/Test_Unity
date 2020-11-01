using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class BoardController : MonoBehaviour
{
    Rigidbody rigid;
    public Vector3 Velocity => rigid.velocity;
    [HideInInspector]public float speed = 0f;
    private float Speed { get { return speed * deltatime; } }
    private readonly float deltatime = 0.015f;
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }
    public void AddForce(float force)
    {
        rigid.AddForce(new Vector3(force,0,0)*Speed, ForceMode.VelocityChange);
    }

}
