using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GenerateMesh : MonoBehaviour
{
    public GameObject meshPrefab;
    public GameObject meshPrefabPolygon;
    public Transform meshContainer;

    public void DoExtrusionMesh(Vector3[] toExtrude, in Vector3[] extruder, bool closedCurve)
    {
        Vector3[] vertices;

        if(closedCurve == true)
        {
            Array.Resize(ref toExtrude, toExtrude.Length + 1);

            toExtrude[toExtrude.Length-1] = toExtrude[0];

            vertices = new Vector3[toExtrude.Length * (extruder.Length + 2)];
        }else{
            vertices = new Vector3[toExtrude.Length * extruder.Length];
        }
        
        int[] indices;
        Vector3[] normals;

        ExtrudeVertices(in toExtrude, extruder, ref vertices);

        GenerateExtrusionIndices(toExtrude.Length, extruder.Length, out indices);

        GameObject meshObject;
        if(closedCurve == false)
        {
            meshObject = Instantiate(meshPrefab, meshContainer);
        }else{
            meshObject = Instantiate(meshPrefabPolygon, meshContainer);
        }

        meshObject.transform.position = -toExtrude[0];

        Mesh mesh = new Mesh();
        int verticesLength;

        if(closedCurve == true)
        {
            int[] indicesLid;

            if(FillVoid(toExtrude.Length, extruder.Length, ref vertices, out indicesLid) == true)
            {
                verticesLength = vertices.Length;

                DoubleFaceVertices(ref vertices);
                DoubleFaceIndices(verticesLength, ref indicesLid);
                mesh.vertices = vertices;
                mesh.subMeshCount = 2;
                mesh.SetIndices(indicesLid, MeshTopology.Triangles, 1);
            }else{
                verticesLength = vertices.Length;

                DoubleFaceVertices(ref vertices);
                mesh.vertices = vertices;
            }
        }else{
            verticesLength = vertices.Length;

            DoubleFaceVertices(ref vertices);
            mesh.vertices = vertices;
        }

        DoubleFaceIndices(verticesLength, ref indices);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        normals = new Vector3[vertices.Length];

        for(int i  = 0; i < vertices.Length; i++)
        {
            normals[i] = new Vector3(0, 0, 1);
        }

        mesh.RecalculateNormals();

        meshObject.GetComponent<MeshFilter>().mesh = mesh;

        SetupCollidersAndRigidBodys(ref meshObject, ref mesh);
    }

    public void DoRevolutionMesh(Vector3[] toExtrude, Vector3 point, Vector3 axis)
    {
        Vector3[] vertices;

        vertices = new Vector3[toExtrude.Length * 30];
        
        int[] indices;
        Vector3[] normals;

        RevolutionVertices(in toExtrude, ref vertices, point, axis);

        GenerateRevolutionIndices(toExtrude.Length, out indices);

        GameObject meshObject;

        meshObject = Instantiate(meshPrefab, meshContainer);

        meshObject.transform.position = Vector3.zero;//-toExtrude[0];

        Mesh mesh = new Mesh();

        int verticesLength = vertices.Length;
        DoubleFaceVertices(ref vertices);
        mesh.vertices = vertices;

        DoubleFaceIndices(verticesLength, ref indices);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        normals = new Vector3[vertices.Length];

        for(int i  = 0; i < vertices.Length; i++)
        {
            normals[i] = new Vector3(0, 0, 1);
        }

        mesh.RecalculateNormals();

        meshObject.GetComponent<MeshFilter>().mesh = mesh;

        SetupCollidersAndRigidBodys(ref meshObject, ref mesh);
    }

    private void RevolutionVertices(in Vector3[] toExtrude, ref Vector3[] vertices, Vector3 point, Vector3 axis)
    {
        for(int cpt = 0; cpt < 30; cpt++)
        {
            var rot = Matrix4x4.zero;

            float q0 = Mathf.Cos(Mathf.PI*2/30*cpt/2);
            float q1 = Mathf.Sin(Mathf.PI*2/30*cpt/2) * axis.x;
            float q2 = Mathf.Sin(Mathf.PI*2/30*cpt/2) * axis.y;
            float q3 = Mathf.Sin(Mathf.PI*2/30*cpt/2) * axis.z;

            rot[0, 0] = q0*q0 + q1*q1 - q2*q2 - q3*q3;
            rot[0, 1] = 2*(q1*q2 - q0*q3);
            rot[0, 2] = 2*(q1*q3 - q0*q2);

            rot[1, 0] = 2*(q2*q1 - q0*q3);
            rot[1, 1] = q0*q0 - q1*q1 + q2*q2 - q3*q3;
            rot[1, 2] = 2*(q2*q3 - q0*q1);

            rot[2, 0] = 2*(q3*q1 - q0*q2);
            rot[2, 1] = 2*(q3*q2 - q0*q1);
            rot[2, 2] = q0*q0 - q1*q1 - q2*q2 + q3*q3;

            for(int i = 0; i < toExtrude.Length; i++)
            {
                vertices[cpt * toExtrude.Length + i] = (Vector3)(rot * (toExtrude[i] - point)) + point;
            }
        }
    }

    private void ExtrudeVertices(in Vector3[] toExtrude, Vector3[] extruder, ref Vector3[] vertices)
    {
        //vertices = new Vector3[toExtrude.Length * extruder.Length];

        Vector3 vecA =  extruder[1] - extruder[0];

        int extruderLength;

        extruderLength = extruder.Length;

        for(int i = 0; i < toExtrude.Length; i++)
        {
            for(int cpt = 0; cpt < extruder.Length; cpt++)
            {   
                if(cpt <= 1)
                {
                    vertices[i * extruderLength + cpt] = toExtrude[i] + extruder[cpt];
                }else{
                    Vector3 vecB =  extruder[cpt] - extruder[0];

                    float angleX = -Mathf.Acos((vecA[1] * vecB[1] + vecA[2] * vecB[2]) / (Mathf.Sqrt(vecA[1]*vecA[1] + vecA[2]*vecA[2]) * Mathf.Sqrt(vecB[1]*vecB[1] + vecB[2]*vecB[2])));
                    float angleY = -Mathf.Acos((vecA[0] * vecB[0] + vecA[2] * vecB[2]) / (Mathf.Sqrt(vecA[0]*vecA[0] + vecA[2]*vecA[2]) * Mathf.Sqrt(vecB[0]*vecB[0] + vecB[2]*vecB[2])));
                    float angleZ = -Mathf.Acos((vecA[0] * vecB[0] + vecA[1] * vecB[1]) / (Mathf.Sqrt(vecA[0]*vecA[0] + vecA[1]*vecA[1]) * Mathf.Sqrt(vecB[0]*vecB[0] + vecB[1]*vecB[1])));

                    var rotX = Matrix4x4.identity;
                    rotX[1,1] = Mathf.Cos(angleX);
                    rotX[1,2] = -Mathf.Sin(angleX);
                    rotX[2,1] = Mathf.Sin(angleX);
                    rotX[2,2] = Mathf.Cos(angleX);

                    var rotY = Matrix4x4.identity;
                    rotY[0,0] = Mathf.Cos(angleY);
                    rotY[0,2] = Mathf.Sin(angleY);
                    rotY[2,0] = -Mathf.Sin(angleY);
                    rotY[2,2] = Mathf.Cos(angleY);

                    var rotZ = Matrix4x4.identity;
                    rotZ[0,0] = Mathf.Cos(angleZ);
                    rotZ[0,1] = -Mathf.Sin(angleZ);
                    rotZ[1,0] = Mathf.Sin(angleZ);
                    rotZ[1,1] = Mathf.Cos(angleZ);

                    vertices[i * extruderLength + cpt] = (Vector3)(rotX * rotY * rotZ * (toExtrude[i]-toExtrude[0])) + toExtrude[0] + extruder[cpt];
                }
                
            }
        }
    }

    private void GenerateRevolutionIndices(int toExtrudeLength, out int[] indices)
    {
        int index = 0;

        indices = new int[toExtrudeLength * (30-1) * 6];        

        for(int i = 0; i < toExtrudeLength; i++)
        {
            for(int cpt  = 0; cpt < 29; cpt++)
            {
                int A;
                int B;
                int C;
                int D;

                A = cpt + 30 * i;
                B = cpt + 1 + 30 * i;

                if(i+1 == toExtrudeLength)
                {
                    C = cpt + 1;
                    D = cpt;
                }else{
                    C = cpt + 1 + 30 * (i + 1);
                    D = cpt + 30 * (i + 1);
                }

                indices[index] = A;
                index++;

                indices[index] = B;
                index++;

                indices[index] = C;
                index++;

                indices[index] = C;
                index++;

                indices[index] = D;
                index++;

                indices[index] = A;
                index++;
            }
        }        
    }

    private void GenerateExtrusionIndices(int toExtrudeLength, int extruderLength, out int[] indices)
    {
        int index = 0;
        int n;

        n = toExtrudeLength-1;

        indices = new int[n * (extruderLength-1) * 6];
        

        for(int i = 0; i < n; i++)
        {
            for(int cpt  = 0; cpt < extruderLength-1; cpt++)
            {
                int A = cpt + extruderLength * i;
                int B = cpt + 1 + extruderLength * i;
                int C = cpt + 1 + extruderLength * (i + 1);
                int D = cpt + extruderLength * (i + 1);

                indices[index] = A;
                index++;

                indices[index] = B;
                index++;

                indices[index] = C;
                index++;

                indices[index] = C;
                index++;

                indices[index] = D;
                index++;

                indices[index] = A;
                index++;
            }
        }        

        Debug.Log(n * (extruderLength-1) * 6 + " " + index);
    }

    private bool FillVoid(int toExtrudeLength, int extruderLength, ref Vector3[] vertices, out int[] indices)
    {
        LinkedList<Vector3> face = new LinkedList<Vector3>();
        Dictionary<Vector3, int> hashTable  = new Dictionary<Vector3, int>();
        Vector3[] trianglesFaceA;
        Vector3[] trianglesFaceB;

        for(int i = 0; i < toExtrudeLength-1; i++)
        {
            vertices[toExtrudeLength * extruderLength + i] = vertices[extruderLength * i];
            hashTable.Add(vertices[extruderLength * i], toExtrudeLength * extruderLength + i);
            face.AddLast(vertices[extruderLength * i]);
        }

        JoinPointsAlongPlane(face, out trianglesFaceA);

        face.Clear();

        for(int i = 0; i < toExtrudeLength-1; i++)
        {
            vertices[toExtrudeLength * (extruderLength + 1) + i] = vertices[extruderLength-1 + extruderLength * i];
            hashTable.Add(vertices[toExtrudeLength * (extruderLength + 1) + i], toExtrudeLength * (extruderLength + 1) + i);
            face.AddLast(vertices[toExtrudeLength * (extruderLength + 1) + i]);
        }

        JoinPointsAlongPlane(face, out trianglesFaceB);

        if((trianglesFaceA.Length + trianglesFaceB.Length) % 3 == 0)
        {
            indices = new int[trianglesFaceA.Length + trianglesFaceB.Length];
            //indices = new int[trianglesFaceA.Length];

            for(int i = 0; i < trianglesFaceA.Length; i++)
            {
                indices[i] = hashTable[trianglesFaceA[i]];
            }

            for(int i = 0; i < trianglesFaceB.Length; i++)
            {
                indices[i+trianglesFaceA.Length] = hashTable[trianglesFaceB[i]];
            }
        
            return true;
        }else{
            indices = new int[0];

            return false;
        }
    }

    /// Join the points along the plane to the halfway point
    private void JoinPointsAlongPlane(in LinkedList<Vector3> face, out Vector3[] triangles)
    {
        //Get the 2d Vectors along the plane sorted in a clockwise polygon point list
        //And store the indexes of there 3d equivalent in a map
        Dictionary<Vector2, Vector3> vertexMap;

        LinkedList<Vector2> polygon;
        ProjectsOnPlane(in face, out polygon, out vertexMap);

        //Decompose the polygon into y-monotones polygons
        //O(n*Log(n))
        LinkedList<LinkedList<Vector2>> monotonePolygons;
        PolygonTriangulation.DecomposeToMonotone(polygon, out monotonePolygons);

        //triangulate the mononotone polygon 
        //O(n)
        LinkedList<Vector2> trianglesVec2 = new LinkedList<Vector2>();
        LinkedListNode<LinkedList<Vector2>> monotonePolygonsListNode;

        for(monotonePolygonsListNode = monotonePolygons.First; monotonePolygonsListNode != null; monotonePolygonsListNode = monotonePolygonsListNode.Next)
        {
            PolygonTriangulation.TriangulateMonotonePolygon(monotonePolygonsListNode.Value, ref trianglesVec2);
        }

        //Turn the 2d triangles into 3d triangles
        triangles = new Vector3[trianglesVec2.Count];
        LinkedListNode<Vector2> trianglesVec2Node;
        int cpt = 0;

        for(trianglesVec2Node = trianglesVec2.First; trianglesVec2Node != null; trianglesVec2Node = trianglesVec2Node.Next, cpt++)
        {
            triangles[cpt] = vertexMap[trianglesVec2Node.Value];
        }
    }

    //Creates a non crossing polygon from the points along the plane
    private void ProjectsOnPlane(in LinkedList<Vector3> face, out LinkedList<Vector2> polygon, out Dictionary<Vector2, Vector3> vertexMap)
    {
        if(face.Count < 3)
        {
            polygon = new LinkedList<Vector2>();
            vertexMap = new Dictionary<Vector2, Vector3>();

            return;
        }

        //Projects the 3d vectors on the 2d plane
        polygon = new LinkedList<Vector2>();
        Vector3 xAxis = Vector3.one;
        Vector3 yAxis = Vector3.one;
        Vector3 planeOrigin = Vector3.zero;//Any point on the plane will do.
        Vector3 zAxis = new Plane(face.First.Value, face.First.Next.Value, face.First.Next.Next.Value).normal;
        
        Vector3.OrthoNormalize(ref zAxis, ref xAxis, ref yAxis);

        vertexMap = new Dictionary<Vector2, Vector3>();

        LinkedListNode<Vector3> faceNode;
        Vector3 pointZero = face.First.Value;

        for(faceNode = face.First; faceNode != null; faceNode = faceNode.Next)
        {
            Vector3 planePos = faceNode.Value - pointZero;
            Vector2 vec2 = new Vector2(Vector3.Dot(planePos, xAxis), Vector3.Dot(planePos, yAxis));

            //If there's a point duplicate ignore it and everything will be alright, right ?
            //Probably happens because of the doublesided face, should be removed in the future
            if(vertexMap.ContainsKey(vec2) == false)
            {
                polygon.AddLast(vec2);
                vertexMap.Add(vec2, faceNode.Value);
            }
        }
    }

    /// Add mesh collider and rigid body to game object
    private void SetupCollidersAndRigidBodys(ref GameObject gameObject, ref Mesh mesh)
    {      
        //try{          
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = false;
            //Already in the defaults ?
            /*meshCollider.cookingOptions |= MeshColliderCookingOptions.WeldColocatedVertices;
            meshCollider.cookingOptions |= MeshColliderCookingOptions.EnableMeshCleaning;*/
            
        /*}catch
        {
            Debug.Log("Mesh was probably topologically challenging for PhysX.");
        }*/
    }

    public void DoubleFaceVertices(ref Vector3[] vertices)
    {
        int verticesLength = vertices.Length;

        Array.Resize(ref vertices, vertices.Length*2);
        Array.Copy(vertices, 0, vertices, verticesLength, verticesLength);
    }

    public void DoubleFaceIndices(int verticesLength, ref int[] indices)
    {
        int n = 3;

        if((indices.Length % n) != 0)
        {
            return;
        }

        int indicesLength = indices.Length;

        Array.Resize(ref indices, indices.Length*2);

        for(int i = 0; i < indicesLength; i += n)
        {
            for(int cpt = 0; cpt < n; cpt++)
            {
                indices[indicesLength+i+cpt] = indices[i+n-1-cpt] + verticesLength;
            }
        }
    }
}
