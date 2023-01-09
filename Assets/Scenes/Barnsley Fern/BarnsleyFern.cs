using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

public class BarnsleyFern : MonoBehaviour
{
const int numberOfPoints = 20000;
    public GameObject prefab;
    private NativeArray<int> randomValues = new(numberOfPoints, Allocator.Persistent);
    private NativeArray<Vector2> points = new(numberOfPoints, Allocator.Persistent);
    private TransformAccessArray transformAccessArray;
    private UpdateTransformJob updateTransformJob;
    private JobHandle updateTransformJobHandle;
    private UpdatePointsJob updatePointsJob;
    void Start()
    {
        transformAccessArray = new(numberOfPoints);
        // generate 1000 cubes
        for (int i = 0; i < randomValues.Length; i++)
        {
            // instantiate a cube
            GameObject cube = Instantiate(prefab);
            randomValues[i] = Random.Range(0, 100);
            points[i] = Vector2.zero;
            transformAccessArray.Add(cube.transform);
        }
    }

    void OnDestroy()
    {
        updateTransformJobHandle.Complete();
        transformAccessArray.Dispose();
        randomValues.Dispose();
        points.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        updatePointsJob = new UpdatePointsJob() { points = points, randomValues = randomValues };
        updatePointsJob.Run(numberOfPoints);
        points = updatePointsJob.points;

        updateTransformJob = new UpdateTransformJob() { points = points };
        updateTransformJobHandle = updateTransformJob.Schedule(transformAccessArray);
        updateTransformJobHandle.Complete();
    }

    [BurstCompile]
    struct UpdatePointsJob : IJobFor
    {
        public NativeArray<Vector2> points;
        [ReadOnly]
        public NativeArray<int> randomValues;
        public void Execute(int index)
        {
            var point = index == 0 ? Vector2.zero : points[index - 1];
            // generate a random number between 0 and 100
            var r = randomValues[index];
            var nextPoint = new Vector2(0, 0);
            // if r is less than 1, do this
            if (r < 1)
            {
                nextPoint = new Vector2(0, 0.16f) * point;
            }
            // if r is between 1 and 86, do this
            else if (r < 86)
            {
                nextPoint.x = 0.85f * point.x + 0.04f * point.y;
                nextPoint.y = -0.04f * point.x + 0.85f * point.y + 1.60f;
            }
            // if r is between 86 and 93, do this
            else if (r < 93)
            {
                nextPoint.x = 0.20f * point.x + -0.26f * point.y;
                nextPoint.y = 0.23f * point.x + 0.22f * point.y + 1.60f;
            }
            // if r is between 93 and 100, do this
            else
            {
                nextPoint.x = -0.15f * point.x + 0.28f * point.y;
                nextPoint.y = 0.26f * point.x + 0.24f * point.y + 0.44f;
            }
            // move the cube to the new point
            points[index] = nextPoint;
        }
    }
    
    [BurstCompile]
    struct UpdateTransformJob : IJobParallelForTransform
    {

        [ReadOnly]
        public NativeArray<Vector2> points;
        public void Execute(int index, TransformAccess transform)
        {
            transform.position = points[index] * 20;
        }
    }

}
