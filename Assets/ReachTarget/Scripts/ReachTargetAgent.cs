using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SimpleVehicleController))]
public class ReachTargetAgent : Agent
{
    public Transform target;
    public Renderer ground;
    private Rigidbody m_rbody;
    private SimpleVehicleController m_controller;
    private float m_rewardScalar = 0.0001f;
    private bool m_targetReached = false;

    public Transform obstacle;
    public Transform boat;
    public Transform boatTarget;
    private bool m_obstacleReached = false;
    private float boatSpeed = 0.05f;
    private int MaxStep = 6000;

    protected Vector3 AgentVelocity
    {
        get { return m_rbody.velocity; }
    }

    protected float RewardScalar
    {
        get { return m_rewardScalar; }
    }

    void Start()
    {
        m_rbody = GetComponent<Rigidbody>();
        m_controller = GetComponent<SimpleVehicleController>();

        // If the agent's maxStep > 0, use it to calculate the
        // reward scalar. To be used for per-step rewards
        if (maxStep > 0)
        {
            m_rewardScalar = 1f / maxStep;
        }
    }

    public void FixedUpdate()
    {
        AddReward(-0.5f / MaxStep);
        CheckDistanceToPt();
        CheckDistanceToTarget();

        boatSpeed = 0;
        //boatSpeed = Random.Range(0.05f, 0.3f);
        UpdateBoat();

        // If the car falls off the platform, end episode
        if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        // Only respawn the agent if we didn't reach the target
        if (!m_targetReached || !m_obstacleReached)
            RespawnAgent();
        m_targetReached = false;
        RespawnTarget();
        RespawnObstacle();
        RespawnBoat();
    }

    public override void OnActionReceived(float[] action)
    {
        m_controller.SetSteering(action[0]);
        m_controller.SetThrottle(action[1]);
        m_controller.SetBrake(action[2]);
    }

    void OnCollisionEnter(Collision collision)
    {
        EndEpisode();
    }

    public override float[] Heuristic()
    {
        var action = new float[3];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        action[2] = Input.GetAxis("Jump");
        return action;
    }

    void CheckDistanceToTarget()
    {
        var distanceToTarget = Vector3.Distance(
            this.transform.position,
            target.transform.position
        );

        // Upon reaching the target, respawn it to a random position
        // and add reward of +1
        if (distanceToTarget < 3)
        {
            Vector3 dir = target.transform.position - this.transform.position;
            float angle = Vector3.Angle(dir, this.transform.forward);
            bool fwd = false;

            if(angle < 90f/2)
            {
                fwd = true;
            }

            m_targetReached = true;

            if(fwd)
            {
                AddReward(1f);
            }
            else
            {
                AddReward(0.5f);
            }

            EndEpisode();
        }
    }

    void RespawnAgent()
    {
        m_controller.SetThrottle(0);
        m_controller.SetSteering(0);
        m_controller.SetBrake(1);
        m_rbody.velocity = Vector3.zero;
        m_rbody.angularVelocity = Vector3.zero;
        this.transform.localPosition = new Vector3(0, 1f, 0);
        this.transform.rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
    }

    void RespawnTarget()
    {
        Vector3 extents = ground.bounds.extents - (Vector3.one * 3);
        target.localPosition = new Vector3(Random.Range(-extents.x, extents.x),
                                           0.5f,
                                           Random.Range(-extents.z, extents.z));
        target.rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
    }

    void RespawnObstacle()
    {
        Vector3 extents = ground.bounds.extents - (Vector3.one * 3);
        obstacle.localPosition = new Vector3(Random.Range(-extents.x, extents.x),
                                           0.5f,
                                           Random.Range(-extents.z, extents.z));
        obstacle.rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
    }

    void RespawnBoat()
    {
        Vector3 extents = ground.bounds.extents - (Vector3.one * 3);
        boat.localPosition = new Vector3(Random.Range(-extents.x, extents.x),
                                           0.5f,
                                           Random.Range(-extents.z, extents.z));
        boat.rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
    }

    void NewTarget()
    {
        Vector3 extents = ground.bounds.extents - (Vector3.one * 3);
        boatTarget.localPosition = new Vector3(Random.Range(-extents.x, extents.x),
                                           0.5f,
                                           Random.Range(-extents.z, extents.z));
    }

    void UpdateBoat()
    {
        Vector3 dir = boatTarget.position - boat.position;
        boat.position = Vector3.MoveTowards(boat.position, boatTarget.position, boatSpeed);
        boat.rotation = Quaternion.LookRotation(dir);
    }

    void OnTriggerEnter(Collider other)
    {
        AddReward(-0.25f);
        EndEpisode();
    }

    void CheckDistanceToPt()
    {
        var distanceToTarget = Vector3.Distance(
            boat.transform.position,
            boatTarget.transform.position
        );

        // Upon reaching the target, respawn it to a random position
        // and add reward of +1
        if (distanceToTarget < 1)
        {
            NewTarget();
        }
    }
}
