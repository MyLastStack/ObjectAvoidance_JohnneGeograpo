using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterMovement : MonoBehaviour
{
    [SerializeField] GameObject prey;

    void Start()
    {
        
    }

    void Update()
    {
        transform.LookAt(prey.transform.position);
        transform.Translate(Vector3.forward * Time.deltaTime * 2);
    }
}
