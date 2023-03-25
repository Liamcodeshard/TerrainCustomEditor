using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour
{
    // incase the user wants to reset the terrain before adding any additions to it
    public bool resetTerrain;

    // empty vector for the random height map
    public Vector2 randomHeightRange = new Vector2(0,0.1f);
    
    // here we will be taking a 2D image and turning it into a height map - we need to be able to scale the pic
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);

    //perlin single wave
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinXOffset = 100;
    public int perlinYOffset = 100;
    public int perlinOctaves = 3;
    public float perlinPersistance = 8;
    public float perlinHeightScale = 0.09f;


    //Multiple perlin waves

    //first we set up a class that contains the same parameters as our perlin noise
    [System.Serializable]
    public class PerlinParameters
    {
        //perlin stuff again
        public float mPerlinXScale = 0.01f;
        public float mPerlinYScale = 0.01f;
        public int mPerlinXOffset = 100;
        public int mPerlinYOffset = 100;
        public int mPerlinOctaves = 3;
        public float mPerlinPersistance = 8;
        public float mPerlinHeightScale = 0.09f;
        public bool remove = false;

    }
    //then we create a public list in teh editor that will contain new perlin parameter objects
    public List<PerlinParameters> perlinParameters = new List<PerlinParameters>()
    {
        new PerlinParameters()
    };



    // data containers for this terrain
    public Terrain terrain;
    public TerrainData terrainData;


    float[,] GetHeightMap()
    {
        if (!resetTerrain)
        {
            return terrainData.GetHeights(0, 0,
                terrainData.heightmapResolution,
                terrainData.heightmapResolution);
        }
        else
        {
            return new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        }
    }

    public void Perlin()
    {
        float[,] perlinHeightMap = GetHeightMap();
        //make heightmap with perlin noise

        for (int x = 0; x < terrainData.heightmapResolution; x++) 
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                perlinHeightMap[x,y] +=Utils.fBM((x + perlinXOffset) *perlinXScale,
                    (y +perlinYOffset) *perlinYScale,
                    perlinOctaves,
                    perlinPersistance) * perlinHeightScale;

            }
        }

        terrainData.SetHeights(0, 0, perlinHeightMap);
    }

    public void MultiplePerlinTerrain()
    {
        float[,] muliplePerlinHeightMap = GetHeightMap();

        for (int y = 0; y < terrainData.heightmapResolution; y++)
        {
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                foreach (PerlinParameters p in perlinParameters)
                {
                     muliplePerlinHeightMap[x, y] += Utils.fBM((x + p.mPerlinXOffset) * p.mPerlinXScale,
                                                               (y + p.mPerlinYOffset) * p.mPerlinYScale,
                                                                  p.mPerlinOctaves,
                                                                  p.mPerlinPersistance) * p.mPerlinHeightScale;
                }
               

            }
        }

        terrainData.SetHeights(0, 0, muliplePerlinHeightMap);
    }

    public void AddNewPerlin()
    {
        perlinParameters.Add(new PerlinParameters());
    }

    public void RemovePerlin()
    {
        List<PerlinParameters> keptPerlinParameters = new List<PerlinParameters>();

        for (int i = 0; i < perlinParameters.Count; i++)
        {
            if (!perlinParameters[i].remove)
            {
                keptPerlinParameters.Add(perlinParameters[i]);
            }
        }

        if (keptPerlinParameters.Count == 0)
        {
            keptPerlinParameters.Add(perlinParameters[0]);
        }

        perlinParameters = keptPerlinParameters;
    }

    public void RandomTerrain()
    {
        // get current hieght map data onto a new hieghtmap
        float[,] randomHeightMap = GetHeightMap();

        //loop along the x and y
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                // set each point to a random height between our range --> the range is the height of the map(maybe 600)
                randomHeightMap[x, y] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        // set the terrain to our heightmaps randomly generated points

        terrainData.SetHeights(0,0, randomHeightMap);
            // we can limit the affected region using the x,y values here,
        // but we dont want to since we want to affect the whole terrain
    }

    public void LoadTexture()
    {
        float[,] imageTextureHeightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {
                // set each point on the map to the corresponding pixel on the image
                imageTextureHeightMap[x, z] +=
                    heightMapImage.GetPixel((int)(x * heightMapScale.x),
                                            (int)(z * heightMapScale.z)).grayscale 
                                                * heightMapScale.y;
            }
        }
        terrainData.SetHeights(0, 0, imageTextureHeightMap);
    }

    public void ResetTerrain()
    {
        float[,] resetMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
       terrainData.SetHeights(0, 0,resetMap);

    }


    void Awake()
    {
        // gets the unity object that manages tags
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        // acess the tag property
        SerializedProperty tagsProp = tagManager.FindProperty("tags");


        // using the tag property we create tags
        AddTag(tagsProp, "Terrain");
        AddTag(tagsProp, "Cloud");
        AddTag(tagsProp, "Shore");

        //apply tag changes to tag datatbase
        tagManager.ApplyModifiedProperties();

        // tag this gameobject
        this.gameObject.tag = "Terrain";
    }
    void OnEnable()
    {
        Debug.Log("Initializing Terrain Data");
        terrain = this.GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData;
    }

    void AddTag(SerializedProperty tagsProp, string newTag)
    {
        bool found = false;

        //ensure tag doesnt exist
        for(int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag)) { found = true; break; }
        }

        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
    }


}
