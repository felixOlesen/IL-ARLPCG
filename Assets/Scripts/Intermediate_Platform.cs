using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intermediate_Platform : MonoBehaviour
{   
    /*
        VARIABLE DECLARATIONS
        All necessary variables and object declarations.
        Allows for this script to interact with external singleton instances.
    */
    private bool solverCollided;
    public GameObject envacademy;

    public bool getSolverCollision(){
        return solverCollided;
    }

    /*
        OnCollisionEnter()
        Called when solver collides with the platform
        Detects a collision with the solver and raised a flag
        In case the solver has collided
    */
    private void OnCollisionEnter(Collision collision){
        var colCount = 1;
        if(collision.gameObject.name == "Solver_Agent" && colCount == 1){
            solverCollided = true;
            colCount++;
        }
    }

    /*
        OnCollisionEnter()
        Called when solver stops colliding with the platform
        Detects end of collision with the solver and resets the flag
    */
    private void OnCollisionExit(Collision collision){
        if(collision.gameObject.name == "Solver_Agent"){
            solverCollided = false;

        }
    }
}
