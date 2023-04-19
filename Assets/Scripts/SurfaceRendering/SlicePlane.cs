using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicePlane : MonoBehaviour
{
    public ComputeShader MarchingShader;

    ComputeBuffer _segmentsBuffer;
    ComputeBuffer _segmentsCountBuffer;
    ComputeBuffer _weightsBuffer;
    // ComputeBuffer _vectorsBuffer;

    public NoiseGenerator NoiseGenerator;
    public ChunkVolume chunkVolume;

    public Material segmentMat;
    public Mesh segmentMesh;

    public GameObject segmentPrefab;
    public Transform planeIntersectParent;

    int planeGridPoints;

    private void Awake() {
        planeGridPoints = GridMetrics.PointsPerChunk * 2 + 16;
        CreateBuffers();
    }

    private void OnDestroy() {
        ReleaseBuffers();
    }

    struct Segment {
        public Vector3 a;
        public Vector3 b;

        public static int SizeOf => sizeof(float) * 3 * 2;
        public new string ToString => string.Format("{0}, {1}", a, b);
    }

    float[] _weights;

    public void Render(){
        NoiseGenerator.planeForward = transform.forward;
        NoiseGenerator.planeRight = transform.right;
        NoiseGenerator.Offset = transform.position - chunkVolume.transform.position;
        NoiseGenerator.size = chunkVolume.size;
        NoiseGenerator.function = chunkVolume.function;
        _weights = NoiseGenerator.GetNoisePlane(planeGridPoints);
        ConstructLine();
    }

    Vector3 oldPosition = Vector3.zero;
    Quaternion oldRotation = Quaternion.identity;

    void Update(){
        if((oldPosition - transform.position).magnitude > 0 || (oldRotation.eulerAngles - transform.rotation.eulerAngles).magnitude > 0){
            Render();
            oldPosition = transform.position;
            oldRotation = transform.rotation;
        }
        
    }

    void ConstructLine() {
        MarchingShader.SetBuffer(0, "_Segments", _segmentsBuffer);
        MarchingShader.SetBuffer(0, "_Weights", _weightsBuffer);

        MarchingShader.SetInt("_ChunkSize", planeGridPoints);
        MarchingShader.SetFloat("_IsoLevel", .5f);
        MarchingShader.SetVector("_PlaneForward", transform.forward);
        MarchingShader.SetVector("_PlaneRight", transform.right);
        // MarchingShader.SetBuffer(0, "_Vectors", _vectorsBuffer);
        
        _weightsBuffer.SetData(_weights);
        _segmentsBuffer.SetCounterValue(0);

        MarchingShader.Dispatch(0, planeGridPoints / GridMetrics.NumThreads, planeGridPoints / GridMetrics.NumThreads, 1);

        // segmentMat.SetBuffer("_Vectors", _vectorsBuffer);
        // var bounds = new Bounds(transform.position, new Vector3(GridMetrics.PointsPerChunk, GridMetrics.PointsPerChunk, GridMetrics.PointsPerChunk));
        // Graphics.DrawMeshInstancedProcedural(segmentMesh, 0, segmentMat, bounds, GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk);

        Segment[] segments = new Segment[ReadSegmentCount()];
        _segmentsBuffer.GetData(segments);

        foreach (Transform child in planeIntersectParent) {
            GameObject.Destroy(child.gameObject);
        }

        foreach(Segment seg in segments){
            Vector3 pt1 = transform.position + (transform.right * seg.a.x + transform.forward * seg.a.z) * (chunkVolume.size / 100);
            Vector3 pt2 = transform.position + (transform.right * seg.b.x + transform.forward * seg.b.z) * (chunkVolume.size / 100);
            Vector3 pos = (pt1 + pt2) * 0.5f;
            Vector3 fromVolume = (pos - chunkVolume.transform.position) * (100 / chunkVolume.size);
            
            if(Mathf.Abs(fromVolume.x) < GridMetrics.PointsPerChunk && 
                Mathf.Abs(fromVolume.y) < GridMetrics.PointsPerChunk && 
                Mathf.Abs(fromVolume.z) < GridMetrics.PointsPerChunk){
                Vector3 dir = (pt2 - pt1);
                GameObject newSeg = Instantiate(segmentPrefab, pos, Quaternion.FromToRotation(Vector3.up, dir), planeIntersectParent);
                newSeg.transform.localScale = new Vector3(0.001f, dir.magnitude * 0.6f, 0.001f);
            }
        }

    }

    private void OnDrawGizmos() {
        if (_weights == null || _weights.Length == 0) {
            return;
        }
        for (int x = 0; x < planeGridPoints; x++) {
            for (int y = 0; y < planeGridPoints; y++) {
                int index = x + planeGridPoints * y;
                float noiseValue = _weights[index];
                if(noiseValue == -1){
                    Gizmos.color = Color.blue;
                } else{
                    if(noiseValue > 0.5f){
                        Gizmos.color = Color.black;
                    } else{
                        Gizmos.color = Color.white;
                    }
                }
                Gizmos.DrawCube(transform.position + (transform.right * (x - planeGridPoints / 2) + transform.forward * (y - planeGridPoints / 2)) * (chunkVolume.size / 100), Vector3.one * .004f);
            }
        }
    }

    int ReadSegmentCount() {
        int[] segCount = { 0 };
        ComputeBuffer.CopyCount(_segmentsBuffer, _segmentsCountBuffer, 0);
        _segmentsCountBuffer.GetData(segCount);
        return segCount[0];
    }

    void CreateBuffers() {
        _segmentsBuffer = new ComputeBuffer(5 * (planeGridPoints * planeGridPoints), Segment.SizeOf, ComputeBufferType.Append);
        _segmentsCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        _weightsBuffer = new ComputeBuffer(planeGridPoints * planeGridPoints * planeGridPoints, sizeof(float));
        // _vectorsBuffer = new ComputeBuffer(planeGridPoints * planeGridPoints, 3 * 4 * sizeof(float));
    }

    void ReleaseBuffers() {
        _segmentsBuffer.Release();
        _segmentsCountBuffer.Release();
        _weightsBuffer.Release();
        // _vectorsBuffer.Release();
    }

}
