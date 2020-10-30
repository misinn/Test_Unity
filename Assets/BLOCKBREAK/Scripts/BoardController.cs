using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class BoardController : MonoBehaviour
{
    Rigidbody rigid;
    public Vector3 Velocity => rigid.velocity;
    [SerializeField] float speed = 0f;
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }
    public void AddForce(Vector3 force)
    {
        var speed = this.speed*Time.deltaTime;
        rigid.AddForce(force*speed, ForceMode.VelocityChange);
    }

}
