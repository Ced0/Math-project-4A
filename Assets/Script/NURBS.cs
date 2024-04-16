using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NURBS : MonoBehaviour
{
    public int nPoints;
    public Vector3[] positions;
    private float[] weight;
    public MeshFilter meshFilter;
    public bool selected = true;
    public string userKnotsString = "";
    private float[] userKnots;
    private bool validKnots = false;
    public int _degree = 3;
    public bool repeatLastPoint = false;

    // Start is called before the first frame update
    void Start()
    {
        userKnots = new float[0];
        SetUserKnots();

        positions = new Vector3[nPoints];

        DrawCurve();
    }

    public bool SetUserKnots()
    {
        string[] subs = userKnotsString.Split(',');

        if(subs.Length == 1)
        {
            validKnots = false;
            return false;
        }

        userKnots = new float[subs.Length];

        for(int i = 0; i < subs.Length; i++)
        {
            

            if(float.TryParse(subs[i], out userKnots[i]) == true && i != 0 && userKnots[i-1] > userKnots[i])
            {
                validKnots = false;
                return false;
            }
        }
        
        Debug.Log("hey");
        validKnots = true;
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if(selected == true)
        {
            //Increase/decrease number of vertices of the curve
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                nPoints++;
            }

            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                nPoints--;
            }

            if(nPoints < 2)
            {
                nPoints = 2;
            }

            positions = new Vector3[nPoints];

            DrawCurve();
        }
    }

    protected void DrawCurve()
    {
        //Knots length: number of points + degree + 1
        //float[] knots = new float[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
        //float[] knots = new float[]{0, 0, 0, 1, 2, 2, 2};
        //float[] knots = new float[]{0, 0, 0, 0, 1.25f, 1.75f, 3, 3, 3, 3};
        //float[] knots = new float[]{0, 0, 0, 0, 1, 2, 3, 3, 3, 3};
        //float[] knots = new float[]{0, 0, 0, 1/4, 1/4, 1/2, 1/2, 3/4, 3/4, 1, 1, 1};
        //float[] knots = new float[]{0, 1, 2, 3, 4, 5, 6};
        

        int pointCount = transform.GetChild(0).childCount;
        
        if(repeatLastPoint == true)
        {
            pointCount++;
        }
        
        float[] knots;

        if((userKnots.Length == (pointCount + _degree + 1)) && validKnots == true)
        {
            knots = userKnots;
        }else{
            knots = new float[pointCount + _degree + 1];
            int knotCount = 0;

            for(int i = 0; i < knots.Length; i++)
            {
                if(i <= _degree)
                {
                    knots[i] = 0;
                }else if(i < knots.Length - _degree)
                {
                    knotCount++;
                    knots[i] = knotCount;
                }else{
                    knots[i] = knots[i-1];
                }
            }
        }

        List<Vector4> controlPoints = new List<Vector4>();
        //int[] intControlPoints = new int[]{-1,  2, 0, -1};

        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            float weight = transform.GetChild(0).GetChild(i).GetComponent<Point>().weight;

            controlPoints.Add(transform.GetChild(0).GetChild(i).position * weight);

            controlPoints[controlPoints.Count-1] += new Vector4(0, 0, 0, weight);
        }

        if(repeatLastPoint == true && transform.GetChild(0).childCount > 0)
        {
            float weight = transform.GetChild(0).GetChild(0).GetComponent<Point>().weight;

            controlPoints.Add(transform.GetChild(0).GetChild(0).position * weight);

            controlPoints[controlPoints.Count-1] += new Vector4(0, 0, 0, weight);
        }

        float low = knots[_degree];
        float high = knots[knots.Length-1 - _degree];
        float step = (high - low)/(float)(nPoints-1);
        int arrCounter = 0;


        for(float n = 0; arrCounter < nPoints; n+=step)
        {
            float t = n + low;

            int segment = ((_degree + (int)n) >= controlPoints.Count ? (_degree + (int)n) -1 : (_degree + (int)n));

            while(t >= high)
            {
                t = t - 0.000001f;
            }

            positions[arrCounter] = interpolate(t, knots, controlPoints, _degree);

            arrCounter++;
        }

        /*if((arrCounter) == (nPoints))
        {
            positions[nPoints-1] = positions[nPoints-2];
        }*/

        Mesh mesh = new Mesh();
        mesh.vertices = positions;

        int[] indices = new int[nPoints];

        for(int i = 0; i < nPoints; i++) indices[i] = i;

        mesh.SetIndices(indices, MeshTopology.LineStrip, 0);

        meshFilter.mesh = mesh;
    }

    protected Vector3 interpolate(float t, float[] knots, List<Vector4> points, int degree)
    {
        int n = knots.Length - degree - 1;

        if(n < degree+1 || points.Count < n)
        {
            Debug.Log("Basis Spline error");
            return Vector3.zero;
        }
        
        Vector4 sum = Vector4.zero;

        for(int i = 0; i < n; i++)
        {
            sum += points[i] * Basis(t, degree, i, knots);
        }

        return ((Vector3)sum)/sum.w;
    }

    float Basis(float t, int degree, int i, float[] knots)
    {
        float c1 = 0;
        float c2 = 0;

        if(degree == 0)
        {
            return (knots[i] <= t && t < knots[i+1]) ? 1.0f : 0.0f;
        }
            
        if(knots[i+degree] != knots[i])
        {
            c1 = (t - knots[i])/(knots[i+degree] - knots[i]) * Basis(t, degree-1, i, knots);
        }
            
        if(knots[i+degree+1] != knots[i+1])
        {
            c2 = (knots[i+degree+1] - t)/(knots[i+degree+1] - knots[i+1]) * Basis(t, degree-1, i+1, knots);
        }

        return c1 + c2;
    }
}
