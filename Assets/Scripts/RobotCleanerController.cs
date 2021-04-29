using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// A Robot Cleaning Machine Learning Agent
/// </summary>
public class RobotCleanerController : Agent
{
    [Tooltip("The force for moving forward and back")]
    public float moveForce = 1f;

    [Tooltip("The force for rotation")]
    public float rotationForce = 1f;

    [Tooltip("For wether the agent is in training or play mode")]
    public bool trainingMode;

    [Tooltip("The controller for the objects that the robot is collecting")]
    public ObjectController objController;

    [Tooltip("This collider will be the range of which the clearing we be randomly put when in training")]
    public Collider coll;

    //RidgidBody
    private Rigidbody rigidBody;

    //The closest object to the collector
    private GameObject closestObject = null;

    private bool freshAgent = true;

    private float PreviousDistance = 0;

    /// <summary>
    /// This is for agent initalization
    /// </summary>
    public override void Initialize()
    {
        rigidBody = GetComponent<Rigidbody>();

        if (!trainingMode) MaxStep = 0;
    }

    /// <summary>
    /// Called when actions are received from a player input or neural network
    /// action[0] = movement (+1 = foward, -1 = backwards)
    /// action[1] = rotation (+1 = left, -1 = right)
    /// </summary>
    /// <param name="action"></param>
    public override void OnActionReceived(float[] action)
    {
        Vector3 movement = action[0] * transform.forward;
        rigidBody.AddForce(movement * moveForce);

        rigidBody.AddTorque(transform.up * rotationForce * action[1]);

    }

    /// <summary>
    /// Reset the agent when a new episode begins
    /// </summary>
    public override void OnEpisodeBegin()
    {

        if (trainingMode) {
            //Randomly distribute objects
            objController.DistributeObjects();

            //Reset Previous
            freshAgent = true;

            //Resets velocity
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;

            //Move cleaning bot to a random place
            MoveToRandomPlace();

 
            if (UnityEngine.Random.value > 0.5f)
            {
                objController.MoveRandomObjectToLocation(gameObject.transform.forward.normalized * 2 + gameObject.transform.position);
            }
        }

        //Update the near object
        UpdateNearestObject();
    }

    /// <summary>
    /// Collection information from the environment
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
        if (closestObject == null)
        {
            sensor.AddObservation(new float[8]);
            return;
        }
        
        //The Agent's local rotation (4 observations)
        sensor.AddObservation(transform.localRotation.normalized);

        //Vector from the bot to the nearest object
        Vector3 toObject = closestObject.transform.position - gameObject.transform.position;

        //This is 3 observations, direction to object
        sensor.AddObservation(toObject.normalized);

        //This is 1 observation
        float dotProduct = Vector3.Dot(gameObject.transform.forward, toObject.normalized);
        sensor.AddObservation(dotProduct);

        //This is 1 observation, distance to object
        sensor.AddObservation(toObject.magnitude);

        if (trainingMode)
        {
            if (freshAgent)
            {
                freshAgent = false;
                PreviousDistance = toObject.magnitude;
            }

            AddReward(0.02f * dotProduct);

            AddReward(0.5f * (PreviousDistance - toObject.magnitude));

            PreviousDistance = toObject.magnitude;
        }

    }

    /// <summary>
    /// This is called when heuristic of the agent is set to
    /// true. This allows player to control it.
    /// </summary>
    /// <param name="actionsOut"></param>
    public override void Heuristic(float[] actionsOut)
    {
        float move = 0f;
        float rotate = 0f;


        //For forward and backwards movement
        if (Input.GetKey(KeyCode.W)) move = 1f;
        else if (Input.GetKey(KeyCode.S)) move = -1f;

        //For rotating left and right
        if (Input.GetKey(KeyCode.A)) rotate = -1f;
        else if (Input.GetKey(KeyCode.D)) rotate = 1f;

        //Returns the actions

        actionsOut[0] = move;
        actionsOut[1] = rotate;


    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("ObjectToBeCollected"))
        {
            objController.CollectedObject(collider.gameObject);

            if (trainingMode)
            {
                AddReward(0.2f);
                freshAgent = true;
            }

            UpdateNearestObject();

        }
    }

    private void UpdateNearestObject()
    {
        closestObject = objController.GetClosestObject(gameObject.transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (trainingMode && collision.collider.CompareTag("Boundary"))
        {
            // Collided with a area boundary, give a negative reward
            AddReward(-0.5f);
        }
    }

    private void MoveToRandomPlace()
    {
        gameObject.transform.position = new Vector3(
            Random.Range(coll.bounds.min.x, coll.bounds.max.x),
            Random.Range(coll.bounds.min.y, coll.bounds.max.y),
            Random.Range(coll.bounds.min.z, coll.bounds.max.z));
    }

    // Start is called before the first frame update
    void Update()
    {
        if (closestObject != null)
        {
            Debug.DrawLine(gameObject.transform.position, closestObject.transform.position, Color.green);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (closestObject == null)
        {
            UpdateNearestObject();
        }
    }
}
