using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public GameObject curvePrefab;
    public Transform curveContainer;
    public Dropdown curveList;
    public Dropdown extrusionList;
    public GenerateMesh meshGenerator;
    private int selected = -1;
    private int extrusionSelected = -1;
    public GameObject pointPrefab;
    public Material black;
    public Material red;

    public InputField inputKnots;
    public Text inputText;

    public Transform revolutionPointA;
    public Transform revolutionPointB;

    public void Start()
    {
        curveList.onValueChanged.AddListener(delegate { CurveListItemSelected(curveList); });
        extrusionList.onValueChanged.AddListener(delegate { ExtrusionListItemSelected(curveList); });
    }

    public void Update()
    {
    }

    public void CreateExtrusionMesh(bool closedCurve)
    {
        if(curveContainer.childCount < 2) return;

        if(selected == extrusionSelected) return;

        meshGenerator.DoExtrusionMesh(curveContainer.GetChild(selected).GetComponent<NURBS>().positions,
        curveContainer.GetChild(extrusionSelected).GetComponent<NURBS>().positions, closedCurve);
    }

    public void CreateRevolutionMesh(bool closedCurve)
    {
        if(selected != -1)
        {
            Vector3 axis = (revolutionPointB.position - revolutionPointA.position).normalized;

            meshGenerator.DoRevolutionMesh(curveContainer.GetChild(selected).GetComponent<NURBS>().positions, revolutionPointA.position, axis);
        }
    }

    public void CreateNURBS()
    {
        if(selected != -1)
        {
            curveContainer.GetChild(selected).gameObject.GetComponent<NURBS>().selected = false;
        }

        GameObject newCurve = Instantiate(curvePrefab);
        newCurve.transform.parent = curveContainer;
        curveList.options.Add(new Dropdown.OptionData() { text = curveList.options.Count.ToString()});
        extrusionList.options.Add(new Dropdown.OptionData() { text = extrusionList.options.Count.ToString()});
        curveList.value = curveList.options.Count-1;

        if(extrusionList.options.Count-1 == 0)
        {
            ExtrusionSelect(0);
            extrusionSelected = 0;
        }

        CurveListItemSelected(curveList);
    }

    public void DeleteNURBS()
    {
        if(selected == -1) return;

        Destroy(curveContainer.GetChild(selected).gameObject);
        curveList.options.RemoveAt(selected);
        extrusionList.options.RemoveAt(selected);
        
        if(curveList.options.Count-1 == -1)
        {
            selected = -1;
        }

        if(selected != 0)
        {
            curveList.value = 0;

            if(extrusionSelected == selected)
            {
                extrusionSelected = 0;
                extrusionList.value = 0;
            }else if(extrusionSelected > selected){
                extrusionSelected--;
                extrusionList.value--;
            }
        }else{
            CurveSelect(1);//Destroy is not immediate

            if(extrusionSelected == selected)
            {
                ExtrusionSelect(1);
            }
        }
    }

    public void CreatePoint()
    {
        if(selected == -1) return;

        GameObject newPoint = Instantiate(pointPrefab);
        newPoint.transform.parent = curveContainer.GetChild(selected).GetChild(0);
    }

    public void DeletePoint()
    {
        if(selected == -1) return;

        Destroy(curveContainer.GetChild(selected).GetChild(0).GetChild(curveContainer.GetChild(selected).GetChild(0).childCount - 1).gameObject);
    }

    void CurveListItemSelected(Dropdown dropdown)
    {
        if(selected != -1)
        {
            curveContainer.GetChild(selected).GetChild(0).gameObject.SetActive(false);
            curveContainer.GetChild(selected).gameObject.GetComponent<NURBS>().selected = false;
        }
            
        selected = curveList.value;
        CurveSelect(selected);
    }

    void ExtrusionListItemSelected(Dropdown dropdown)
    {
        if(extrusionSelected != -1)
        {
            curveContainer.GetChild(extrusionSelected).GetComponent<MeshRenderer>().materials[0] = black;
        }

        ExtrusionSelect(extrusionList.value);
    }

    private void CurveSelect(int value)
    {
        curveContainer.GetChild(value).GetChild(0).gameObject.SetActive(true);
        curveContainer.GetChild(value).gameObject.GetComponent<NURBS>().selected = true;
    }

    private void ExtrusionSelect(int value)
    {
        if(extrusionSelected != -1) curveContainer.GetChild(extrusionSelected).GetComponent<MeshRenderer>().material = black;
        extrusionSelected = value;
        curveContainer.GetChild(value).GetComponent<MeshRenderer>().material = red;
    }

    public void SetKnots()
    {
        if(selected != -1)
        {
            NURBS nurbs = curveContainer.GetChild(selected).GetComponent<NURBS>();
            nurbs.userKnotsString = inputKnots.text;
            if(nurbs.SetUserKnots() == true)
            {
                inputText.color = Color.black;
            }else{
                inputText.color = Color.red;
            }
        }
    } 
}
