using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FG;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Matrix matrix = new Matrix
        (8,3,
        6.12f,5.554f,4.454545f,
        3.145f,2,1.5445f
        );
        Debug.Log(matrix);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
