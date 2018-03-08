using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature  {

    private GameObject myGameObject;
    private GameObject currentPlanet;
    private Rigidbody myRigidBody;

    public Creature(GameObject creaturePrefab, GameObject currentPlanet, Vector3 spawnPosition)
    {
        myGameObject = GameObject.Instantiate(creaturePrefab);
        myGameObject.transform.position = spawnPosition;
        this.currentPlanet = currentPlanet;
        myGameObject.transform.parent = currentPlanet.transform;
        myRigidBody = myGameObject.GetComponent<Rigidbody>();
    }

    public void Update(float delta)
    {
        ApplyGravity(delta);
    }

    private void ApplyGravity(float delta)
    {
        if (currentPlanet != null) myRigidBody.AddForce(((-currentPlanet.transform.position-myGameObject.transform.position).normalized) * 10 * delta);
    }
}
