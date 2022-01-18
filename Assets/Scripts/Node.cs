using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EoLとEILをベースとするノードのクラス
/// </summary>
public interface INode
{
    float U
    {
        get; set;
    }
    Vector3 X
    {
        get; set;
    }

    INode C
    {
        get; set;
    }
    INode A
    {
        get; set;
    }
}


/// <summary>
/// EoL: Eulerian-on-Lagrangian
/// </summary>
public class EoL : INode
{
    private float _eulerian;
    private Vector3 _lagrangian;

    public EoL(float u, Vector3 x, INode prev, INode next)
    {
        _eulerian = u;
        _lagrangian = x;
        A = prev;
        C = next;
    }

    public float U
    {
        get { return _eulerian; }
        set { _eulerian = value; }
    }
    public Vector3 X
    {
        get { return _lagrangian; }
        set { _lagrangian = value; }
    }

    public INode A { get; set; }
    public INode C { get; set; }


    public static explicit operator EIL(EoL node)
    {
        var x_ab = node.A.X - node.X;
        var x_bc = node.X - node.C.X;
        var x_ac = node.A.X - node.C.X;

        float u =
            Mathf.Max(
                node.C.U,
                (Vector3.Dot(x_ac, x_bc) * node.A.U + Vector3.Dot(x_ac, x_ab) * node.C.U) / Vector3.Dot(x_ac, x_ac)
                );
        return new EIL(u, node.A, node.C);
    }
}

/// <summary>
/// EIL: Eulerian with Interpolated Lagrangian
/// </summary>
public class EIL : INode
{
    private float _eulerian;

    public EIL(float u, INode prev, INode next)
    {
        _eulerian = u;
        A = prev;
        C = next;
    }


    public float U
    {
        get { return _eulerian; }
        set { _eulerian = value; }
    }
    public Vector3 X
    {
        get
        {
            return
                ((C.U - U) * A.X + (U - A.U) * C.X) / (C.U - A.U);
        }
        set
        {
        }
    }

    public INode A { get; set; }
    public INode C { get; set; }


    public static explicit operator EoL(EIL node)
    {
        var x_ab_magnitude = Vector3.Distance(node.A.X, node.X);
        var x_bc_magnitude = Vector3.Distance(node.X, node.C.X);
        float u =
            (x_bc_magnitude * node.A.U + x_ab_magnitude * node.C.U) / (x_ab_magnitude + x_bc_magnitude);
        return new EoL(u, node.X, node.A, node.C);
    }
}