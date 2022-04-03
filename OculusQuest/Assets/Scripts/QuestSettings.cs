#if (UNITY_EDITOR) 
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class QuestSettings
{
    [MenuItem("Window/Ensure Oculus Quest settings")]
    static void EnsureSettings()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
        EditorApplication.ExecuteMenuItem("Oculus/Tools/Remove AndroidManifest.xml");
        EditorApplication.ExecuteMenuItem("Oculus/Tools/Create store-compatible AndroidManifest.xml");
        addTag("EnableOnChoice");        
        UnityEditor.PlayerSettings.virtualRealitySupported = true;
        UnityEditor.PlayerSettings.SetVirtualRealitySDKs(BuildTargetGroup.Android, new string[] {"Oculus"});
        UnityEditor.PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
        UnityEditor.PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
        UnityEditor.PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.etuvpretool.Default"+DateTime.Now.ToString("yyyy-MM-dd"));
    }

    [MenuItem("Window/Adapt scene for Oculus Quest")]
    static void AdaptScene()
    {
        PresentationWindow.EnsureMaster();
        Camera[] cameras = UnityEngine.Object.FindObjectsOfType<Camera>();
        foreach (Camera cam in cameras)
        {
            GameObject.DestroyImmediate(cam);
        }
        PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/OVRCameraRig.prefab"));
    }


    static void addTag(string tag)
    {
        UnityEngine.Object[] tagAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if(tagAsset!=null && tagAsset.Length>0)
        {
            SerializedObject obj = new SerializedObject(tagAsset[0]);
            SerializedProperty tags = obj.FindProperty("tags");
            bool tagAlreadyPresent = false;
            for(int i=0; i<tags.arraySize;i++)
            {
                if(tags.GetArrayElementAtIndex(i).stringValue==tag)
                {
                    tagAlreadyPresent = true;
                }
            }

            if(!tagAlreadyPresent)
            {
                tags.InsertArrayElementAtIndex(0);
                tags.GetArrayElementAtIndex(0).stringValue = tag;
                obj.ApplyModifiedProperties();
                obj.Update();
            }

        }
    }



}

#endif