using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarnsleyFern : MonoBehaviour
{
    public GameObject prefab;
    readonly private List<GameObject> instances = new ();
    void Start()
    {
        // generate 1000 cubes
        for (int i = 0; i < 1000; i++)
        {
            // instantiate a cube
            GameObject cube = Instantiate(prefab);
            instances.Add(cube);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
