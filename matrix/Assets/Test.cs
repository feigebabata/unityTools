using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FG;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var m1 = new Matrix
        (
            3   ,   2   ,
            1   ,   -5  ,   3   ,
            0   ,   -2  ,   6   ,
            7   ,   2   ,   -4
        );
        var m2 = new Matrix
        (
            3   ,   3   ,
            -8  ,   6   ,   1   ,
            7   ,   0   ,   -3   ,
            2   ,   4   ,   5
        );

        Debug.Log(m2*m1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
