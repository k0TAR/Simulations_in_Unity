using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

public class PbdRigidBodyManager : MonoBehaviour
{
    [SerializeField] private PbdRigidBody rigidBodyPrefab;

    private readonly List<PbdRigidBody> _rigidBodies = new List<PbdRigidBody>();

    private Random _random;

    private void Start()
    {
        _random = new Random(1);
        for (var i = 0; i < 20; ++i)
        {
            var rigidBody = Instantiate(rigidBodyPrefab);
            rigidBody.transform.position = _random.NextFloat3(-1.5f, 1.5f);
            _rigidBodies.Add(rigidBody);
            rigidBody.transform.SetParent(transform, false);
        }
    }

    private void FixedUpdate()
    {
        for (var i = 0; i < _rigidBodies.Count - 1; ++i)
        {
            for (var j = i; j < _rigidBodies.Count; ++j)
            {
                var rigidBody1 = _rigidBodies[i];
                var rigidBody2 = _rigidBodies[j];
                if (rigidBody1 == rigidBody2)
                {
                    continue;
                }

                var a = rigidBody1.transform.position;
                var b = rigidBody2.transform.position;

                var ab = b - a;

                var abMagnitude = ab.magnitude;
                var abDirection = ab.normalized;
                if (abDirection == Vector3.zero) abDirection = Vector3.up;

                if (abMagnitude < 1)
                {
                    a -= (1 - abMagnitude) * abDirection / 2;
                    b += (1 - abMagnitude) * abDirection / 2;
                }

                a = math.clamp(a, -2, 2);
                b = math.clamp(b, -2, 2);

                rigidBody1.transform.position = a;
                rigidBody2.transform.position = b;
            }
        }
    }
}
