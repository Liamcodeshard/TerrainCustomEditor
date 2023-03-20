using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour
{
    public Vector2 randomHeightRange = new Vector2(0,0.1f);
    public Terrain terrain;
    public TerrainData terrainData;
    public void RandomTerrain()
    {
        // create somewhere to store the point data
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, 
                                                                    terrainData.heightmapHeight);

        // get the size of the terrain to size our height map
        heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        
        //loop along the x and y
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                // set each point to a random height between our range
                heightMap[x, y] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        // set the terrain to our heightmaps randomly generated points
        // we can limit the affected region using the x,y values here,
        // but we dont want to since we want to affect the whole terrain
        terrainData.SetHeights(0,0, heightMap);
    }

    public void ResetTerrain()
    {
        float[,] resetMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
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
