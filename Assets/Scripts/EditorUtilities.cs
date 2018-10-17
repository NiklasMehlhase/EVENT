using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorUtilities  {

    public static string GetSuffix(string prefix)
    {
        if (GameObject.Find(prefix) == null)
        {
            return "";
        }
        else
        {
            int num = 1;
            for (; GameObject.Find(prefix + " (" + num + ")") != null; num++) ;
            return " (" + num + ")";
        }
    }
}
