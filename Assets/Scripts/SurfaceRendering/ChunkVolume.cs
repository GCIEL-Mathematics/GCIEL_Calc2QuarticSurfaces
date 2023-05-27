using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
This script is used to define the 2x2x2 grid of chunks that makes up the rendered object. 
Each chunk has a limit to the amount of triangles it can define because of the memory size therefore,
it gets split up to keep the triangle count lower.

This object holds the parameters of the object itself.

The frequency and amplitude fields are artifacts of the original marching cube code. 
I have reused them to show examples of how parameters can be passed to the compute shader.
*/

public class ChunkVolume : MonoBehaviour
{

    public int function;

    public GameObject chunkPrefab;
    public List<Chunk> chunks;

    public SlicePlane slicePlane;

    [SerializeField, Range(0.1f, 5f)] public float size = 1;
    [SerializeField, Range(1f, 20f)] public float scale = 10;
    [SerializeField, Range(0f, 4f)] public float frequency = 0.08f;
    [SerializeField, Range(1f, 4f)] public float amplitude = 5;

    void Start(){

        slicePlane.chunkVolume = this;

        chunks = new List<Chunk>();

        for(int i = 0; i < 8; i++){
            chunks.Add(Instantiate(chunkPrefab, transform.position, Quaternion.identity, transform).GetComponent<Chunk>());
            chunks[i].size = size;
        }

        // Each chunk's origin is defined in it's center so each chunk needs to be offset from the origin of the whole volume.
        float offsetVal = (GridMetrics.PointsPerChunk / 2 - 0.5f) * (size / 100f);
        chunks[0].transform.position = transform.position + new Vector3(-offsetVal, -offsetVal, offsetVal);
        chunks[1].transform.position = transform.position + new Vector3(offsetVal, -offsetVal, offsetVal);
        chunks[2].transform.position = transform.position + new Vector3(-offsetVal, -offsetVal, -offsetVal);
        chunks[3].transform.position = transform.position + new Vector3(offsetVal, -offsetVal, -offsetVal);
        chunks[4].transform.position = transform.position + new Vector3(-offsetVal, offsetVal, offsetVal);
        chunks[5].transform.position = transform.position + new Vector3(offsetVal, offsetVal, offsetVal);
        chunks[6].transform.position = transform.position + new Vector3(-offsetVal, offsetVal, -offsetVal);
        chunks[7].transform.position = transform.position + new Vector3(offsetVal, offsetVal, -offsetVal);

        Render();

    }

    void Update(){
        // Render(); // Uncomment if you want the object to update on every frame
    }

    public void Render(){
        // Here we dispatch all of the parameters to the chunks and call for each to be rerendered
        for(int i = 0; i < 8; i++){
            NoiseGenerator ng = chunks[i].GetComponent<NoiseGenerator>();
            ng.frequency = frequency;
            ng.amplitude = amplitude;
            ng.scale = scale;
            ng.size = size;
            ng.function = function;
            // ng.planeForward = slicePlane.transform.forward;
            // ng.planeRight = slicePlane.transform.right;
            ng.Offset = chunks[i].transform.position - transform.position;
            chunks[i].Render();
        }
        
        slicePlane.Render();
    }
}
