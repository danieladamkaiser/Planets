using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet 
{
    private GameObject myGameObject;
    public GameObject MyPlanet { get { return myGameObject; } }
    private PlanetMonoBehaviour planetMono;

    public Planet(GameObject planetPrefab)
    {
        myGameObject = GameObject.Instantiate(planetPrefab);
        planetMono = myGameObject.GetComponent<PlanetMonoBehaviour>();
    }
}
