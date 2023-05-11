using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;



public class Solver_Agent : Agent
{
    /*
        VARIABLE DECLARATIONS
        All necessary variables and object declarations.
        Allows for this script to interact with external singleton instances.
    */
    [SerializeField] private Transform targetTransform;
    private Rigidbody rb = null;
    public Collider[] groundColliders = new Collider[2];
    public GameObject inter_goal;
    public GameObject platform;
    public GameObject plane;
    public GameObject envacademy;
    public float jumpTime;
    public float moveSpeed = 2;
    public float jumpHeight = 4;
    public float downForce = 111;
    public float jumpSpeed = 777;
    public float maxVelocityChange = 15f;
    private int totalInterPlats = 1;
    Vector3 jumpStartPosition;
    Vector3 jumpTargetPosition;

    public override void Initialize(){
        rb = this.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    /*
        OnEpisodeBegin()
        Called at the beginning of every episode.
        Allows for episode-dependent functinos and variables to be called.
    */
    public override void OnEpisodeBegin(){
        jumpTime = 0;
        transform.position = new Vector3(0,1,0);
        transform.eulerAngles = new Vector3(0,0,0);
        rb.velocity = Vector3.zero;
        inter_goal.GetComponent<BoxCollider>().enabled=true;
        var inter_goal_list = GameObject.FindGameObjectsWithTag("intermediate_goal");
        totalInterPlats = inter_goal_list.Length;
        foreach (var obj in inter_goal_list){
            obj.GetComponent<BoxCollider>().enabled=true;
            obj.GetComponent<MeshRenderer>().enabled=true;
        }
    }
    
    public int GetTotalInterPlats(){
        return totalInterPlats;
    }

    /*
        IsGrounded()
        Checks if the solver is on the ground
        Also checks if the solve is partially or fully on the ground
    */
    public bool IsGrounded(bool coverage){
        if (!coverage){
                groundColliders = new Collider[2];
                var collidable = gameObject;
                Physics.OverlapBoxNonAlloc(collidable.transform.position + new Vector3(0,-0.05f,0),
                new Vector3(0.95f/2f, 0.5f, 0.95f/2f), groundColliders,
                collidable.transform.rotation);
            var grounded = false;
            foreach (var col in groundColliders){
                if(col != null && col.transform != transform && 
                    (col.CompareTag("platform") || 
                    col.CompareTag("plane") || col.CompareTag("end_platform"))){
                    
                    grounded = true;
                    break;

                }
            }
            return grounded;
        } else {
            RaycastHit hit;
            Physics.Raycast(transform.position + new Vector3(0,-0.05f,0), -Vector3.up, out hit, 0.46f);

            if(hit.collider != null && (hit.collider.CompareTag("platform") || 
                hit.collider.CompareTag("plane")) && hit.normal.y > 0.95f){
                    return true;
                }
            return false;
        }

    }
    
    /*
        ApplyJump()
        Resets jump timer
    */
    void ApplyJump()
    {
        jumpTime = 0.1f;
        jumpStartPosition = rb.position;
    }

    /*
        JumpAim()
        Applies the forward force of the solver when jumping
    */
    void JumpAim(Vector3 aimPosition, Rigidbody rigbod, float targetVelocity, float maxVelocity)
    {
        var moveToTarget = aimPosition - rb.worldCenterOfMass;
        var desiredVelocity = Time.fixedDeltaTime * targetVelocity * moveToTarget;
        if(float.IsNaN(desiredVelocity.x) == false){
            rb.velocity = Vector3.MoveTowards(
                rb.velocity, desiredVelocity, maxVelocity);
        }
    }

    /*
        ObsNorm()
        Normalizes a value based on given min and max values
    */
    public float ObsNorm(float RealVal, float MinVal, float MaxVal){
        var normalizedValue = 0.0f;

        normalizedValue = (RealVal - MinVal)/(MaxVal - MinVal);

        return normalizedValue;
    }

    /*
        CollectObservations()
        Called every lifecycle loop.
        Adds normalised observations as inputs to the neural network.
    */
    public override void CollectObservations(VectorSensor sensor){
        var agentPosition = rb.position;
        var latestPlatObs = envacademy.GetComponent<EnvAcademy>().GetEnvLatest().transform.position;
        Quaternion agentRotation = transform.rotation;
        Vector3 normalizedRot = agentRotation.eulerAngles / 180.0f - Vector3.one;

        sensor.AddObservation(normalizedRot);

        sensor.AddObservation(latestPlatObs.x/40f);
        sensor.AddObservation(latestPlatObs.y/40f);
        sensor.AddObservation(latestPlatObs.z/40f); 

        sensor.AddObservation(agentPosition.x/40f);
        sensor.AddObservation(agentPosition.y/40f);
        sensor.AddObservation(agentPosition.z/40f);

        sensor.AddObservation(targetTransform.position.x/15f);
        sensor.AddObservation(ObsNorm(targetTransform.position.y, 10f, 15f));
        sensor.AddObservation(targetTransform.position.z/15f);
        sensor.AddObservation(IsGrounded(true) ? 1 : 0);

        sensor.AddObservation(rb.velocity.x/30f);
        sensor.AddObservation(rb.velocity.z/30f);

    }

    /*
        MoveAgent()
        Takes a discrete action buffer and applies forces
        based on the action values
    */
    public void MoveAgent(ActionSegment<int> actionsChosen) {

        AddReward(-0.0005f);

        var fullyGrounded = IsGrounded(false);
        var partiallyGrounded = IsGrounded(true);

        var mainDirection = Vector3.zero;
        var rotationDirection = Vector3.zero;
        var forwardAction = actionsChosen[0];
        var rotateAction = actionsChosen[1];
        var sideAction = actionsChosen[2];
        var jumpAction = actionsChosen[3];

        
        if(forwardAction == 1){
            mainDirection = (fullyGrounded ? 1f : 0.5f) * 1f * transform.forward;
        } else if (forwardAction == 2){
            mainDirection = (fullyGrounded ? 1f : 0.5f) * -1f * transform.forward;
        }
        if(rotateAction == 1){
            rotationDirection = (fullyGrounded ? 1f : 0.5f) * transform.up * -1f;
        } else if (rotateAction == 2){
            rotationDirection = (fullyGrounded ? 1f : 0.5f) * transform.up * 1f;
        }

        if(jumpAction == 1){
            if((jumpTime <= 0f) && partiallyGrounded){
                ApplyJump();
            }
        }

        transform.Rotate(rotationDirection, Time.fixedDeltaTime * 300f);
        rb.AddForce(mainDirection * moveSpeed, ForceMode.VelocityChange);

        if(jumpTime > 0f){
            jumpTargetPosition = 
                new Vector3(rb.position.x,
                    jumpStartPosition.y + jumpHeight,
                    rb.position.z) + mainDirection;
            JumpAim(jumpTargetPosition, rb, jumpSpeed, maxVelocityChange);
        }

        if(!(jumpTime > 0f) && !fullyGrounded){
            rb.AddForce(
                Vector3.down * downForce, ForceMode.Acceleration);
        }
        jumpTime -= Time.fixedDeltaTime;
        

    }

    /*
        OnActionReceived()
        Called every lifecycle loop.
        Collects action outputs from the neural network.
        Calls the MoveAgent() function.
    */
    public override void OnActionReceived(ActionBuffers actions) {

        MoveAgent(actions.DiscreteActions);

        if (transform.position.y < -4.0f ){
            Debug.Log("Out of bounds: -0.5 Reward");
            
            AddReward(-0.5f);
            IndicateFail();
            EndEpisode();
        }

    }

    /*
        IndicateFail()
        Indicates if the solver has gone out of bounds
        Calls an SolverFail() function in the EnvironmentAcademy Script
    */
    public void IndicateFail(){
    
        envacademy.GetComponent<EnvAcademy>().SolverFail();
    }

    /*
        Heuristic()
        Allows for the developer to control the solver using
        WASD and Space
    */
    public override void Heuristic(in ActionBuffers actionsOut){

        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[1] = 2;
        }
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        discreteActionsOut[3] = Input.GetKey(KeyCode.Space) ? 1 : 0;    
    }
 
    /*
        OnTriggerEnter()
        Checks solver collisions with intermediate goals and end goals
        Applies reward accordingly
    */
    private void OnTriggerEnter(Collider other) {
        if(other.TryGetComponent<Goal_Area>(out Goal_Area goal_area)){
            AddReward(+1f);
            Debug.Log("****************************** Add 1 Reward ******************************");
            EndEpisode();
        }
        if(other.TryGetComponent<Intermediate_Goal>(out Intermediate_Goal intermediate_goal)){
            AddReward(+0.4f);
            intermediate_goal.GetComponent<BoxCollider>().enabled=false;
            intermediate_goal.GetComponent<MeshRenderer>().enabled=false;
            Debug.Log("----------------------------- Add 0.4 Reward -----------------------------");
            
        }
    }
}
 