using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour
{
    public Vector2 randomHeightRange = new Vector2(0,0.1f);

    public void RandomTerrain()
    {

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
