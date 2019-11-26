using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FG;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector v2_a = new Vector(30,1);
        Vector v2_b = new Vector(-30,-1);
        Debug.Log(Vector.Dot(v2_a.Normalize,v2_b.Normalize));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
