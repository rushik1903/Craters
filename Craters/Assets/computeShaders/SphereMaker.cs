using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SphereMaker : MonoBehaviour
{
    public int initialBreaks;
    public Craters craters;
    public float radius = 5;
    public float testValue=1;
    public ComputeShader planetHeightShader;
    public int edgeCuts = 3;
    public GameObject camProxy;
    private MeshCollider meshCollider;
    private MeshCollider invertedMeshCollider;
    public GameObject camera;
    public GameObject prefab;
    Mesh mesh;
    float gr = 1.6180339887499f;

    Vector3[] initialVertices;

    Vector3[] vertices;
    public float[] heights;
    public List<Vector3> verticesList = new List<Vector3>();
    int[] triangles;
    public List<int> trianglesList = new List<int>();

    public List<int> breakableVertices = new List<int>();

    public List<int> wt = new List<int>();


    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        meshCollider = GetComponent<MeshCollider>();
        meshCollider.convex = true;
        GetComponent<MeshFilter>().mesh = mesh;

        //invertedMeshCollider = gameObject.AddComponent<MeshCollider>();

        CreateShapeIcosohedron();
        //CreateShapeCube();
        //CreatePlane();
        FlipNormals();
        UpdateMesh();
        DrawEdges();
        meshCollider.sharedMesh = mesh;

        //calling vertices to break initially
        for(int i = 0; i < initialBreaks; i++)
        {
            camProxy.transform.position = new Vector3(camProxy.transform.position.x, camProxy.transform.position.y, camProxy.transform.position.z + 2);
            CheckDistances();
            UpdateMesh();
        }
    }

    private void FlipNormals()
    {
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -normals[i];
        }
        mesh.normals = normals;
    }

    void CreateShapeIcosohedron()
    {
        initialVertices = new Vector3[]
        {
            //rect1
            new Vector3(1,gr,0).normalized,
            new Vector3(1,-gr,0).normalized,
            new Vector3(-1,-gr,0).normalized,
            new Vector3(-1,gr,0).normalized,
            //rect2
            new Vector3(gr,0,1).normalized,
            new Vector3(-gr,0,1).normalized,
            new Vector3(-gr,0,-1).normalized,
            new Vector3(gr,0,-1).normalized,
            //rect3
            new Vector3(0,1,gr).normalized,
            new Vector3(0,1,-gr).normalized,
            new Vector3(0,-1,-gr).normalized,
            new Vector3(0,-1,gr).normalized,
        };

        for (int i = 0; i < initialVertices.Length; i++)
        {
            initialVertices[i] = initialVertices[i] * radius;
        }

        verticesList = initialVertices.ToList();
        vertices = verticesList.ToArray();

        triangles = new int[]
        {
            0,9,3,
            0,3,8,
            0,8,4,
            0,4,7,
            0,7,9,

            1,11,2,
            1,2,10,
            1,10,7,
            1,7,4,
            1,4,11,

            2,11,5,
            2,5,6,
            2,6,10,

            3,5,8,
            3,9,6,
            3,6,5,

            4,8,11,
            8,5,11,
            9,7,10,
            9,10,6
        };
        trianglesList = triangles.ToList();
        DrawEdges();
        Vector3[] newVertices;
        int[] newTriangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {

        }
    }

    private void CreateShapeCube()
    {
        initialVertices = new Vector3[]
        {
            //rect1
            new Vector3(1,1,1),
            new Vector3(1,1,-1),
            new Vector3(1,-1,-1),
            new Vector3(1,-1,1),
            //rect2
            new Vector3(-1,1,1),
            new Vector3(-1,1,-1),
            new Vector3(-1,-1,-1),
            new Vector3(-1,-1,1),
        };
        for (int i = 0; i < initialVertices.Length; i++)
        {
            initialVertices[i] = initialVertices[i] * 5;
        }

        vertices = initialVertices;
        verticesList = vertices.ToList();

        
        triangles = new int[]
        {
            0,3,2,
            0,2,1,
            1,2,5,
            5,2,6,
            5,6,4,
            4,6,7,
            4,7,0,
            0,7,3,
            5,4,0,
            5,0,1,
            6,3,7,
            6,2,3
        };
        trianglesList = triangles.ToList();
    }

    private void CreatePlane()
    {
        initialVertices = new Vector3[]
        {
            //rect1
            new Vector3(1,1,1),
            new Vector3(1,1,-1),
            new Vector3(1,-1,-1),
        };
        for (int i = 0; i < initialVertices.Length; i++)
        {
            initialVertices[i] = initialVertices[i] * 5;
        }

        vertices = initialVertices;
        verticesList = vertices.ToList();


        triangles = new int[]
        {
            0,1,2,
        };
        trianglesList = triangles.ToList();
    }

    private void DrawEdges()
    {
        for (int i = 0; i < triangles.Length - 2; i += 3)
        {
            Debug.DrawLine(vertices[triangles[i]], vertices[triangles[i + 1]], Color.black);
            Debug.DrawLine(vertices[triangles[i + 1]], vertices[triangles[i + 2]], Color.black);
            Debug.DrawLine(vertices[triangles[i + 2]], vertices[triangles[i]], Color.black);
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
        //DebugEdgeLines();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckDistances();
            UpdateMesh();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            MakeHeights();
            UpdateMesh();
            //SpawnDebugCubes();
            Debug.Log(vertices.Length);
        }
        //MakeHeights();
        //UpdateMesh();
        //SpawnDebugCubes();

    }

    private void CheckDistances()
    {
        Vector3 camPoint = camProxy.transform.position;

        if (wt.Count == 0)
        {
            for (int i = 0; i < verticesList.Count; i++)
            {
                //int temp = (int)Mathf.Ceil(Vector3.Distance(camPoint, verticesList[i]));
                int temp = (int)Mathf.Ceil(Vector3.Distance(camPoint, gameObject.transform.position));
                wt.Add(temp);
            }
        }


        breakableVertices = new List<int>();
        for(int i = 0; i < vertices.Length; i++)
        {
            if(((int)Mathf.Ceil(Vector3.Distance(camPoint, gameObject.transform.position))) < wt[i])
            {
                wt[i] = (int)Mathf.Ceil(Vector3.Distance(camPoint, gameObject.transform.position));
                breakableVertices.Add(i);
            }
        }
        int limit = trianglesList.Count - 2;
        for(int i = 0; i < limit; i+=3)
        {
            for(int j = 0; j < breakableVertices.Count; j++)
            {
                //Debug.Log(i.ToString() + j.ToString() + trianglesList[i].ToString()+ "////");
                if (breakableVertices[j] == trianglesList[i])
                {
                    //Debug.Log(i.ToString() + j.ToString() + '1');
                    BreakTriangle(i, wt[i]);
                    i -= 3;
                    limit -= 3;
                    break;
                }
                else if(breakableVertices[j] == trianglesList[i + 1])
                {
                    //Debug.Log(i.ToString() + j.ToString() + '2');
                    BreakTriangle(i, wt[i]);
                    i -= 3;
                    limit -= 3;
                    break;
                }
                else if (breakableVertices[j] == trianglesList[i + 2])
                {
                    //Debug.Log(i.ToString() + j.ToString() + '3');
                    BreakTriangle(i, wt[i]);
                    i -= 3;
                    limit -= 3;
                    break;
                }
            }
        }
        //Debug.Log("end");
        //BreakTriangle(0);

        vertices = verticesList.ToArray();
        triangles = trianglesList.ToArray();
        //mesh.RecalculateNormals();
        //DrawEdges();
    }

    private void BreakTriangle(int index_in_triangles, int currentWt)
    {
        int a, b, c;
        a = trianglesList[index_in_triangles];
        b = trianglesList[index_in_triangles + 1];
        c = trianglesList[index_in_triangles + 2];
        int current_no_of_vertices = vertices.Length;

        Vector3 a_b = (vertices[a] + vertices[b]) / 2;
        Vector3 b_c = (vertices[b] + vertices[c]) / 2;
        Vector3 c_a = (vertices[c] + vertices[a]) / 2;

        int ab = -1;
        int bc = -1;
        int ca = -1;

        for (int i = 0; i < verticesList.Count; i++)
        {
            if (verticesList[i] == a_b)
            {
                ab = i;
            }
            if (verticesList[i] == b_c)
            {
                bc = i;
            }
            if (verticesList[i] == c_a)
            {
                ca = i;
            }
        }

        /*if (!(ab==-1 || bc==-1 || ca == -1))
        {
            return; 
            //returning if there are no new vertices
        }*/
        Vector3 camPoint = camProxy.transform.position;
        if (ab == -1)
        {
            verticesList.Add(a_b);
            //wt.Add(currentWt);
            wt.Add((int)Mathf.Ceil(Vector3.Distance(camPoint, gameObject.transform.position)));
            ab = current_no_of_vertices;
            current_no_of_vertices++;
        }
        if(bc == -1)
        {
            verticesList.Add(b_c);
            //wt.Add(currentWt);
            wt.Add((int)Mathf.Ceil(Vector3.Distance(camPoint, gameObject.transform.position)));
            bc = current_no_of_vertices;
            current_no_of_vertices++;
        }
        if (ca == -1)
        {
            verticesList.Add(c_a);
            //wt.Add(currentWt);
            wt.Add((int)Mathf.Ceil(Vector3.Distance(camPoint, gameObject.transform.position)));
            ca = current_no_of_vertices;
            current_no_of_vertices++;
        }

        vertices = verticesList.ToArray();

        //removing old triangle from triangles
        trianglesList.RemoveAt(index_in_triangles); //time complexity O(1)
        trianglesList.RemoveAt(index_in_triangles);
        trianglesList.RemoveAt(index_in_triangles);

        //adding new 4 triangles into triangles
        List<int> newFaces = new List<int>() { 
            a,ab,ca,
            b,bc,ab,
            c,ca,bc,
            ab,bc,ca,
        };
        trianglesList.AddRange(newFaces);
    }
    private void ShootRay()
    {
        GetComponent<MeshCollider>().convex = false;
        GetComponent<MeshCollider>().convex = true;
        Debug.Log("here");
        RaycastHit[] hits;
        hits = Physics.RaycastAll(camera.transform.position, camera.transform.forward, Mathf.Infinity);
        Debug.Log(hits.Length);
        for(int i = 0; i < hits.Length; i++)
        {
            Debug.DrawRay(camera.transform.position, camera.transform.forward * hits[i].distance, Color.yellow, Mathf.Infinity);
        }
        /*if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hits, Mathf.Infinity))
        {
            Debug.Log("hit");
            Debug.DrawRay(camera.transform.position, camera.transform.forward * hit.distance, Color.yellow, Mathf.Infinity);
        }
        else
        {
            Debug.DrawRay(camera.transform.position, camera.transform.forward * 1000, Color.red, Mathf.Infinity);
            Debug.Log("Did not Hit");
        }*/
    }

    private void MakeHeights()
    {
        heights = new float[vertices.Length];

        ComputeBuffer shaderVertices = new ComputeBuffer(vertices.Length, 3*sizeof(float));
        shaderVertices.SetData(vertices);
        ComputeBuffer shaderHeights = new ComputeBuffer(heights.Length, sizeof(float));
        shaderHeights.SetData(heights);
        ComputeBuffer shaderCraterCentres = new ComputeBuffer(craters.craterCentres.Length, 3 * sizeof(float));
        shaderCraterCentres.SetData(craters.craterCentres);
        ComputeBuffer shaderCraterRadii = new ComputeBuffer(craters.craterRadii.Length, sizeof(float));
        shaderCraterRadii.SetData(craters.craterRadii);
        ComputeBuffer shaderCraterDepth = new ComputeBuffer(craters.craterDepth.Length, sizeof(float));
        shaderCraterDepth.SetData(craters.craterDepth);

        planetHeightShader.SetBuffer(0, "vertices", shaderVertices);
        planetHeightShader.SetBuffer(0, "heights", shaderHeights);
        planetHeightShader.SetBuffer(0, "craterCentres", shaderCraterCentres);
        planetHeightShader.SetBuffer(0, "craterRadii", shaderCraterRadii);
        planetHeightShader.SetBuffer(0, "craterDepth", shaderCraterDepth);
        planetHeightShader.SetFloat("planetRadius", radius);
        planetHeightShader.SetInt("numCraters", craters.numCraters);
        planetHeightShader.SetInt("numVertices", vertices.Length);
        planetHeightShader.SetFloat("smoothness", craters.smoothness);
        planetHeightShader.SetFloat("rimWidth", craters.rimWidth);
        planetHeightShader.SetFloat("rimHeight", craters.rimHeight);

        planetHeightShader.Dispatch(0, vertices.Length / 10, 1, 1);

        //getting shaderData(shader heights) into c# heights
        shaderHeights.GetData(heights);

        shaderVertices.Dispose();
        shaderHeights.Dispose();
        shaderCraterCentres.Dispose();
        shaderCraterRadii.Dispose();
        shaderCraterDepth.Dispose();

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = vertices[i].normalized * (heights[i]);
        }
    }

    private void SpawnDebugCubes()
    {
        GameObject[] deleteThem = GameObject.FindGameObjectsWithTag("debugCubes");
        for(int i = 0; i < deleteThem.Length; i++)
        {
            GameObject.Destroy(deleteThem[i]);
        }


        for(int i = 0; i < vertices.Length; i++)
        {
            GameObject.Instantiate(prefab, vertices[i], Quaternion.identity).GetComponent<number>().index = i;
        }
    }

    private void DebugEdgeLines()
    {
        for (int i = 0; i < triangles.Length - 2; i += 3)
        {
            Debug.DrawLine(vertices[triangles[i]], vertices[triangles[i + 1]], Color.black);
            Debug.DrawLine(vertices[triangles[i + 1]], vertices[triangles[i + 2]], Color.black);
            Debug.DrawLine(vertices[triangles[i + 2]], vertices[triangles[i]], Color.black);
        }
    }
}
