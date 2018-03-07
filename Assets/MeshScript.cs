using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshScript : MonoBehaviour {


    public Material myMaterial;
    [Range(1,10)]
    public int numberOfApplications = 1;
    [Range(1f,5f)]
    public float spread;
    [Range(1f, 5f)]
    public float multiplier = 1;
    [Range(0, 1)]
    public float water = 0.5f;
    [Range(0, 1)]
    public float mountains = 0.5f;
    public bool alterCurrent;
    public Mesh baseMesh;
    private Mesh myMesh;
    private MeshFilter myMeshFilter;
    private float myRadius = 1;
    private Texture2D myTexture;
    private bool transition = false;
    private float timer = 0;
    private Vector3[] startVerts;
    private Vector3[] targetVerts;
    private MeshCollider myMeshCollider;
    private float[] perlinNoiseValues;
    void Awake()
    {
        myMeshCollider = GetComponent<MeshCollider>();
        myMeshFilter = GetComponent<MeshFilter>();
        myTexture = new Texture2D(2048,1024);
        myMaterial.mainTexture = myTexture;
        GetComponent<MeshRenderer>().material = myMaterial;
        RestartMesh();
    }
    void Start()
    {
        //ApplyPerlinNoise(myMesh, new Vector2(Random.Range(0, 100), Random.Range(0, 100)), spread, multiplier, alterCurrent);
    }

	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space)) { transition = false; myMeshCollider.sharedMesh = myMesh; }
        if (Input.GetKeyDown(KeyCode.R)) RestartMesh();
        if (Input.GetKeyDown(KeyCode.Alpha1)) TransitionToBase();
	    if (Input.GetKeyDown(KeyCode.Alpha2)) MakeMeshSpherical(myMesh, myRadius);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ApplyPerlinNoise(myMesh, new Vector3(Random.Range(0, 100), Random.Range(0, 100), Random.Range(0,100)), spread, multiplier,numberOfApplications, alterCurrent);
        if (Input.GetKeyDown(KeyCode.I)) Info(myMesh);

        transform.Rotate(new Vector2(5, 15)*Time.deltaTime);
	
        if (transition)
        {
            timer += Time.deltaTime;

            SmoothTransition(timer);
            //if (timer > 1)
            //{
            //    transition = false;
            //    timer = 0;
            //    myMeshCollider.sharedMesh = myMesh;
            //    //ColorTexture();
            //}
        }
        RaycastHit hit;
        if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit))
        {
            if (transition)
                myMeshCollider.sharedMesh = myMesh;
            ChangeAtPoint(myMesh, hit.point,1.2f);
        } 
        if (Input.GetMouseButtonDown(1) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            if (transition)
                myMeshCollider.sharedMesh = myMesh;
            ChangeAtPoint(myMesh, hit.point,0.8f);
        }
	}

    //void AlterMesh(Vector3[] vertices, int[] triangles = null, Vector2[] uvs = null)
    //{
    //    myMesh.vertices = vertices;
    //    if (triangles != null) myMesh.triangles = triangles;
    //    else myMesh.triangles = baseMesh.triangles;
    //    if (uvs != null) myMesh.uv = uvs;
    //    else myMesh.uv = baseMesh.uv;
    //}

    void MakeMeshSpherical(Mesh mesh, float radius = 0.3f)
    {
        startVerts = myMesh.vertices;
        int count = mesh.vertices.Length;
        for (int i = 0; i < count; i++)
        {
            targetVerts[i] = Vector3.Lerp(startVerts[i], startVerts[i].normalized * radius, 1f);
        }
        transition = true;
        timer = 0;
        myRadius = radius;
    }

    void ApplyPerlinNoise(Mesh mesh, Vector3 offset, float spread, float multiplier = 1, int numberOfApplications = 1, bool current = false)
    {
        startVerts = myMesh.vertices;
        Vector3[] vertices = current ? mesh.vertices : baseMesh.vertices;

        for (int i =0;i< vertices.Length;i++)
        {
            perlinNoiseValues[i] = 0;
        }
        while (numberOfApplications > 0)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                perlinNoiseValues[i] += ((Mathf.PerlinNoise(vertices[i].x * spread + offset.x, vertices[i].y * spread + offset.y) + (Mathf.PerlinNoise(vertices[i].z * spread + offset.z, vertices[i].y * spread + offset.y)) + (Mathf.PerlinNoise(vertices[i].x * spread + offset.x, vertices[i].z * spread + offset.z)))-1.5f) * multiplier/20;
                targetVerts[i] = vertices[i].normalized * (myRadius + (perlinNoiseValues[i]) );
            }
            numberOfApplications--;
            multiplier *=0.5f;
            spread *= 2;
        }
        transition = true;
        timer = 0;
    }
    void ChangeAtPoint(Mesh mesh, Vector3 point, float lenghtMod)
    {
        startVerts = myMesh.vertices;
        List<int> vertsIndexes = FindVertices(point, 0.5f);
        foreach (int i in vertsIndexes)
        {
            targetVerts[i] = startVerts[i].magnitude * startVerts[i].normalized * lenghtMod;
            Debug.Log(targetVerts[i].magnitude);
        }
        transition = true;
        timer = 0;
    }

    void SmoothTransition(float timer)
    {
        Vector3[] currVerts = new Vector3[startVerts.Length];
        Color[] colors = new Color[startVerts.Length];
        for (int i=0;i<startVerts.Length;i++)
        {
            currVerts[i] = Vector3.Lerp(startVerts[i], targetVerts[i], timer);
            float magnitude = currVerts[i].magnitude;
            if (magnitude < (0.5f + water) * myRadius) { currVerts[i] = targetVerts[i].normalized * myRadius * (0.49f + water); colors[i] = new Color(0, 0, 0.8f); }
            else if (magnitude >= (0.5f + water) * myRadius && magnitude < myRadius * (0.57f + water)) { colors[i] = new Color(0.1f, 0.6f, 0.1f); }
            else if (magnitude > myRadius * (0.57f + water) && magnitude < myRadius * (1.1f + water - mountains)) { colors[i] = new Color(0.3f, 0.3f, 0.3f); }
            else
            {
                currVerts[i] = targetVerts[i].normalized * (myRadius+(perlinNoiseValues[i]*(1+mountains)));
                colors[i] = new Color(0.8f, 0.8f, 0.8f);
            }
        }
        myMesh.vertices = currVerts;
        myMesh.colors = colors;
        myMesh.RecalculateNormals();
    }

    void TransitionToBase()
    {
        startVerts = myMesh.vertices;
        targetVerts = baseMesh.vertices;
        transition = true;
        timer = 0;
    }

    void RestartMesh()
    {
        myMesh = new Mesh();
        myMesh.vertices = (Vector3[])baseMesh.vertices.Clone();
        myMesh.triangles = (int[])baseMesh.triangles.Clone();
        myMesh.uv = (Vector2[])baseMesh.uv.Clone();
        perlinNoiseValues = new float[myMesh.vertices.Length];
        startVerts = myMesh.vertices;
        targetVerts = myMesh.vertices;
        myMeshFilter.mesh = myMesh;
        myMeshCollider.sharedMesh = myMesh;
        CalculateUVs(myMesh);
        //ColorTexture();
        GetComponent<MeshRenderer>().material = myMaterial;
        myMesh.RecalculateNormals();

        transition = false;
    }

    List<int> FindVertices(Vector3 position, float radius)
    {
        List<int> tempVertsIndexes = new List<int>();
        Vector3[] tmpVerts = myMesh.vertices;
        int count = myMesh.vertices.Length;
        for (int i = 0; i < count; i++)
        {
            float Distance = Vector3.Distance(position, transform.rotation * (tmpVerts[i] ));
            if (Distance < radius)
            {
                tempVertsIndexes.Add(i);
            }
        }
        Debug.Log(tempVertsIndexes.Count);
        return tempVertsIndexes;
    }

    void Info(Mesh mesh)
    {
        Debug.Log("Vertices: " + mesh.vertices.Length + ", UVs: " + mesh.uv.Length + ", Triangles: " + mesh.triangles.Length + ", Pixels in texture: " + myTexture.GetPixels().Length + " ,PerlinsNoise: " + perlinNoiseValues[0] + " " + perlinNoiseValues[100] + " " + perlinNoiseValues[300] + " " + perlinNoiseValues[500]);
    }

    void CalculateUVs(Mesh mesh)
    {
        Vector3[] verts = mesh.vertices;
        int count = verts.Length;
        Vector2[] tmpUVs = new Vector2[count];
        for(int i = 0; i<count; i++)
        {
            Vector3 vertice =verts[i].normalized;
            tmpUVs[i] = new Vector2((Mathf.Atan2(vertice.x, vertice.z) / Mathf.PI )*0.5f, vertice.y * 0.5f + 0.5f);
        }
        myMesh.uv = tmpUVs;
    }

    void ColorTexture()
    {
        Color[] tmpColors = new Color[myTexture.GetPixels().Length];
        float[] tmpPerlins = perlinNoiseValues;
        int countOfUVs = myMesh.uv.Length;
        int currUV = 0;
        int countOfPixels = myTexture.GetPixels().Length;
        int curPixel = 0;
        float pixelsPerUV =  countOfPixels/ (float)countOfUVs;
            
        while (currUV < countOfUVs)
        {
            while(curPixel< countOfPixels)
            {
                tmpColors[curPixel] = new Color(tmpPerlins[currUV], tmpPerlins[currUV], tmpPerlins[currUV]);
                curPixel++;
                if (curPixel >= (currUV + 1) * pixelsPerUV) break;
            }
            currUV++;

        }
        myTexture.filterMode= FilterMode.Point;
        myTexture.SetPixels(tmpColors);
        myTexture.Apply();
    }
}
