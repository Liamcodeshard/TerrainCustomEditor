using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour
{
    // empty vector for the random height map
    public Vector2 randomHeightRange = new Vector2(0,0.1f);
    
    // here we will be taking a 2D image and turning it into a height map - we need to be able to scale the pic
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);

    //perlin stuff
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;

    // could add an offset
    public float perlinXOffset = 100f;
    public float perlinYOffset = 100f;


    // data containers for this terrain
    public Terrain terrain;
    public TerrainData terrainData;

    
    public void Perlin()
    {
        float[,] perlinHeightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

        //make heightmap with perlin noise

        for(int x = 0; x< terrainData.heightmapResolution; x++) 
        {
            for (int y = 0; y< terrainData.heightmapResolution; y++)
            {
                perlinHeightMap[x,y] = Mathf.PerlinNoise(x*perlinXScale + perlinXOffset, y*perlinYScale + perlinYOffset);
            }
        }

        terrainData.SetHeights(0, 0, perlinHeightMap);
    }

    public void RandomTerrain()
    {
        // get current hieght map data onto a new hieghtmap
        float[,] randomHeightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, 
                                                                    terrainData.heightmapResolution);

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
        float[,] imageTextureHeightMap;
        imageTextureHeightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution]; 
        
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {
                // set each point on the map to the corresponding pixel on the image
                imageTextureHeightMap[x, z] =
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
