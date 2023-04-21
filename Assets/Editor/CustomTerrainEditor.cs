﻿using UnityEditor;
using UnityEngine;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]


public class CustomTerrainEditor : Editor
{

    // properties -----------
    SerializedProperty randomHeightRange;
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;
    SerializedProperty perlinXScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinXOffset;
    SerializedProperty perlinYOffset;
    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistance;
    SerializedProperty perlinHeightScale;
    SerializedProperty resetTerrain;
    SerializedProperty peakCount;
    SerializedProperty fallOffValue;
    SerializedProperty dropOffValue;
    SerializedProperty minHeight;
    SerializedProperty maxHeight;
    SerializedProperty voronoiType;



    GUITableState perlinParameterTable;
    SerializedProperty perlinParameters;

    // allows this secttion ot be dropped down
    bool showRandom = false;
    private bool showLoadHeights = false;
    private bool showPerlinNoise = false;
    private bool showMultiplePerlinNoise = false;
    private bool showVoronoi = false;

    void OnEnable()
    {
        //matches the property with the one on the terrain
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinXOffset = serializedObject.FindProperty("perlinXOffset");
        perlinYOffset = serializedObject.FindProperty("perlinYOffset");
        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");
        resetTerrain = serializedObject.FindProperty("resetTerrain");
        perlinParameterTable = new GUITableState("perlinParameterTable");
        perlinParameters = serializedObject.FindProperty("perlinParameters");

        //missing multiple perlin noise TBC!!

        peakCount = serializedObject.FindProperty("peakCount");
        fallOffValue = serializedObject.FindProperty("fallOffValue");
        dropOffValue = serializedObject.FindProperty("dropOffValue");
        minHeight = serializedObject.FindProperty("minHeight");
        maxHeight = serializedObject.FindProperty("maxHeight");
        voronoiType = serializedObject.FindProperty("voronoiType");

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        // editor scripts recognise the target as the type (above)
        CustomTerrain terrain = (CustomTerrain)target;

        EditorGUILayout.PropertyField(resetTerrain);

        // this is the logic purely behind the arrow
        showRandom = EditorGUILayout.Foldout(showRandom, "Random");

        // this is what we see when the arrow is down
        if (showRandom)
        {

            // Random terrain button
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);
            if (GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }
        }   


        // this is the logic purely behind the arrow
        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi");

        // this is what we see when the arrow is down
        if (showVoronoi)
        {

            // Random terrain button
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Voronoi", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(peakCount, 1, 100, new GUIContent("Peak Count"));
            EditorGUILayout.Slider(fallOffValue, 0, 10, new GUIContent("Fall Off Value"));
            EditorGUILayout.Slider(dropOffValue, 0, 10, new GUIContent("dropOffValue"));
            EditorGUILayout.Slider(minHeight, 0, 1, new GUIContent("Min Height"));
            EditorGUILayout.Slider(maxHeight, 0, 1, new GUIContent("Max Height"));
            EditorGUILayout.PropertyField(voronoiType);

            if (GUILayout.Button("Voronoi"))
            {
                terrain.VoronoiTessalation();
            }
        }    
        



        // this is the logic purely behind the arrow
        showPerlinNoise = EditorGUILayout.Foldout(showPerlinNoise, "Perlin Noise");
        // this is what we see when the arrow is down
        if (showPerlinNoise)
        {

            // Generate noise button
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Generate Perlin Noise to Set Heights", EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinXScale, 0, 1, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0, 1, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinXOffset,0,10000, new GUIContent("Offset X"));
            EditorGUILayout.IntSlider(perlinYOffset,0,10000, new GUIContent("Offset Y"));
            EditorGUILayout.IntSlider(perlinOctaves,0,10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistance, 0, 10, new GUIContent("Persistence"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 1, new GUIContent("Height Scale"));



            if (GUILayout.Button("Generate Noise"))
            {
                terrain.Perlin();
            }
        }


        showMultiplePerlinNoise = EditorGUILayout.Foldout(showMultiplePerlinNoise, "Multiple Perlin Noise Waves");
        if (showMultiplePerlinNoise)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Multiple Perlin Noise Waves", EditorStyles.boldLabel);

            perlinParameterTable = GUITableLayout.DrawTable(perlinParameterTable, perlinParameters);

            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("+"))
            {
                terrain.AddNewPerlin();
            }           
            if(GUILayout.Button("-"))
            {
                terrain.RemovePerlin();
            }
            EditorGUILayout.EndHorizontal();
            if(GUILayout.Button("Apply Multiple Perlin"))
            {
                terrain.MultiplePerlinTerrain();
            }
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load Heights");
        // this is what we see when the arrow is down
        if (showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Heights From Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);
            if (GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }
        }

        // Reset button
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Reset Terrain", EditorStyles.boldLabel);
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrain();
        }


        serializedObject.ApplyModifiedProperties();
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
