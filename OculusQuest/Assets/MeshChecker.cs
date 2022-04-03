#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
class MyCustomBuildProcessor : IPreprocessBuild
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        PresentationObject[] presentObjs = UnityEngine.Object.FindObjectsOfType<PresentationObject>();
        MeshFilter[] meshObjs = Resources.LoadAll<MeshFilter>("Models");
        foreach(PresentationObject obj in presentObjs)
        {
            MeshFilter filter = obj.GetComponent<MeshFilter>();
            if(filter!=null)
            {
                if(!filter.sharedMesh.isReadable)
                {
                    Debug.LogError("Mesh of "+filter.name+" is not readable");
                }
            }
        }

        foreach(MeshFilter filter in meshObjs)
        {
            if (!filter.sharedMesh.isReadable)
            {
                Debug.LogError("Mesh of " + filter.name + " is not readable");
            }
        }

    }
}
#endif