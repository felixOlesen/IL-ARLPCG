using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class ARLPCG : Agent
{
    /*
    VARIABLE DECLARATIONS
    All necessary variables and object declarations.
    Allows for this script to interact with external singleton instances.
    */
    public GameObject endPlatform;
    public GameObject intermediatePlatform;
    private GameObject firstPlatform;
    public float auxInput = 1;
    private GameObject latestPlatform;
    private GameObject newLatestPlatform;
    private GameObject oldLatestPlatform;
    private float internalReward;
    private float externalReward;
    public bool spawnDone;
    public GameObject academyEnv;

    /*
        OnEpisodeBegin()
        Called at the beginning of every episode.
        Allows for episode-dependent functinos and variables to be called.
    */
    public override void OnEpisodeBegin(){
        spawnDone = false;
    }

    /*
        PlayerHasProgressed()
        Called every FixedUpdate by the environment controller.
        Firstly checks the latest platform if the solver has collided with it.
        Secondly returns a boolean value: True is collided, False if not collided.
    */
    public bool PlayerHasProgressed(){
        var playerProgressed = latestPlatform.GetComponent<Intermediate_Platform>().getSolverCollision();
        return playerProgressed;
    }

    /*
        SpawnPlatform()
        Called every action.
        Takes an action buffer and scales it to the correct platform dimensions.
        Secondly returns a boolean value: True is collided, False if not collided.
    */
    public void SpawnPlatform(ActionSegment<float> actionsChosen){
        //Save previous platform
        oldLatestPlatform = latestPlatform;
        var previousLatestPlatPos = oldLatestPlatform.transform.position;
        
        //Retrieve scaled actions from actions buffer
        var platForwardAction = ScaleAction(actionsChosen[0], 5.0f, 10.0f);
        var platRotationAction = ScaleAction(actionsChosen[1], -90.0f, 90.0f);
        var platHeightAction = ScaleAction(actionsChosen[2], -1.0f, 4.5f);

        //Instantiate Platform Prefab
        newLatestPlatform = Instantiate(intermediatePlatform, latestPlatform.transform.position, latestPlatform.transform.rotation);

        //Apply actions to new platform object
        newLatestPlatform.transform.Rotate(0.0f, platRotationAction, 0.0f);
        newLatestPlatform.transform.Translate(Vector3.forward * platForwardAction);
        newLatestPlatform.transform.Translate(Vector3.up * platHeightAction);

        //Save new and old distances and angles from end platform
        float newDistance = Vector3.Distance(newLatestPlatform.transform.position, endPlatform.transform.position);
        float newLatestAngle = Vector3.Angle(newLatestPlatform.transform.position, endPlatform.transform.position);
        float oldDistance = Vector3.Distance(latestPlatform.transform.position, endPlatform.transform.position);
        float oldLatestAngle = Vector3.Angle(latestPlatform.transform.position, endPlatform.transform.position);

        //Check if platform is closer
        if(newDistance < oldDistance && newLatestAngle < oldLatestAngle){
            AddReward(1f);
            Debug.Log("[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[PLATFORM IS CLOSER]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]");
        } 

        //Re-Assign LatestPlatform and log the new distance
        latestPlatform = newLatestPlatform;
        Debug.Log(newDistance);
        if((latestPlatform.transform.position.y >= (endPlatform.transform.position.y - 3.8f) && latestPlatform.transform.position.y <= (endPlatform.transform.position.y + 3.8f)) && newDistance <= 12.0f){
            Debug.Log("SUCCESSFUL PLATFORM CHAIN");
            AddReward(3f);
            academyEnv.GetComponent<EnvAcademy>().SetEndReached(true);

        }

        
    }

    public void SetFirstPlatform(GameObject obj){
        firstPlatform = obj;
    }

    /*
        CollectObservations()
        Called every lifecycle loop.
        Adds normalised observations as inputs to the neural network.
    */
    public override void CollectObservations(VectorSensor sensor){

        var relativeToEndPos = endPlatform.transform.position - latestPlatform.transform.position;

        sensor.AddObservation(relativeToEndPos.x/40f);
        sensor.AddObservation(relativeToEndPos.y/20f);
        sensor.AddObservation(relativeToEndPos.z/40f);

        if(latestPlatform == firstPlatform){
            var firstToEndPos = endPlatform.transform.position - firstPlatform.transform.position;
            sensor.AddObservation(firstToEndPos.x/40f);
            sensor.AddObservation(firstToEndPos.y/20f);
            sensor.AddObservation(firstToEndPos.z/40f);
            sensor.AddObservation(firstPlatform.transform.rotation.eulerAngles/180f);
        } else {
            var oldLatestToEndPos = endPlatform.transform.position - oldLatestPlatform.transform.position;
            sensor.AddObservation(oldLatestToEndPos.x/40f);
            sensor.AddObservation(oldLatestToEndPos.y/20f);
            sensor.AddObservation(oldLatestToEndPos.z/40f);
            sensor.AddObservation(oldLatestPlatform.transform.rotation.eulerAngles/180f);
        }
        sensor.AddObservation(Vector3.Angle(latestPlatform.transform.position, endPlatform.transform.position)/180f);
        sensor.AddObservation(Vector3.Distance(latestPlatform.transform.position, endPlatform.transform.position)/60f);
        sensor.AddObservation(auxInput);


    }

    public GameObject GetLatestPlatform(){
        return latestPlatform;
    }

    public void SetLatestPlatform(GameObject platform){
        latestPlatform = platform;
    }

    /*
        OnActionReceived()
        Called every lifecycle loop.
        Collects action outputs from the neural network.
        Calls the SpawnOPlatform() function if the player has progressed.
    */
    public override void OnActionReceived(ActionBuffers actions) {
        AddReward(-0.0005f);
        if(PlayerHasProgressed()){
            SpawnPlatform(actions.ContinuousActions);
            AddReward(0.2f);
            spawnDone = true;
        }
    }

    /*
        AuxReward()
        Called every every time the solver fails.
        Called by the environment controller.
    */
    public void AuxReward(){
        AddReward(auxInput*10);
    }

    protected override void OnEnable() {
        base.OnEnable();
    }
    protected override void OnDisable() {
        base.OnDisable();
    }

    /*
        Heuristic()
        Provides action buffers of random numbers to be 
        used as actions instead of NN outputs
    */
    public override void Heuristic(in ActionBuffers actionsOut){
        var continuousActionsOut = actionsOut.ContinuousActions;
        var forward = Random.Range(-1.0f, 1.0f);
        var rotate = Random.Range(-1.0f, 1.0f);
        var height = Random.Range(-1.0f, 1.0f);

        continuousActionsOut[0] = forward;
        continuousActionsOut[1] = rotate;
        continuousActionsOut[2] = height;
        
    }

}
