using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Observation
{

    float[] ary;
    private int index;
    public int Size { get; set; }
    public Observation(int size)
    {
        ary = new float[size];
        Size = size;
        index = 0;
    }
    public void Init()
    {
        ary = new float[ary.Length];
        index = 0;
    }
    public void AddObservation(float obs)
    {
        if (index == ary.Length) new System.Exception("指定したサイズ以上の観察を取得しました。");
        ary[index++] = obs;
    }
    public void AddObservation(params float[] inputs)
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            AddObservation(inputs[i]);
        }
    }
    public static implicit operator float[](Observation obs)
    {
        return obs.ary;
    }


}
