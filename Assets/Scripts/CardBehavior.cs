using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBehavior : MonoBehaviour
{
    public GameObject target;
    Vector3 cardMovement;
    
    public int cardValue;

    void Update(){
        transform.position = Vector2.Lerp(transform.position, target.transform.position, Time.deltaTime * 25);
    }
}
