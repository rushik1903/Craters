using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Craters : MonoBehaviour
{
    public AnimationCurve craterRadiusCurve, craterDepthCurve;
    public float maxCraterRadius;
    public GameObject prefab;
    public int numCraters = 5;
    private SphereMaker sphereMaker;
    public float radius = 5f;
    public float smoothness = 0.5f;
    public float rimWidth = 0.33f;
    public float rimHeight = 0.2f;

    public Vector3[] craterCentres;
    public float[] craterRadii;
    public float[] craterDepth;

    // Start is called before the first frame update    
    void Start()
    {
        sphereMaker = gameObject.GetComponent<SphereMaker>();
        UpdateCraters();
        //craterCentres[4] = new Vector3(0, 5, 0);
    }

    private void UpdateCraters()
    {
        craterCentres = new Vector3[numCraters];
        craterRadii = new float[numCraters];
        craterDepth = new float[numCraters];
        for (int i = 0; i < numCraters; i++)
        {
            craterCentres[i] = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            craterCentres[i] = craterCentres[i].normalized * radius;
            craterRadii[i] = RandomRadius();
            craterDepth[i] = RandomDepth();
        }
    }

    private float RandomDepth()
    {
        return craterDepthCurve.Evaluate(Random.Range(0f, 1f));
    }

    private float RandomRadius()
    {
        return craterRadiusCurve.Evaluate(Random.Range(0f, 1f)) * maxCraterRadius;
    }

    private void SpawnDebugCubes()
    {
        GameObject[] deleteThem = GameObject.FindGameObjectsWithTag("craterCentre");
        for (int i = 0; i < deleteThem.Length; i++)
        {
            GameObject.Destroy(deleteThem[i]);
        }


        for (int i = 0; i < craterCentres.Length; i++)
        {
            meteorNumber temp = GameObject.Instantiate(prefab, craterCentres[i], Quaternion.identity).GetComponent<meteorNumber>();
            temp.index = i;
            temp.radius = craterRadii[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            UpdateCraters();
            //SpawnDebugCubes();
        }
    }
}
