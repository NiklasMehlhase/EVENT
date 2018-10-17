#if (UNITY_EDITOR) 
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

[InitializeOnLoad]
public class DragImage : Editor
{

    static DragImage()
    {
        SceneView.onSceneGUIDelegate += SceneViewCallback;
    }

    
    static void SceneViewCallback(SceneView sceneView)
    {
        if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }
        if (Event.current.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            if (DragDropCallback())
            {
                Event.current.Use();
            }
        }
    }

    static bool DragDropCallback()
    {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Vector3 intersectionPoint = Vector3.zero;
        RaycastHit hitInfo;
        if(Physics.Raycast(mouseRay, out hitInfo))
        {
            intersectionPoint = hitInfo.point-mouseRay.direction*0.01f;            
        }
        else { 
            Plane xzPlane = new Plane(new Vector3(1,0,0), new Vector3(0,0,1), Vector3.zero);
            float enter;
            xzPlane.Raycast(mouseRay, out enter);
            intersectionPoint = mouseRay.origin + mouseRay.direction * enter;
        }
        List<Texture2D> images = new List<Texture2D>();
        List<VideoClip> videos = new List<VideoClip>();
        foreach (Object obj in DragAndDrop.objectReferences)
        {
            if (obj.GetType() == typeof(Texture2D))
            {
                Texture2D tex = (Texture2D)obj;
                images.Add(tex);
            }
            else if(obj.GetType() == typeof(VideoClip))
            {
                VideoClip clip = (VideoClip)obj;
                videos.Add(clip);
            }
        }

        Quaternion rayRotation = Quaternion.LookRotation(mouseRay.direction, Vector3.up)*Quaternion.Euler(90,0,180);
        foreach (Texture2D img in images)
        {
            CreateImage(img,intersectionPoint,rayRotation);
        }
        foreach (VideoClip clip in videos)
        {
            CreateVideo(clip, intersectionPoint, rayRotation);
        }

        return images.Count>0||videos.Count>0;
    }    

    static void CreateVideo(VideoClip videoClip,Vector3 pos,Quaternion rot)
    {
        GameObject videoObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
		videoObject.name = "Video_" + videoClip.name.Replace(' ','_');
        videoObject.transform.localScale = new Vector3(0.1f * ((float)videoClip.width) / ((float)videoClip.height), 1f, 0.1f);
        videoObject.transform.rotation = rot;
        videoObject.transform.position = pos;
        MeshRenderer renderer = videoObject.GetComponent<MeshRenderer>();
        RenderTexture renderTex = new RenderTexture((int)videoClip.width, (int)videoClip.height, 24);
        renderTex.Create();
        Material mat = new Material(Shader.Find("Unlit/Texture"));
        mat.SetTexture("_MainTex", renderTex);
        renderer.material = mat;
        VideoPlayer videoPlayer = videoObject.AddComponent<VideoPlayer>();
        videoPlayer.targetTexture = renderTex;
        videoPlayer.clip = videoClip;
        videoPlayer.playOnAwake = false;
        Undo.RegisterCreatedObjectUndo(videoObject, "Create video");        
    }

    static void CreateImage(Texture2D tex,Vector3 pos,Quaternion rot)
    {
        GameObject imgObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh mesh = (Mesh)Instantiate(imgObject.GetComponent<MeshFilter>().sharedMesh);        
        List<Vector2> uvs = new List<Vector2>();        
        mesh.GetUVs(0, uvs);
        uvs[12] = new Vector2(0, 0);
        uvs[13] = new Vector2(0, -1);
        uvs[14] = new Vector2(-1, -1);
        uvs[15] = new Vector2(-1, 0);
        mesh.SetUVs(0, uvs);
        imgObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        imgObject.name = "Image_"+tex.name.Replace(' ','_');
        imgObject.transform.localScale = new Vector3(1f * ((float)tex.width) / ((float)tex.height), 0.01f, 1f);
        imgObject.transform.rotation = rot;
        imgObject.transform.position = pos;
        MeshRenderer renderer = imgObject.GetComponent<MeshRenderer>();
		Material mat = new Material(Shader.Find("Unlit/Texture"));

        mat.SetTexture("_MainTex", tex);
		renderer.material = mat;
        Undo.RegisterCreatedObjectUndo(imgObject, "Create image");
    }
}
#endif