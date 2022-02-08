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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            foreach (var rigidbody in _rigidBodies)
            {
                rigidbody.transform.position += (Vector3)_random.NextFloat3Direction() * 0.1f;
            }
        }
    }

    private void FixedUpdate()
    {
        foreach (var rigidbody in _rigidBodies)
        {
            var position = rigidbody.transform.position;
            var velocity = (position - rigidbody.PrevPosition) / Time.deltaTime;

            velocity += Physics.gravity * Time.deltaTime;

            rigidbody.PrevPosition = position;
            position += velocity * Time.deltaTime;

            rigidbody.transform.position = position;
        }

        for (var i = 0; i < _rigidBodies.Count - 1; ++i)
        {
            for (var j = i; j < _rigidBodies.Count; ++j)
            {
                var rigidBodyA = _rigidBodies[i];
                var rigidBodyB = _rigidBodies[j];
                if (rigidBodyA == rigidBodyB)
                {
                    continue;
                }

                Vector3 a = rigidBodyA.transform.position;
                Vector3 b = rigidBodyB.transform.position;

                Vector3 ab = b - a;

                float abMagnitude = ab.magnitude;
                Vector3 abDirection = ab.normalized;
                if (abDirection == Vector3.zero) abDirection = Vector3.up;

                if (abMagnitude < 1)
                {
                    a -= (1 - abMagnitude) * abDirection / 2;
                    b += (1 - abMagnitude) * abDirection / 2;
                }

                a = math.clamp(a, -2, 2);
                b = math.clamp(b, -2, 2);

                rigidBodyA.transform.position = a;
                rigidBodyB.transform.position = b;
            }
        }
    }
}
