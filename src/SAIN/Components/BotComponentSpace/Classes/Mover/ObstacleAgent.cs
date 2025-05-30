﻿using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.Mover
{
  public class ObstacleAgent : MonoBehaviour
  {
    private float CarvingTime = 0.5f;
    private float CarvingMoveThreshold = 0.1f;

    private NavMeshAgent? Agent;
    private NavMeshObstacle? Obstacle;

    private float LastMoveTime;
    private Vector3 LastPosition;

    public void Awake()
    {
      Agent = GetComponent<NavMeshAgent>();
      Obstacle = GetComponent<NavMeshObstacle>();

      Obstacle.enabled = false;
      Obstacle.carveOnlyStationary = false;
      Obstacle.carving = true;

      LastPosition = transform.position;
    }

    public void Update()
    {
      if (Vector3.Distance(LastPosition, transform.position) > CarvingMoveThreshold)
      {
        LastMoveTime = Time.time;
        LastPosition = transform.position;
      }
      if (LastMoveTime + CarvingTime < Time.time)
      {
        Agent.enabled = false;
        Obstacle.enabled = true;
      }
    }

    public void SetDestination(Vector3 Position)
    {
      Obstacle.enabled = false;

      LastMoveTime = Time.time;
      LastPosition = transform.position;

      StartCoroutine(MoveAgent(Position));
    }

    private IEnumerator MoveAgent(Vector3 Position)
    {
      yield return null;
      Agent.enabled = true;
      Agent.SetDestination(Position);
    }
  }
}
