using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EoLとEILをベースとするノードのクラス
/// </summary>
public interface INode
{
    public float U
    {
        get; set;
    }
    public Vector3 X
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

    public EoL(float u, Vector3 x)
    {
        _eulerian = u;
        _lagrangian = x;
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
}

/// <summary>
/// EIL: Eulerian with Interpolated Lagrangian
/// </summary>
public class EIL : INode
{
    private float _eulerian;
    //private Vector3 _lagrangian;

    private INode _next;
    private INode _prev;

    public EIL(float u, EoL prev, EoL next)
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
                (C.U - U) / (C.U - A.U) * A.X +
                (U - A.U) / (C.U - A.U) * C.X;
        }
        set
        {

        }
    }

    public INode C
    {
        get { return _next; }
        set { _next = value; }
    }
    public INode A
    {
        get { return _prev; }
        set { _prev = value; }
    }


}