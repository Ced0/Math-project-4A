using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoScript : MonoBehaviour
{
    public GameObject moveGizmo;
    public GameObject scaleGizmo;
    public GameObject rotateGizmo;

    public GameObject sphereGizmo;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.G))
        {
            moveGizmo.SetActive(true);
            scaleGizmo.SetActive(false);
            rotateGizmo.SetActive(false);
            sphereGizmo.SetActive(true);
        }else if (Input.GetKey(KeyCode.F))
        {
            moveGizmo.SetActive(false);
            scaleGizmo.SetActive(true);
            rotateGizmo.SetActive(false);
            sphereGizmo.SetActive(true);
        }else if (Input.GetKey(KeyCode.R))
        {
            moveGizmo.SetActive(false);
            scaleGizmo.SetActive(false);
            rotateGizmo.SetActive(true);
            sphereGizmo.SetActive(false);
        }
    }
}
