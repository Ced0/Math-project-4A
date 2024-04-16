using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour
{
    public TextMesh textMesh;
    public float weight = 1;
    public bool selected = false;

    void Start()
    {
        textMesh.text = weight.ToString();
    }

    void Update()
    {   
        if(selected == true)
        {
            float old = weight;

            //Increase/decrease number of vertices of the curve
            if (Input.GetKeyDown(KeyCode.P))
            {
                if(weight < 1)
                {
                    weight *= 2;
                }else{
                    weight++;
                }
                
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                if(weight <= 1)
                {
                    weight /= 2;
                }else{
                    weight--;
                }
            }



            if(old != weight)
            {
                textMesh.text = weight.ToString();
            }
        }
    }
}