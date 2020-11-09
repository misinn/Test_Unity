using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using static UnityEngine.Mathf;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    private GameManager gamemanager;
    private Rigidbody rigid;
    public float defaltspeed;
    public float speed { get; private set; }
    public float accelOnHit;
    public float rotateDegreesOnBoard;
    public Vector3 velocity;
    private bool ArrowUpdate;
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
        lasttime = Time.time-0.001f;
    }
    public void SetUp(Vector3 pos)
    {
        this.transform.localPosition = pos;
        speed = defaltspeed;
        velocity = new Vector3();
    }
    public void GameStart()
    {
        ArrowUpdate = true;
    }
    private void FixedUpdate()
    {
        TimerUpdate();
        if (Skip || !ArrowUpdate)
        {
            return;
        }
        VelocityCheck();
        this.velocity.z -= 0.01f*deltatime;
        velocity = this.velocity / GetxzMag(this.velocity) * speed;
        rigid.velocity = velocity;
        
    }
    public void GameEnd()
    {
        ArrowUpdate = false;
        rigid.velocity = new Vector3();
    }
    private void OnCollisionEnter(Collision collision)
    {
        var hitGameObj = collision.gameObject;
        var normal = collision.contacts[0].normal;
        if (!hitGameObj.CompareTag("board") &&!CanHit(normal, velocity)) return;
        var nvec = Vector3.Project(velocity, normal);
        var pvec = velocity - nvec;
        var bound = pvec - nvec;
        velocity = bound;
        speed += accelOnHit;
        velocity.y = 0;

        //TODO 巨大球のときに揺らす
        //Camera.main.transform.DOShakePosition(0.5f, 0.2f).OnComplete(() => Camera.main.transform.DOMove(new Vector3(0, 45, -0.3f), 0.1f));
        if (hitGameObj.CompareTag("board"))
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
    
    
    //Bottom
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Bottom")
        {
            gamemanager.OnBallDroped();
        }
        else if (other.gameObject.CompareTag("block")) //機械学習の遺産
        {
            gamemanager.OnBallHitVirtualBlock(other.GetComponent<Block>());
        }
    }
    private float GetxzMag(Vector3 vec)
    {
        float x = vec.x, z = vec.z;
        float ans = x * x + z * z;
        return ans > 0 ? Sqrt(ans) : 0.001f;
    }
    private bool CanHit(Vector3 vec1, Vector3 vec2)
    {
        var dot = Vector3.Dot(vec1, vec2);
        return dot < 0;
    }
    int exceptionleng = 0;
    private void VelocityCheck()
    {
        var rvec = rigid.velocity;
        var vec = velocity;
        //異常が起こっているとき、velocityを反転
        if (GetxzMag(rvec) < GetxzMag(vec) / 10f) exceptionleng++;
        else exceptionleng = 0;
        if (exceptionleng > 4)
        {
            velocity = -velocity;
            velocity.z += speed / 3;
            exceptionleng = 0;
            Debug.Log("Exception_Ball");
        }
    }
    float lasttime = 0f;
    float deltatime = 0.004f;
    private void TimerUpdate()
    {
        deltatime = Time.time - lasttime;
        lasttime = Time.time;
    }
}