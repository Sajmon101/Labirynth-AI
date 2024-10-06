using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;
using Unity.VisualScripting;

public class Find_exit : Agent
{
    public GameObject exit_to_choose;
    public GameObject pointPrefab;
    //private List<GameObject> points = new List<GameObject>();
    GameObject[] points = new GameObject[4];
    private int wall_rnd_idx = 0;
    //private int old_direction = 0;
    private float nextmove = 0.5f;
    //private float old_hor = 0;
    //private float old_ver = 0;
    Ray ray_left = new Ray(Vector3.zero,Vector3.back);
    Ray ray_right = new Ray(Vector3.zero, Vector3.back);
    Ray ray_up = new Ray(Vector3.zero, Vector3.back);
    Ray ray_down = new Ray(Vector3.zero, Vector3.back);

    public override void CollectObservations(VectorSensor sensor)
    {
        //observe agent posiotion
        sensor.AddObservation(transform.localPosition);
        //observe disabled wall position
        GameObject sensor_wall = exit_to_choose.transform.GetChild(wall_rnd_idx).gameObject;
        //if (wall_rnd_idx<=5) sensor_wall.transform.localPosition += new Vector3(0.5f, 0, 0); 
        //if (wall_rnd_idx<=10 && wall_rnd_idx>5) sensor_wall.transform.localPosition += new Vector3(0, 0, -0.5f); 
        //if (wall_rnd_idx<=15 && wall_rnd_idx>10) sensor_wall.transform.localPosition += new Vector3(0, 0, 0.5f); 
        //if (wall_rnd_idx<=20 && wall_rnd_idx>15) sensor_wall.transform.localPosition += new Vector3(-0.5f, 0, 0); 

        sensor.AddObservation(sensor_wall.transform.localPosition);
        sensor.AddObservation(Physics.Raycast(ray_right, 1.2f, 1));
        sensor.AddObservation(Physics.Raycast(ray_left, 1.2f, 1));
        sensor.AddObservation(Physics.Raycast(ray_up, 1.2f, 1));
        sensor.AddObservation(Physics.Raycast(ray_down, 1.2f, 1));
    }
    public override void OnEpisodeBegin()
    {
        //active wall again
        GameObject wall = exit_to_choose.transform.GetChild(wall_rnd_idx).gameObject;
        wall.SetActive(true);

        //destroy points
        for (int i = 0; i < 4; i++)
        {
            Destroy(points[i]);
        }

        //place AI randomly
        int x_rnd = UnityEngine.Random.Range(0, 5);
        int z_rnd = UnityEngine.Random.Range(0, 5);
        transform.localPosition = new Vector3(-1.5f + x_rnd, 0.25f, -1.5f + z_rnd);

        //place points randomly
        int a = 0;
        for (int i = 0; i<2; i++)
            for (int j = 0; j<2; j++)
            {
                int rnd_x = UnityEngine.Random.Range(0, 2);
                int rnd_z = UnityEngine.Random.Range(0, 2);
                points[a] = Instantiate(pointPrefab, transform.parent.position + new Vector3(-1.5f + 3f*i + rnd_x, 0.25f, 2.5f - 3f*j - rnd_z), Quaternion.identity, transform.parent);
                a++;
            }

        //disable wall randomly
        wall_rnd_idx = UnityEngine.Random.Range(0, 19);
        wall = exit_to_choose.transform.GetChild(wall_rnd_idx).gameObject;
        wall.SetActive(false);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        discreteActions[0] = 0;
        if (Input.GetKey(KeyCode.RightArrow)) discreteActions[0] = 4;
        if(Input.GetKey(KeyCode.LeftArrow)) discreteActions[0] = 1;
        if(Input.GetKey(KeyCode.UpArrow)) discreteActions[0] = 2;
        if(Input.GetKey(KeyCode.DownArrow)) discreteActions[0] = 3;

        //float move_x = Input.GetAxis("Horizontal");
        //if (move_x>0) discreteActions[0] = 1;
        //if (move_x<0) discreteActions[0] = -1;
        //if (move_x>0 && (move_x<old_hor)) discreteActions[0] = 0;
        //if (move_x<0 && (move_x>old_hor)) discreteActions[0] = 0;
        //old_hor = move_x;

        //float move_z = Input.GetAxis("Vertical");
        //if (move_z>0) discreteActions[1] = 1;
        //if (move_z<0) discreteActions[1] = -1;
        //if (move_z>0 && (move_z<old_hor)) discreteActions[0] = 0;
        //if (move_z<0 && (move_z>old_hor)) discreteActions[0] = 0;
        //old_hor = move_z;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //checking if walls are around
        ray_left = new Ray(transform.localPosition, Vector3.left);
        ray_right = new Ray(transform.localPosition, Vector3.right);
        ray_up = new Ray(transform.localPosition, Vector3.forward);
        ray_down = new Ray(transform.localPosition, Vector3.back);

        int award = 20;

        int direction = actionBuffers.DiscreteActions[0];

        //movement
        //if (old_direction != direction)
        if(Time.time > nextmove)
        {
            nextmove = Time.time + 0.1f;

            switch (direction)
            {
                case 4:
                    if (award>6) award=award--;
                    if(!Physics.Raycast(ray_right, 1.2f, 1)) transform.localPosition += Vector3.right;
                    else SetReward(-0.5f);
                    break;
                case 1:
                    if (award>6) award=award--;
                    if(!Physics.Raycast(ray_left, 1.2f, 1)) transform.localPosition += Vector3.left;
                    else SetReward(-0.5f);
                    break;
                case 2:
                    if (award>6) award=award--;
                    if(!Physics.Raycast(ray_up, 1.2f, 1)) transform.localPosition += Vector3.forward;
                    else SetReward(-0.5f);
                    break;
                case 3:
                    if (award>6) award=award--;
                    if(!Physics.Raycast(ray_down, 1.2f, 1)) transform.localPosition += Vector3.back;
                    else SetReward(-0.5f);
                    break;
            }

        //    old_direction = direction;
        }

        ////when escape maze
        Ray ground = new Ray(transform.localPosition, Vector3.down);
        if (!Physics.Raycast(ground, 2f))
        {
            SetReward(award);

            EndEpisode();
        }
    }

    ////collect point
    void OnCollisionEnter(UnityEngine.Collision collision)
    {
        SetReward(+2f);
        if(collision.gameObject.tag == "point")
        {
            Destroy(collision.gameObject);
        }
    }
}
