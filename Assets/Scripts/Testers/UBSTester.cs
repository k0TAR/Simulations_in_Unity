using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class UBSTester : MonoBehaviour
{
    [SerializeField] GameObject _control_point_parent;
    [SerializeField, Range(1, 100)] int _curve_accuracy = 10;
    [SerializeField] bool _make_uniform = true;
    [SerializeField, Range(0, 10)] int _open_knot_vector_offset_number = 3;

    private Vector3[] _cps = null;
    private Vector3[] _curve_points = null;
    private LineRenderer _lineRenderer = null;

    private GameObject _parent_linerenderers_GB;

    private void OnValidate()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _curve_points = null;
        _cps = null;

        Transform[] cps_Transform = _control_point_parent.GetComponentsInChildren<Transform>();
        cps_Transform = cps_Transform.Where((val, index) => index != 0).ToArray();
        //_cps = cps_Transform.Select(c => c.position).ToArray();




        if (_make_uniform)
        {
            _cps = new Vector3[cps_Transform.Length];

            for (int i = 0; i < cps_Transform.Length; i++)
            {
                _cps[i] = cps_Transform[i].position;

            }
        } 
        else
        {
            _cps = new Vector3[cps_Transform.Length + _open_knot_vector_offset_number * 2];

            for(int i = 0; i < _open_knot_vector_offset_number; i++)
            {
                _cps[i] = cps_Transform[0].position;
                _cps[cps_Transform.Length + _open_knot_vector_offset_number + i ] 
                    = cps_Transform.Last().position;
            }

            for (int i = _open_knot_vector_offset_number; i < cps_Transform.Length + _open_knot_vector_offset_number; i++)
            {
                _cps[i] = cps_Transform[i - _open_knot_vector_offset_number].position;
            }

        }



        int knot_count = _cps.Length;
        int curve_segment_count = knot_count - 3;
        Material[] curve_segment_materials = new Material[4];
        Color[] colors = new Color[] { Color.red, Color.green, Color.blue, Color.gray };
        for (int i = 0; i < 4; i++)
        {
            curve_segment_materials[i] = new Material(Shader.Find("Sprites/Default"));
            curve_segment_materials[i].color = colors[i];

        }


        _parent_linerenderers_GB = new GameObject();
        _parent_linerenderers_GB.name = this.gameObject.name + "LINERENDERER";
        GameObject[] line_renderers_gbs = new GameObject[curve_segment_count];
        LineRenderer[] line_renderers = new LineRenderer[curve_segment_count];
        for (int i = 0; i < curve_segment_count; i++)
        {
            line_renderers_gbs[i] = new GameObject();
            line_renderers_gbs[i].transform.parent = _parent_linerenderers_GB.transform;
            line_renderers[i] = line_renderers_gbs[i].AddComponent<LineRenderer>();
        }



        //_curve_points = Enumerable.Empty<Vector3>().ToArray();
        for (int i = 0; i < curve_segment_count; i++)
        {
            var points = Enumerable
                        .Range(0, _curve_accuracy)
                        .Select(c => c / ((float)_curve_accuracy) )
                        .Select(c => SplineInterpolation.CubicUBSFunc(c, _cps.Skip(i).Take(4).ToArray()))
                        .ToArray();
            //_curve_points = _curve_points.Concat(points).ToArray();
            if(i < curve_segment_count - 1)
            {
                points = points.Concat(new Vector3[] { SplineInterpolation.CubicUBSFunc(0, _cps.Skip(i + 1).Take(4).ToArray()) }).ToArray();
            }
            line_renderers[i].positionCount = points.Length;
            line_renderers[i].widthMultiplier = .5f;
            line_renderers[i].SetPositions(points);
            line_renderers[i].material = curve_segment_materials[i % 4];
        }


        //_lineRenderer.positionCount = _curve_points.Length;
        //_lineRenderer.widthMultiplier = .5f;
        //_lineRenderer.SetPositions(_curve_points);
    }
}
