using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    ComputeBuffer _weightsBuffer;
    public ComputeShader NoiseShader;

    public float size;
    [SerializeField, Range(0f, 20f)] public float scale = 10;
    [SerializeField] public float amplitude = 5;
    [SerializeField] public float frequency = 0.005f;

    public Vector3 Offset;

    [SerializeField] public Vector3 planeForward;
    [SerializeField] public Vector3 planeRight;
    [SerializeField] public Vector3 planePosition;

    [SerializeField] bool isPlane;

    public int function;


    private void Awake() {
        CreateBuffers();
    }

    private void OnDestroy() {
        ReleaseBuffers();
    }

    public float[] GetNoise() {
        float[] noiseValues =
            new float[GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk];

        NoiseShader.SetBuffer(0, "_Weights", _weightsBuffer);

        NoiseShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk);
        NoiseShader.SetInt("_IsPlane", 0);
        NoiseShader.SetInt("_Function", function);
        NoiseShader.SetFloat("_Scale", scale);
        NoiseShader.SetFloat("_Amplitude", amplitude);
        NoiseShader.SetFloat("_Frequency", frequency);
        NoiseShader.SetVector("_Offset", Offset / (size / 100));

        NoiseShader.Dispatch(
            0, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads
        );

        _weightsBuffer.GetData(noiseValues);

        return noiseValues;
    }

    public float[] GetNoisePlane(int planeGridPoints) {
        float[] noiseValues =
            new float[planeGridPoints * planeGridPoints];

        NoiseShader.SetBuffer(1, "_Weights", _weightsBuffer);

        NoiseShader.SetInt("_ChunkSize", planeGridPoints);
        NoiseShader.SetInt("_IsPlane", 1);
        NoiseShader.SetFloat("_Scale", scale);
        NoiseShader.SetFloat("_Amplitude", amplitude);
        NoiseShader.SetFloat("_Frequency", frequency);
        NoiseShader.SetVector("_Offset", Offset / (size / 100));
        NoiseShader.SetVector("_PlaneForward", planeForward);
        NoiseShader.SetVector("_PlaneRight", planeRight);


        NoiseShader.Dispatch(
            1, planeGridPoints / GridMetrics.NumThreads, planeGridPoints / GridMetrics.NumThreads, 1);

        _weightsBuffer.GetData(noiseValues);

        return noiseValues;
    }

    void CreateBuffers() {
        if(isPlane){
            int planeGridPoints = GridMetrics.PointsPerChunk * 2 + 16;
            _weightsBuffer = new ComputeBuffer(
                planeGridPoints * planeGridPoints, sizeof(float)
            );
        } else{
            _weightsBuffer = new ComputeBuffer(
                GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk, sizeof(float)
            );
        }
        
    }

    void ReleaseBuffers() {
        _weightsBuffer.Release();
    }
}
