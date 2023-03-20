using UnityEditor;
using UnityEngine;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]


public class CustomTerrainEditor : Editor
{
    SerializedProperty randomHeightRange;

    // allows this secttion ot be dropped down
    bool showRandom = false;

    void OnEnable()
    {
        //matches the property with the one on the terrain
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        // editor scripts recognise the target as the type (above)
        CustomTerrain terrain = (CustomTerrain)target;

        // this is the logic purely behind the arrow
        showRandom = EditorGUILayout.Foldout(showRandom, "Random");

        // this is what we see when the arrow is down
        if(showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);
            if(GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }
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
