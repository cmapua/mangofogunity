using System;
using MangoFog;
using UnityEngine;

public class PositionTester : MonoBehaviour
{
    [SerializeField]
    private MangoFogChunk _chunk;

    [SerializeField]
    private Vector3 _size = Vector3.one;

    [SerializeField]
    private float _sphereRadius = 1f;

    [SerializeField]
    private int _spacing = 1;
    
    [SerializeField]
    private int _height = 0;

    [SerializeField]
    private Vector3 _offset;

    private Vector3 _lastPosition;

    private void Start()
    {
        _chunk = MangoFogInstance.Instance.GetChunkThatEnclosesPosition(transform.position);
        _lastPosition = transform.position;
    }

    private void Update()
    {
        if ((transform.position - _lastPosition).sqrMagnitude > 1)
        {
            _lastPosition = transform.position;
            
            SetHeight(transform.position, _size, _height);
            
            Debug.Log("SetHeight call");
        }
    }
    
    private void SetHeight(Vector3 boxCenter, Vector3 boxSize, int height)
    {
        // var centerClamped = new Vector3(
        //     Mathf.RoundToInt(boxCenter.x), 
        //     Mathf.RoundToInt(boxCenter.y), 
        //     Mathf.RoundToInt(boxCenter.z));
        
        var halfSize = boxSize * 0.5f;
        var minWorld = boxCenter - halfSize;
        var maxWorld = boxCenter + halfSize;

        var minX = Mathf.RoundToInt(minWorld.x);
        var maxX = Mathf.RoundToInt(maxWorld.x);
        var minZ = Mathf.RoundToInt(minWorld.z);
        var maxZ = Mathf.RoundToInt(maxWorld.z);

        var worldHeight = _chunk.WorldToGridHeight(boxCenter.y + height);
        var spacing = Mathf.Max(1, _spacing);

        for (var z = minZ; z <= maxZ; z += spacing)
        for (var x = minX; x <= maxX; x += spacing)
        {
            var p = new Vector3(x, boxCenter.y, z);
            p = RotatePointAroundPivot(p, boxCenter, transform.rotation);
            _chunk.SetHeight(p + _offset, worldHeight);
        }
    }

    private void DrawGrid(Vector3 boxCenter, Vector3 boxSize)
    {
        // var centerClamped = new Vector3(
        //     Mathf.RoundToInt(boxCenter.x), 
        //     Mathf.RoundToInt(boxCenter.y), 
        //     Mathf.RoundToInt(boxCenter.z));
        
        var halfSize = boxSize * 0.5f;
        var minWorld = boxCenter - halfSize;
        var maxWorld = boxCenter + halfSize;

        var minX = Mathf.RoundToInt(minWorld.x);
        var maxX = Mathf.RoundToInt(maxWorld.x);
        var minZ = Mathf.RoundToInt(minWorld.z);
        var maxZ = Mathf.RoundToInt(maxWorld.z);
        
        var spacing = Mathf.Max(1, _spacing);

        for (var z = minZ; z <= maxZ; z += spacing)
        for (var x = minX; x <= maxX; x += spacing)
        {
            var p = new Vector3(x, boxCenter.y, z);
            p = RotatePointAroundPivot(p, boxCenter, transform.rotation);
            Gizmos.DrawSphere(p, _sphereRadius);
        }
    }
    
    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        return rotation * (point - pivot) + pivot;
    }

    private void OnDrawGizmos()
    {
        //if (!Application.isPlaying || !_chunk) return;
        var origMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        var s = new Vector3(
            _size.x / transform.lossyScale.x, 
            _size.y / transform.lossyScale.y, 
            _size.z / transform.lossyScale.z);
        Gizmos.DrawWireCube(Vector3.zero, s);
        Gizmos.matrix = origMatrix;
        
        DrawGrid(transform.position, _size);
    }
}