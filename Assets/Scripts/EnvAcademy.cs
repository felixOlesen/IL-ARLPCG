using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class EnvAcademy : MonoBehaviour
{
    /*
        VARIABLE DECLARATIONS
        All necessary variables and object declarations.
        Allows for this script to interact with external singleton instances.
    */
    public GameObject endPlatform;
    public int ENV_RESET_THRESHOLD = 5;
    public GameObject solver;
    public GameObject pcg;
    private GameObject envLatestPlat;
    public GameObject intermediatePlatform;
    private int interPlats;
    private int epCount;
    private int epDifference;
    private GameObject beginningPlat;
    private bool EndReached;


    /*
        Start()
        Initializes the script and sets up the game area for both agents.
        Initializes starting platform as the first and latest platform.
        Gets a list of all the intermediate platforms.
        Sets the PCG to be inactive.
    */
    void Start()
    {
        
        NewEndPlat();
        beginningPlat = NewStartPlat();
        epCount = 0;
        pcg.GetComponent<ARLPCG>().SetLatestPlatform(beginningPlat);
        pcg.GetComponent<ARLPCG>().SetFirstPlatform(beginningPlat);
        envLatestPlat = beginningPlat;
        interPlats = solver.GetComponent<Solver_Agent>().GetTotalInterPlats();
        pcg.SetActive(false);
        EndReached = false;

    }


    /*
        Update()
        Checks if the EnvReset threshold has been reached
        If CheckTimeour -> true, call EnvReset()
    */
    void Update()
    {
        if(CheckTimeout()){
            EnvReset();
        }
    }

    /*
        FixedUpdate()
        Checks if the solver has progressed
        Activates the generator to generate another platform.
        Once the platform generation is complete
            Re-Assign the latestplatform
            Check if the platform is too far away
        Deactivate the PCG

    */
    void FixedUpdate() {
        if(envLatestPlat.GetComponent<Intermediate_Platform>().getSolverCollision()){
            if(!EndReached){
                pcg.SetActive(true);
                epDifference = 0;
                epCount = solver.GetComponent<Solver_Agent>().CompletedEpisodes;
            }

            if(pcg.GetComponent<ARLPCG>().spawnDone){
                envLatestPlat = pcg.GetComponent<ARLPCG>().GetLatestPlatform();
                
                if(Vector3.Distance(envLatestPlat.transform.position, endPlatform.transform.position) >= 50.0f){
                    pcg.GetComponent<ARLPCG>().AddReward(-1f);
                    Debug.Log("LATEST PLATFORM TOO FAR. -1 REWARD");
                    solver.GetComponent<Solver_Agent>().EndEpisode();
                    EnvReset();
                }
                pcg.SetActive(false);
            }
        }
    }


    /*
        SolverFail()
        Calls the aux reward in the PCG script to assign reward.
        Gets called on every solver failure.
    */
    public void SolverFail(){
        pcg.GetComponent<ARLPCG>().AuxReward();
    }

    public void SetEndReached(bool reached){
        EndReached = reached;
    }

    public GameObject GetEnvLatest(){
        return envLatestPlat;
    }

    /*
        CreateStartPlatPos()
        Designed for a randomised start platform for future improvements
        Called in the EnvReset() function.
    */
    public Vector3 CreateStartPlatPos(int side){
        var pos = new Vector3(0,0,0);
        if(side == 0){
            pos = new Vector3(Random.Range(-2.75f, 2.75f), 0.4f, 7.77f);
        } else if(side == 1) {
            pos = new Vector3(7.77f, 0.4f, Random.Range(-2.75f, 2.75f));
        } else if(side == 2) {
            pos = new Vector3(Random.Range(-2.75f, 2.75f), 0.4f, -7.77f);
        } else if(side == 3) {
            pos = new Vector3(-7.77f, 0.4f, Random.Range(-2.75f, 2.75f));
        }
        return pos;
    }

    /*
        NewEndPlat()
        Designed for a randomised end platform
        Called in the EnvReset() function.
    */
    public void NewEndPlat(){
        var endPlatX = Random.Range(-35.0f, 35.0f);
        var endPlatY = Random.Range(5.0f, 12.0f);
        var endPlatZ = Random.Range(0.0f, 35.0f);
        var leftOrRight = Random.Range(0,1);
        if(leftOrRight == 0 && endPlatZ <= 15.0f){
            endPlatX = Random.Range(25.0f, 35.0f);
        } else if(leftOrRight == 1 && endPlatZ <=15.0f){
            endPlatX = Random.Range(-25.0f, -35f);
        }
        endPlatform.transform.position = new Vector3(endPlatX, endPlatY, endPlatZ);
    }

    /*
        NewStartPlat()
        Designed for a randomised start platform
        Commented code can be uncommented to initialize the start platform 
        in a random position.
        Called in the EnvReset() function.
    */
    public GameObject NewStartPlat(){

        //var startingSide = Random.Range(0,4);
        var startPlat = Instantiate(intermediatePlatform);
        startPlat.transform.position = new Vector3(0f,0.4f,7.77f);
        /*startPlat.transform.position = CreateStartPlatPos(startingSide);
        if(startingSide == 1) {
            startPlat.transform.Rotate(0f,90f,0f);
        } else if(startingSide == 2) {
            startPlat.transform.Rotate(0f,180f,0f);
        } else if(startingSide == 3) {
            startPlat.transform.Rotate(0f,-90f,0f);
        }*/

        return startPlat;
    }

    /*
        EnvReset()
        Called during timeout or on initialization
        Resets the entire game area
    */
    public void EnvReset(){
        Debug.Log("(((((((((((((((((((((((((((((((ENV RESET)))))))))))))))))))))))))))))))");
        var intermediate_platform_list = GameObject.FindGameObjectsWithTag("platform");
        foreach (GameObject plat in intermediate_platform_list){
            Destroy(plat);
        }
        NewEndPlat();
        beginningPlat = NewStartPlat();
        envLatestPlat = beginningPlat;
        pcg.GetComponent<ARLPCG>().SetLatestPlatform(beginningPlat);
        pcg.GetComponent<ARLPCG>().SetFirstPlatform(beginningPlat);
        pcg.GetComponent<ARLPCG>().EndEpisode();
        EndReached = false;
    }

    /*
        CheckTimeout()
        Checks if the environment reset threshold has been passed
        If it has, EnvReset() is called
    */
    public bool CheckTimeout(){
        epDifference = solver.GetComponent<Solver_Agent>().CompletedEpisodes - epCount;
        var timeout = false;
        if(epDifference > ENV_RESET_THRESHOLD){
            epCount = solver.GetComponent<Solver_Agent>().CompletedEpisodes;
            epDifference = 0;
            timeout = true;
        }
        return timeout;
    }
}
