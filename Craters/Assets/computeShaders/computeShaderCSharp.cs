using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class computeShaderCSharp : MonoBehaviour
{
    public GameObject camera;
    public ComputeShader computeShader;

    public RenderTexture renderTexture;

    public GameObject prefab;

    public List<GameObject> objects;

    private float count = 10;

    private Cube[] data;

    public struct Cube
    {
        public Vector3 position;
        public Color color;
    }
    // Start is called before the first frame update

    /*private void CreateCube(int x, int y)
    {
        GameObject cube = new GameObject("Cube " + x * count + y, typeof(MeshFilter), typeof(MeshRenderer));
        cube.GetComponent<MeshFilter>().mesh = Mesh;
        cube.
    }*/
    void Start()
    {
        data = new Cube[(int)(count * count)];
        SpawnCubes();
        camera.transform.position = new Vector3(count / 2, count / 2, -10);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnRandomizeGPU();
        }
    }

    private void SpawnCubes()
    {
        for(int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                GameObject cube = Instantiate(prefab, new Vector3(i, j, Random.Range(0.0f, 1.0f)), Quaternion.identity);
                objects.Add(cube);

                Cube temp;
                temp.position = cube.transform.position;
                temp.color = Random.ColorHSV();
                data[(int)(i * count + j)] = temp;
            }
        }
    }

    public void OnRandomizeGPU()
    {
        int colorSize = sizeof(float) * 4;
        int vector3Size = sizeof(float) * 3;
        int totalSize = colorSize + vector3Size;

        //creating a buffer to give data to shader
        ComputeBuffer cubesBuffer = new ComputeBuffer(data.Length, totalSize);
        cubesBuffer.SetData(data);

        computeShader.SetBuffer(0, "cubes", cubesBuffer);
        computeShader.SetFloat("Resolution", data.Length);
        computeShader.Dispatch(0, data.Length / 10, 1, 1);  //dispatch probably calls the shader

        cubesBuffer.GetData(data);  //getting info back from shader

        for (int i = 0; i < objects.Count; i++)
        {
            GameObject obj = objects[i];
            Cube cube = data[i];
            obj.GetComponent<MeshRenderer>().material.SetColor("_Color", cube.color);
            objects[i].transform.position = data[i].position;
        }

        cubesBuffer.Dispose();
    }
}
