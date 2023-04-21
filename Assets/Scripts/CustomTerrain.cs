using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using UnityEditor;


[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour
{
    // incase the user wants to reset the terrain before adding any additions to it
    public bool resetTerrain;

    // empty vector for the random height map
    public Vector2 randomHeightRange = new Vector2(0, 0.1f);

    // here we will be taking a 2D image and turning it into a height map - we need to be able to scale the pic
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);

    //perlin single wave ------------------------------------

    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinXOffset = 100;
    public int perlinYOffset = 100;
    public int perlinOctaves = 3;
    public float perlinPersistance = 8;
    public float perlinHeightScale = 0.09f;


    //Multiple perlin waves -------------------

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



    // veronoi ----------------

    public int peakCount = 3;
    public float fallOffValue = 0.2f;
    public float dropOffValue = 0.6f;
    public float minHeight = 0;
    public float maxHeight = 1;

    public enum VoronoiType {Linear = 0, Power = 1,  SinPow = 2, Combined = 3 };
    public VoronoiType voronoiType = VoronoiType.Linear;

    // Midpoint Displacement

    public float MPDHeightMin = -2;
    public float MPDHeightMax = 2;
    public float MPDHeightDampnerPower =2f;
    public float MPDRoughness = 2f;


    // data containers for this terrain ---------------
    public Terrain terrain;
    public TerrainData terrainData;



    float[,] GetHeightMap()
    {
        if (!resetTerrain)
        {
            // gets data from the current terrain
            return terrainData.GetHeights(0, 0,
                terrainData.heightmapResolution,
                terrainData.heightmapResolution);
        }
        else
        {
            //gives a fresh float value
            return new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        }
    }

    public void MidPointDisplacement()
    {
        float[,] midpointPlacementHeightMap = GetHeightMap();
        int width = terrainData.heightmapResolution- 1;
        int squareSize = width;
        float heightMin = MPDHeightMin; // (float)squareSize / 2f * 0.01f;
        float heightMax = MPDHeightMax;

        float heightDampener = (float)MathF.Pow(MPDHeightDampnerPower, -1 * MPDRoughness);

        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

//        midpointPlacementHeightMap[0, 0] = UnityEngine.Random.Range(0f, 0.2f);
//        midpointPlacementHeightMap[0, width-1] = UnityEngine.Random.Range(0f, 0.2f);
//        midpointPlacementHeightMap[width-1, 0] = UnityEngine.Random.Range(0f, 0.2f);
//        midpointPlacementHeightMap[width-1, width-1] = UnityEngine.Random.Range(0f, 0.2f);

        while (squareSize > 0)
        {
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2);
                    midY = (int)(y + squareSize / 2);

                    midpointPlacementHeightMap[midX, midY] = (float)((midpointPlacementHeightMap[x, y] +
                                                            midpointPlacementHeightMap[cornerX, y] +
                                                            midpointPlacementHeightMap[x, cornerY] +
                                                            midpointPlacementHeightMap[cornerX, cornerY]) / 4f 
                                                            + UnityEngine.Random.Range(heightMin, heightMax));
                }
            }
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2);
                    midY = (int)(y + squareSize / 2);
                
                    pmidXR = (int)(midX + squareSize);
                    pmidYU = (int)(midY + squareSize);
                    pmidXL = (int)(midX - squareSize);
                    pmidYD = (int)(midY - squareSize);

                    if (pmidXL<= 0 || pmidYD <= 0 || pmidXR >= width -1 || pmidYU >= width -1) continue;

                    //bottom middle point
                    midpointPlacementHeightMap[midX,y] = (float)((midpointPlacementHeightMap[midX, midY] +
                                                            midpointPlacementHeightMap[x, y] +
                                                            midpointPlacementHeightMap[midX, pmidYD] +
                                                            midpointPlacementHeightMap[cornerX, y]) / 4f
                                                            + UnityEngine.Random.Range(heightMin, heightMax));


                    //middle left point
                    midpointPlacementHeightMap[x,midY] = (float)((midpointPlacementHeightMap[midX, midY] +
                                                            midpointPlacementHeightMap[x, y] +
                                                            midpointPlacementHeightMap[pmidXL, midY] +
                                                            midpointPlacementHeightMap[x, cornerY]) / 4f
                                                            + UnityEngine.Random.Range(heightMin, heightMax));

                    //middle right point
                    midpointPlacementHeightMap[cornerX,midY] = (float)((midpointPlacementHeightMap[midX, midY] +
                                                            midpointPlacementHeightMap[cornerX, y] +
                                                            midpointPlacementHeightMap[pmidXR, midY] +
                                                            midpointPlacementHeightMap[cornerX, cornerY]) / 4f
                                                            + UnityEngine.Random.Range(heightMin, heightMax));


                    // top middle point
                    midpointPlacementHeightMap[midX,cornerY] = (float)((midpointPlacementHeightMap[midX, midY] +
                                                            midpointPlacementHeightMap[x,cornerY] +
                                                            midpointPlacementHeightMap[midX, pmidYU] +
                                                            midpointPlacementHeightMap[cornerX, cornerY]) / 4f
                                                            + UnityEngine.Random.Range(heightMin, heightMax));


                }
            
            }
            squareSize = (int) (squareSize/2f);
            heightMin *= heightDampener;
            heightMax *= heightDampener;
        }

        terrainData.SetHeights(0, 0, midpointPlacementHeightMap);
    }


    public void VoronoiTessalation()
    {
        float[,] voronoiHeightMap = GetHeightMap();

        // find the furthest possible distance on the heightmap (top right and bottomleft i.e)
        float maxDistance = Vector2.Distance(Vector2.zero, new Vector2(terrainData.heightmapResolution,
                                                                        terrainData.heightmapResolution));


        for (int i = 0; i < peakCount; i++)
        {
            // is the int going to cause an issue?**
            int randomPointX = UnityEngine.Random.Range(0, terrainData.heightmapResolution);
            int randomPointY = UnityEngine.Random.Range(0, terrainData.heightmapResolution);
            float randomHeight = UnityEngine.Random.Range(minHeight, maxHeight);


            // take the random point and height and load it into the heightmap
            if (voronoiHeightMap[randomPointX, randomPointY] < randomHeight)
            {
                voronoiHeightMap[randomPointX, randomPointY] = randomHeight;
            }
            else continue;
            // call it something and store it to compare later
            Vector2 peakLocation = new Vector2(randomPointX, randomPointY);

            // loop through terrain and value each point relative to the peak        
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                {
                    if (!(x == randomPointX && y == randomPointY))
                    {
                        float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;
                        // to create a curved slope the distance to peak is squared
                        // to control the gradient of slope we multiply by a fall off value

                        float h = randomHeight - distanceToPeak * fallOffValue - Mathf.Pow(distanceToPeak, dropOffValue);

                        if(voronoiType == VoronoiType.Combined)
                        {
                            h = randomHeight - distanceToPeak* fallOffValue - 
                                 MathF.Pow(distanceToPeak, dropOffValue);
                        }  
                        else if(voronoiType == VoronoiType.Power)
                        {
                            h = randomHeight-MathF.Pow(distanceToPeak, dropOffValue) * fallOffValue;
                        }
                        else if(voronoiType == VoronoiType.SinPow)
                        {
                            h = randomHeight - MathF.Pow(distanceToPeak*3, fallOffValue) - MathF.Sin(distanceToPeak*2*MathF.PI)/dropOffValue;
                        }
                        else
                        {
                            h = randomHeight-distanceToPeak * fallOffValue;
                        }


                        if (voronoiHeightMap[x,y] <h)
                        {
                            voronoiHeightMap[x, y] = h;
                        }

                    }
                }
            }
        }
        terrainData.SetHeights(0, 0, voronoiHeightMap);
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
