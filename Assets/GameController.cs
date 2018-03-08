using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public GameObject PlanetPrefab;
    public GameObject[] CreaturePrefabs;

    private List<Planet> myPlanets = new List<Planet>();
    private List<Creature> creatures = new List<Creature>();

    void Awake()
    {

    }

    void Start()
    {
        myPlanets.Add(new Planet(PlanetPrefab));
        creatures.Add(new Creature(CreaturePrefabs[0],myPlanets[0].MyPlanet, new Vector3(2,0,0)));
    }

    void Update()
    {
        foreach (Creature creature in creatures)
        {
            creature.Update(Time.deltaTime);
        }
    }
}
