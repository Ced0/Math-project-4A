using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectScript : MonoBehaviour
{
    public Camera camera;
    private Transform selectedTransform = null;
    public GameObject gizmos;

    private string gizmoType = "";
    private Vector3 gizmoOffset;
    private Vector3 gizmoAxis;
    private Vector3 gizmoDir;

    //Moving object variables
    private Vector3 mouseOffset;
    private float mouseZCoord;

    private Point selectedPointScript;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(selectedPointScript != null) selectedPointScript.selected = false;

            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            
            bool gizmoHit = false;
            int gizmoMask = 1 << 3;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, gizmoMask)) {
                string[] nameSplit = hit.transform.name.Split('_');

                //Check name to find out which gizmo we've hit.
                if(nameSplit[0] != "Sphere")
                {
                    gizmoType = nameSplit[0];
                    gizmoHit = true;
                }else{
                    gizmoType = "";
                }

                switch(nameSplit[0])
                {
                    case "Move": 
                        mouseZCoord = Camera.main.WorldToScreenPoint(selectedTransform.position).z;
                        gizmoOffset = (GetMouseAsWorldPoint() + mouseOffset);

                        switch(nameSplit[1])
                        {
                            case "X":
                                gizmoDir = selectedTransform.right;
                                gizmoAxis = new Vector3(1, 0, 0);
                                break;

                            case "Y":
                                gizmoDir = selectedTransform.up;
                                gizmoAxis = new Vector3(0, 1, 0);
                                break;

                            case "Z":
                                gizmoDir = selectedTransform.forward;
                                gizmoAxis = new Vector3(0, 0, 1);
                                break;
                        }
                        break;

                    case "Rotate":
                        break;

                    case "Scale":
                        break;
                }
                
                // Do something with the object that was hit by the raycast.
            }
            
            if(gizmoHit == false)
            {
                gizmoType = "";
                int objectsMask = (1 << 6) + (1 << 7);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, objectsMask)) {
                    selectedTransform = hit.transform;

                    gizmos.SetActive(true);
                    gizmos.transform.position = selectedTransform.position;
                    gizmos.transform.rotation = selectedTransform.rotation;
                
                    mouseZCoord = Camera.main.WorldToScreenPoint(selectedTransform.position).z;

                    // Store offset = gameobject world pos - mouse world pos
                    mouseOffset = selectedTransform.position - GetMouseAsWorldPoint();

                    if(selectedTransform.gameObject.tag != "Mesh")
                    {
                        selectedPointScript = selectedTransform.gameObject.GetComponent<Point>();
                        selectedPointScript.selected = true;
                    }
                }else{
                    gizmos.SetActive(false);
                    selectedTransform = null;
                    selectedPointScript = null;
                }
            }
        }else if (Input.GetMouseButton(0) && selectedTransform != null)
        {
            switch(gizmoType)
            {
                case "":
                    selectedTransform.position = GetMouseAsWorldPoint() + mouseOffset;
                    break;

                case "Move":
                    Vector3 tmp = ((GetMouseAsWorldPoint() + mouseOffset) - gizmoOffset);
                    tmp = (new Vector3(tmp.x * gizmoAxis.x, tmp.y * gizmoAxis.y, tmp.z * gizmoAxis.z));
                    selectedTransform.position += gizmoDir * (tmp.x + tmp.z + tmp.y);
                    gizmoOffset = (GetMouseAsWorldPoint() + mouseOffset);
                    break;
            }

            gizmos.transform.position = selectedTransform.position;
            
        }
    }

    private Vector3 GetMouseAsWorldPoint()
    {
        // Pixel coordinates of mouse (x,y)
        Vector3 mousePoint = Input.mousePosition;

        // z coordinate of game object on screen
        mousePoint.z = mouseZCoord;

        // Convert it to world points
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}
