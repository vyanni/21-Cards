using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//By Vianney Susanto
//Date made: April 12th
//This specific script is made for each individual card, specifically to control the movement of the card and values.

public class CardBehavior : MonoBehaviour
{
    public GameObject target;
    //The GameObject 'target' is where the cards move to after being instantiated by the deck, assigned to a card slot by the player or dealer's card layouts  

    public int cardValue;
    //The cardValue is the face-value amount which the card is worth. This is usually added to the player and dealer score, each card has one.
    //Aces have their cardvalue set to 0, and when the player makes the choice, 1 or 11 is automatically added to their score instead.

    void Update(){
        transform.position = Vector2.Lerp(transform.position, target.transform.position, Time.deltaTime * 25);
        //Using the update function, which executes every second, it moves the card's position towards the target
    }
}
