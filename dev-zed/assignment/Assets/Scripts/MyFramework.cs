using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;

static class MyFramework
{
    private const int DIMENSION_3D_COUNT = 3;
    private const int TRIANGLE_VERTEX_COUNT = 3;
    private const int RECT_VERTEX_COUNT = 4;
    private const float METER_EACH_UV_1UNIT = 3;
    private const float WINDOW_START_ANGLE = 180;
    private const float WINDOW_END_ANGLE = 220;
    private const float SAME_UNIT_VECTOR_DOT_APPROXIMATION = 0.99f;
    private const string MAIN_TEXTURE_PROPERTY_NAME = "_BaseMap";
    private static GameObject gameObject;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void ScriptStart() 
    {
        TextAsset rawData = (TextAsset)AssetDatabase.LoadAssetAtPath(
            "Assets/Samples/json/dong.json", typeof(TextAsset));
        JObject AllData = JObject.Parse(rawData.ToString());
        
        Texture texture = (Texture)AssetDatabase.LoadAssetAtPath(
            "Assets/Samples/texture/buildingTester_d.png", typeof(Texture));

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Plane); 
        cube.transform.position = new Vector3(0, 0, 0);
        cube.transform.localScale = new Vector3(100, 1, 100);
        
        List<Vector3> verticesList = new List<Vector3>();
        
        /* 동, 룸타입id 처럼 key가 한글일 경우 Newtonsoft json 사용. coordinatesBase64s 만 뽑는다 JsonUtility 사용 가능*/
            
        foreach (var data in AllData["data"])
        {
            foreach (var room in data["roomtypes"])
            {
                foreach (var base64Str in room["coordinatesBase64s"])
                {
                    List<Vector3> verticesPart = GetCoordinate(base64Str.ToString());
                    
                    if (verticesPart.Count != 0)
                    {
                        verticesList.AddRange(verticesPart);    
                    }
                }
            }
        }

        Vector3[] vertices = verticesList.ToArray();
         
        int[] triangles = Enumerable.Range(0, vertices.Length).ToArray();
          
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.uv = GetMeshUV(vertices, mesh.normals);;
         
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.SetTexture(MAIN_TEXTURE_PROPERTY_NAME, texture);
        material.SetTextureScale(MAIN_TEXTURE_PROPERTY_NAME, new Vector2(1f, 0.5f));
        gameObject = new GameObject();
        gameObject.transform.position = new Vector3(0, 0, 0);
        gameObject.AddComponent<MeshRenderer>().material = material;
        gameObject.AddComponent<MeshFilter>().mesh = mesh;
    }
    
    private static Vector2[] GetMeshUV(Vector3[] vertices, Vector3[] meshNormals)
    {
        Vector2[] UVList = new Vector2[vertices.Length];
        
        Dictionary<Vector3, Vector2> savedUVList = new Dictionary<Vector3, Vector2>();
         
        for (int i = 0; i < vertices.Length; i+=TRIANGLE_VERTEX_COUNT)
        {
            Vector3 normal = meshNormals[i].normalized;
            
            /* Auto calculate normal not exactly make up, down direction normal.Use approximation */
            if (Vector3.Dot(normal, Vector3.up) > SAME_UNIT_VECTOR_DOT_APPROXIMATION ||
                Vector3.Dot(normal, Vector3.down) > SAME_UNIT_VECTOR_DOT_APPROXIMATION)
            {
                UVList[i] = new Vector2(0.75f, 0.5f);
                UVList[i+1] = new Vector2(1, 0.5f);
                UVList[i+2] = new Vector2(1, 1f);
            }
            else
            {
                float angleFromForward = GetAngle(normal);
                    
                float[] wallRangeUV = angleFromForward >= WINDOW_START_ANGLE && angleFromForward <= WINDOW_END_ANGLE ? 
                    new [] {0.5f, 0} : new [] {0.75f, 0.5f};

                Vector3? refVertex = null;
                
                for (int j = 0; j < TRIANGLE_VERTEX_COUNT; j++)
                {
                    SetUV(UVList, vertices, savedUVList, ref refVertex, normal, wallRangeUV,  i+j);    
                }

                if (savedUVList.Count == RECT_VERTEX_COUNT)
                {
                    savedUVList.Clear();
                }
            }
        }
        return UVList.ToArray();
    }

    private static  void SetUV(Vector2[] UVList, Vector3[] vertices, Dictionary<Vector3, Vector2> savedUVList, 
        ref Vector3? refVertex, Vector3 normal, float[] wallRangeUV, int index)
    {
        
        Vector2 UV;
        Vector3 vertex = vertices[index];
        
        if (savedUVList.ContainsKey(vertex))
        {
            UV = savedUVList[vertex];
        }
        else
        {
            int oneCoordV = (int)(vertex.y / METER_EACH_UV_1UNIT);
            
            if (refVertex.HasValue == false)
            {
                UV = new Vector2(wallRangeUV[0], oneCoordV);
            }
            else
            {
                Vector3 DiffVector = vertex - refVertex.Value;
                DiffVector.y = 0;

                float oneCoordU = Vector3.Cross(normal, DiffVector).y < 0 ? wallRangeUV[0] : wallRangeUV[1];        
                UV = new Vector2(oneCoordU, oneCoordV);
            }
        }
        savedUVList[vertex] = UV;
        
        UVList[index] = UV;
        refVertex = vertex;
    }
    
    private static List<Vector3> GetCoordinate(string base64Str) 
    {
        byte[] bytePoints = Convert.FromBase64String(base64Str);
        float[] points = new float[bytePoints.Length / sizeof(float)];
        Buffer.BlockCopy(bytePoints, 0, points, 0, bytePoints.Length);
        
        List<Vector3> vertices = new List<Vector3>();
        
        if (points.Length % DIMENSION_3D_COUNT != 0)
        {
            Debug.LogError("The coordinate needs to three times numbers");
            return vertices;
        }

        Vector3 vertex = new Vector3();
        
        for (int i = 0; i < points.Length; i++)
        {
            int axisIndex = i % DIMENSION_3D_COUNT;
            
            if (axisIndex == 0)
            {
                vertex.x = points[i];
            } 
            else if(axisIndex == 1) 
            {
                vertex.z = points[i];
            } 
            else 
            {
                vertex.y = points[i];
                vertices.Add(vertex);
                vertex = new Vector3();
            }
        }

        if (vertices.Count % TRIANGLE_VERTEX_COUNT != 0)
        {
            Debug.LogError("A triangle needs to three times vertex.");
            return vertices;
        }
        return vertices;
    }

    private static float GetAngle (Vector3 normal)
    {
        return Quaternion.FromToRotation(Vector3.forward, normal).eulerAngles.y;
    }
}


