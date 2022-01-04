using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SplineInterpolation
{
    static float CubicBasisFunc0(float t)
    {
        return 1f / 6f * (float)Math.Pow(1 - t, 3);
    }

    static float CubicBasisFunc1(float t)
    {
        return 1f / 6f * (3 * (float)Math.Pow(t, 3) - 6 * (float)Math.Pow(t, 2) + 4);
    }

    static float CubicBasisFunc2(float t)
    {
        return 1f / 6f * (-3 * (float)Math.Pow(t, 3) + 3 * (float)Math.Pow(t, 2) + 3 * t + 1);
    }

    static  float CubicBasisFunc3(float t)
    {
        return 1f / 6f * (float)Math.Pow(t, 3);
    }


    public static Vector3 CubicUBSFunc(float t, Vector3[] c)
    {
        return c[0] * CubicBasisFunc0(t) + c[1] * CubicBasisFunc1(t) + c[2] * CubicBasisFunc2(t) + c[3] * CubicBasisFunc3(t);
    }


    //By default, open knot vectors are used, but this may need to be changed.
    public static Vector3[] ApplyCubicUBSFuncToVectors(Vector3[] cps, int curve_accuracy)
    {
        int _open_knot_vector_offset_number = 3;
        Vector3[] open_knot_cps = new Vector3[cps.Length + _open_knot_vector_offset_number * 2];

        for (int i = 0; i < _open_knot_vector_offset_number; i++)
        {
            open_knot_cps[i] = cps[0];
            open_knot_cps[cps.Length + _open_knot_vector_offset_number + i]
                = cps.Last();
        }

        for (int i = _open_knot_vector_offset_number; i < cps.Length + _open_knot_vector_offset_number; i++)
        {
            open_knot_cps[i] = cps[i - _open_knot_vector_offset_number];
        }



        int knot_count = open_knot_cps.Length;
        int curve_segment_count = knot_count - 3;

        Vector3[] ubs_points = Enumerable.Empty<Vector3>().ToArray();
        for (int i = 0; i < curve_segment_count; i++)
        {
            var points = Enumerable
                        .Range(0, curve_accuracy)
                        .Select(c => c / ((float)curve_accuracy))
                        .Select(c => SplineInterpolation.CubicUBSFunc(c, open_knot_cps.Skip(i).Take(4).ToArray()))
                        .ToArray();
            ubs_points = ubs_points.Concat(points).ToArray();
        }

        return ubs_points;
    }
}
