using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutonomousAgent : AIAgent
{
    public AIPerception perception = null;
    public AIPerception seekPerception = null;
    public AIPerception fleePerception = null;
    public AIPerception flockPerception = null;

    private void Update()
    {
        if (seekPerception != null)
        {
            var gameObjects = seekPerception.GetGameObjects();
            if (gameObjects.Length > 0)
            {
                Vector3 force = Seek(gameObjects[0]);
                movement.ApplyForce(force);
            }
        }

        if (fleePerception != null)
        {
            var gameObjects = fleePerception.GetGameObjects();
            if (gameObjects.Length > 0)
            {
                Vector3 force = Flee(gameObjects[0]);
                movement.ApplyForce(force);
            }
        }

        if(flockPerception != null)
        {
            var gameObjects = flockPerception.GetGameObjects();
            if(gameObjects.Length > 0)
            {
                movement.ApplyForce(Cohesion(gameObjects));
                movement.ApplyForce(Separation(gameObjects, 2));
                movement.ApplyForce(Alignment(gameObjects));
            }
        }
        transform.position = Utilities.Wrap(transform.position, new Vector3(-20,-2,-20), new Vector3(20,2,20));
    }

    private Vector3 Seek(GameObject target)
    {
        Vector3 direction = target.transform.position - transform.position;
        return GetSteeringForce(direction);
    }

    private Vector3 Flee(GameObject target)
    {
        Vector3 direction = transform.position - target.transform.position;
        return GetSteeringForce(direction);
    }

    private Vector3 Cohesion(GameObject[] neighbours)
    {
        Vector3 positions = Vector3.zero;
        foreach (var neighbour in neighbours)
        {
            positions += neighbour.transform.position;
        }
        Vector3 center = positions / neighbours.Length;
        Vector3 direction = center - transform.position;
        Vector3 force = GetSteeringForce(direction);
        return force;
    }

    private Vector3 Separation(GameObject[] neighbours, float radius)
    {
        Vector3 separation = Vector3.zero;
        foreach (var neighbour in neighbours)
        {
            Vector3 direction = (transform.position - neighbour.transform.position);
            if(direction.magnitude < radius)
            {
                separation += direction / direction.sqrMagnitude;
            }
        }
        Vector3 force = GetSteeringForce(separation);
        return force;
    }

    private Vector3 Alignment(GameObject[] neighbours)
    {
        Vector3 velocities = Vector3.zero;
        foreach (var neighbour in neighbours)
        {
            velocities += neighbour.GetComponent<AIAgent>().movement.Velocity;
        }

        Vector3 averageVelocity = velocities / neighbours.Length;
        Vector3 force = GetSteeringForce(averageVelocity);
        return force;
    }
    public Vector3 GetSteeringForce(Vector3 direction)
    {
        Vector3 desired = direction.normalized * movement.maxSpeed;
        Vector3 steer = desired - movement.Velocity;
        Vector3 force = Vector3.ClampMagnitude(steer, movement.maxForce);

        return force;
    }
}
