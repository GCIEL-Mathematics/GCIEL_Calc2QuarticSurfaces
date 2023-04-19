
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChunkVolume))]
public class RenderButton : Editor
{
    public override void OnInspectorGUI() {
        
        DrawDefaultInspector();

        ChunkVolume myChunk = (ChunkVolume)target;

        if(GUILayout.Button("Render"))
            myChunk.Render();
   }
}
