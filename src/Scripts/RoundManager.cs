using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//By Vianney Susanto
//Date Made: April 12 - 13th
/*
    This script is meant to be the main script running the game. 
    It keeps track of the player's and dealer's score, their total wins and losses, and has the methods for handing out the cards, as well as counting them.
*/

public class RoundManager : MonoBehaviour
{   
    public Transform dealercardsLayout;
    public Transform playercardsLayout;
    public Transform DeckSpawn;
    public List<GameObject> masterDeck = new List<GameObject>();
    private List<GameObject> currentDeck = new List<GameObject>();
    /*
        "dealercardsLayout" is the dealer's area for cards, and the reverse goes with "playercardsLayout". 
        "DeckSpawn" is the place where the cards are instantiated, then using their own script, move towards the dealer/player area for cards.
        "masterDeck" keeps a list of all 52 cards as is. "currentDeck" is the same except it's the one being used within the game. 
        As players go longer, the cards they use will be taken away from the deck to ensure accuracy. 
        Once the round ends, "currentDeck" is then copied with "masterDeck" in order to be reset.
    */

    private bool playerturn = false;
    private bool gameOver = false;
    private bool revealhiddencard = false;
    private int playerScore = 0;
    private int dealerScore = 0;
    private int totalgames; 
    private float totalwins;
    private int hiddenCardvalue;
    private float carddelayTimer = 0f;
    private float titlescreendelay = 0f;
    /*
        'playerturn' is used to see when the player has stopped in order to get all the scoring + dealer's action to start
        'gameOver' is used in order to stop certain actions once the round has finished and the winner has been found
        'revealhiddencard' is used to check the score between when the dealer's hidden card is revealed and possibly the next card, if needed
        'playerScore' and 'dealerScore' are both used to keep track of the points they each have respectively
        'totalgames' and 'totalwins' keeps track of the amount of games and wins you have, wins is a float as a Tie adds 0.5 of a score
        'hiddenCardvalue' keeps the hidden card's value, as it's not immediately added to the dealer's score until it's revealed when the player turns ends
        'carddelayTimer' is used as a delay between when cards are dealt to the dealer, 'titlescreendelay' is for how long the intro goes for.
    */

    public GameObject buttonOne;
    public GameObject buttonEleven;
    public GameObject playAgainButton;
    public GameObject quitButton;
    public GameObject buttonDraw;
    public GameObject buttonStay;
    public GameObject currentcard;
    public GameObject CardSlots;
    public GameObject hiddenCard;
    public GameObject facedownCard;
    public GameObject facedownPrefab;
    public CanvasGroup titleScreen;
    public CanvasGroup pauseMenu;
    /*
        'buttonOne' and 'buttonEleven' are the buttons used to select whether an ace should be worth 1 point or 11 points
        'playAgainbutton' and 'quitButton' are buttons used for playing again or to quit the game
        'buttonDraw' and 'buttonStay' are buttons used for whether the player wants to draw another card or end their turn and stay
        'currentcard' is the variable used for the current card instantiated from the deck, to be immediately added to the score
        'CardSlots' is the variable used for the "target" of the newly instantiated cards, essentially a blank space but used for the animation
        'hiddenCard', 'facedownCard', and 'facedownPrefab' are variables of the dealer's face down card, 
            hiddenCard is the actual value, while facedownCard and facedownPrefab is the image of the back
        'titleScreen' and 'pauseMenu' are the intro screen and pause menu as game objects
    */

    public Text playerWins;
    public Text dealerWins;
    public Text Tie;
    public Text playerScoretext;
    public Text dealerScoretext;
    public Text totalgamestext;
    public Text totalwinstext;
    public Text aceText;
    /*
        'playerWins', 'dealerWins', and 'Tie' are the text for each of the respective outcomes
        'playerScoretext' and 'dealerScoretext' are just the text of their respective scores
        'totalgamestext' and 'totalwinstext' are the text of the total amount of games and wins
        'aceText' is the text above the ace buttons to tell the player that an ace can be worth 1 or 11
    */

    public AudioSource audioSource;
    public AudioClip drawCardsound;
    public AudioClip drawCardsoundtwo;
    public AudioClip finalCardsound;
    /*
        These are all the various audios used, card and shuffling audios found online
    */

    System.Collections.IEnumerator TitleScreenIntro(){
        yield return new WaitForSeconds(6f);

        float transparency = titleScreen.alpha;

        while (titlescreendelay < 3f){
            titlescreendelay += Time.deltaTime;
            float newTransparency = Mathf.Lerp(transparency, 0f, titlescreendelay / 3f);
            titleScreen.alpha = newTransparency;
            yield return null;
        }

        titleScreen.alpha = 0f;
        titleScreen.gameObject.SetActive(false);
        playAgain();

        /*
            TitleScreenIntro() is the function for playing the title screen, essentially waits for 6 seconds, then fades out the screen for 3 seconds,
            using a while loop, then turns off the title screen once it's fully transparent. At the end, starts the game with the playAgain() function.
        */
    }

    void Start(){
        StartCoroutine(TitleScreenIntro());
        //In every Unity project, the Start() function is immediately executed once the program is launched. Tells the program to start the intro dunction
    }

    void Update(){
        //In every Unity project, the Update() function is executed at every frame, which ranges around 30-60 times per second

        if(Input.GetKeyDown(KeyCode.Escape)){
            pauseMenu.gameObject.SetActive(!pauseMenu.gameObject.activeSelf);
        }
        //Always checks if the player is pressing down the escape key, and if so, to activate/deactive the pause menu

        if(gameOver) return;
        //If the round is over, don't bother looking through everything else to save memory, 'return' essentially skips the rest of the code

        if(playerturn){
            if(Input.GetKeyDown(KeyCode.Tab) && buttonDraw.activeSelf){
                DrawCard();
            }
            if(Input.GetKeyDown(KeyCode.LeftShift) && buttonStay.activeSelf){
                Stay();
            }
        }
        //During rounds, checks if the tab and shift key have been pressed to draw a card or stay with their respective functions
        //Makes sure it's not after a round has ended, e.g during the player's turn

        //Consistently checks to see if the player's turn has ended, and then starts the scoring system
        if(!playerturn){
            if(facedownCard != null){
                facedownCard.transform.localScale = Vector3.Lerp(facedownCard.transform.localScale, Vector3.zero, Time.deltaTime * 5f);
                if (facedownCard.transform.localScale.magnitude < 0.01f){
                    Destroy(facedownCard);
                }
            }
            //Immediately fades out the image of the facedown card, revealing the actual hidden card and it's value

            if(!revealhiddencard){
                dealerScore += hiddenCardvalue;
                hiddenCardvalue = 0;

                revealhiddencard = true;
            //Adds the hidden card's value to the dealer's score, where the hidden card's value was kept secret the whole time
                
            dealerScoretext.text = "Dealer Score: " + dealerScore;
            //Updates the dealer's score right now, as if there was a tie or another result, the hidden card's value wouldn't be displayed
                if(dealerScore == playerScore && dealerScore > 16){
                    Tie.gameObject.SetActive(true);
                    playAgainButton.SetActive(true);
                    quitButton.SetActive(true);
                    if(facedownCard != null) Destroy(facedownCard);
                    gameOver = true;
                    totalgames++;
                    totalwins += 0.5f;
                    return;
                }
                //In-between, checks if there was a tie after revealing the hidden card, and if so activates the result for a tie
                //(The text to show there was a tie, the play again + quit button, and adding to the total games and wins)
            }

            //Starts to score, checks player actions first, such as going over or at 21. 
            //If so, tells the GameResults() function whether or not the player one, which displays the win/loss info
            if(!gameOver){
                if (playerScore > 21){
                    GameResults(false);
                    if(facedownCard != null) Destroy(facedownCard);
                    return;
                }else if(playerScore == 21){
                    GameResults(true);
                    if(facedownCard != null) Destroy(facedownCard);
                    return;
                }else if(dealerScore > playerScore && dealerScore < 22){
                    GameResults(false);
                    if(facedownCard != null) Destroy(facedownCard);
                    return;
                }

                //If neither of these, goes straight to the dealer's turn to start getting cards-
                // -as any of the above could occur in a win/loss before the dealer even needs to draw a card
                carddelayTimer += Time.deltaTime;
                    if(dealerScore <= 16 && carddelayTimer >= 1f){
                        DrawCard();
                        carddelayTimer = 0f;
                        //Adds a small delay between adding the cards to the dealer's hand
                    }else if(dealerScore > 16){
                        buttonDraw.SetActive(false);
                        buttonStay.SetActive(false);

                        switch (dealerScore){
                        case int n when n > playerScore && n < 22:
                            if(facedownCard != null) Destroy(facedownCard);
                            GameResults(false);
                            return;
                        case int n when n < playerScore && playerScore < 22:
                            if(facedownCard != null) Destroy(facedownCard);
                            GameResults(true);
                            return;
                        case int n when n > 21:
                            if(facedownCard != null) Destroy(facedownCard);
                            GameResults(true);
                            return;
                        case int n when n == playerScore:
                            Tie.gameObject.SetActive(true);
                            playAgainButton.SetActive(true);
                            quitButton.SetActive(true);
                            if(facedownCard != null) Destroy(facedownCard);
                            gameOver = true;
                            totalgames++;
                            totalwins += 0.5f;
                            return;
                        }

                        //Checks the several different cases after the dealer gets dealt, such as if it busts, is greater/less than the player's score, or a tie
                        //Then shows the respective results, either using GameResults() which is strictly for win/loss, or if it was a tie
                    }
                }   
        }else if(playerScore >= 21){
            playerturn = false;
        }
        //The above large if-statement occurrs if the player stayed, where playerturn becomes false
        //If the player busts, then it'll catch it and turn playerturn false as well here
        
        playerScoretext.text = "Your Score: " + playerScore;
        dealerScoretext.text = "Dealer Score: " + dealerScore;

        totalwinstext.text = "Total Wins: " + totalwins;
        totalgamestext.text = "Total Games: " + totalgames;
        //Consistently updates the player and dealer's scores, as well as the total wins and games
    }


    void GameResults(bool win){
        //The method for displaying all the win/loss info on the screen
        if(gameOver == false){
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            gameOver = true;
            //Uses the outer if-statement, as well as deselects everything just for safety, as there were some bugs with both during testing

            if(win){
                playerWins.gameObject.SetActive(true);
                playAgainButton.SetActive(true);
                quitButton.SetActive(true);

                buttonDraw.SetActive(false);
                buttonStay.SetActive(false);
                totalgames++;
                totalwins++;
                
                //If the player wins, all the win info such as player wins text, play again/quit button is active, turning off draw and stay between rounds-
                // -and updates the total wins and games
            }else{
                dealerWins.gameObject.SetActive(true);
                playAgainButton.SetActive(true);
                quitButton.SetActive(true);

                buttonDraw.SetActive(false);
                buttonStay.SetActive(false);
                totalgames++;
                //The same goes for if the player loses, displays all the loss info such as dealer wins, and the others same as above, without updating total wins
            }
        }
    }
    
    public void chooseOne(){
        playerScore += 1;
        ToggleChoiceButtons(false);
        //The method for the button choosing the value of the ace to be 1, adds 1 to the score then turns off the buttons
    }

    public void chooseEleven(){
        playerScore += 11;
        ToggleChoiceButtons(false);
        //The method for the button choosing the value of the ace to be 11, adds 11 to the score then turns off the buttons
    }

    public void DrawCard(){
        //The method for how both the dealer and the player draw their respective cards

        if(playerturn == true){
            GameObject newcardSlot = Instantiate(CardSlots, playercardsLayout.transform);

            int randomCard = Random.Range(0, currentDeck.Count);
            currentcard = Instantiate(currentDeck[randomCard], DeckSpawn.transform);
            currentcard.GetComponent<CardBehavior>().target = newcardSlot.gameObject;
            CardBehavior cardPoints = currentcard.GetComponent<CardBehavior>();
            
            playSoundFX();

            if(currentcard.tag == "Ace"){
                HandleAce();
            }

            playerScore += cardPoints.cardValue;
            currentDeck.RemoveAt(randomCard);
            
            /*
                If the method is called while it's the player's turn, creates a random card slot and card itself, assigns the card slot as the 'target'- 
                -in order for the card to move to that position from the deck. The points from the card is found using GetComponent, and adds it to the-
                -dealer or player's score. It plays a random shuffle sound effect, and if it's an ace, plays the HandleAce() method for whether an ace should-
                -be 1 or 11. The ace is naturally at 0, and 1 or 11 is automatically added to the score after the player makes their choice.
            */
        }
        if(playerturn == false){
            GameObject newcardSlot = Instantiate(CardSlots, dealercardsLayout.transform);

            int randomCard = Random.Range(0, currentDeck.Count);
            currentcard = Instantiate(currentDeck[randomCard], DeckSpawn.transform);
            currentcard.GetComponent<CardBehavior>().target = newcardSlot.gameObject;

            CardBehavior cardPoints = currentcard.GetComponent<CardBehavior>();
            
            playSoundFX();

            if(currentcard.tag == "Ace"){
                HandleAce();
            }

            dealerScore += cardPoints.cardValue;
            currentDeck.RemoveAt(randomCard);
            /*
                If the method is called during the dealer's turn, the same above is done, with a card + card slot made for the animation. 
                The only difference is that the points are added to the dealer's score instead. 
            */
        }
    }

    public void HandleAce(){
        //The method for if an ace is instantiated, as an ace can be 1 or 11
        CardBehavior cardPoints = currentcard.GetComponent<CardBehavior>();
        {
            if(playerScore < 11 && playerturn){
                ToggleChoiceButtons(true);
            }else{
                cardPoints.cardValue = 1;
            }
            //If it's less than 11, then the player gets to make the choice and the buttons are toggled true.
            //If it's more than 11, then the ace is automatically a 1, as anything 11+ added with 11 goes over 21
        }

        {
            if(dealerScore < 11 && !playerturn){
                int randomchoice = Random.Range(0,1);
                if(randomchoice == 0){
                    cardPoints.cardValue = 11;
                }else{
                    cardPoints.cardValue = 1;
                }
            }else if(currentcard.tag == "Ace" && !playerturn){
                cardPoints.cardValue = 1;
            }
            //If it's less than 11, it chooses randomly between 1 and 11
            //If it's more than 11, it's automatically worth 1 point
        }
    }

    public void Stay(){
        if(playerturn == true){
            playerturn = false;
            buttonDraw.SetActive(false);
            buttonStay.SetActive(false);
        }
        //Once the player stays and ends their turn, the draw and stay buttons are turned off, and the player turn ends, starting the dealer's turn
    }
    
    public void playAgain(){
        //The method for starting the round over again, resetting all the settings
        {
            ClearCards(DeckSpawn);
            ClearCards(playercardsLayout);
            ClearCards(dealercardsLayout);
            //Takes out all the cards that have been spawned, either the card values at DeckSpawn, or the slots at the player/dealer cards area
        }

        {
            playAgainButton.gameObject.SetActive(false);
            playerWins.gameObject.SetActive(false);
            dealerWins.gameObject.SetActive(false);
            Tie.gameObject.SetActive(false);
            quitButton.SetActive(false);
            ToggleChoiceButtons(false);
            //Turns off all of the info displayed for a win/loss/tie
        }

        {
            playerScore = 0;
            dealerScore = 0;
            //Resets the scores
        }

        {
            currentDeck = new List<GameObject>(masterDeck);
            audioSource.PlayOneShot(finalCardsound);
            //Resets the current deck, as well as makes a shuffling sound
        }

        {
            gameOver = false;
            revealhiddencard = false;
            playerturn = false;
            DrawCard();
            //Starts the round by turning off gameOver, revealhiddencard, but also playerturn as it deals a card to the dealer first.
        }

        {
            //The code for instantiating the facedown card, is close to identical to the regular draw card (e.g creating a card slot + random card)
            //but also instantiates the back to hide the actual card, as well as hiding the card value by storing it in a separate variable 
            GameObject hiddencardSlot = Instantiate(CardSlots, dealercardsLayout.transform);

            int randomCard = Random.Range(0, currentDeck.Count);
            hiddenCard = Instantiate(currentDeck[randomCard], DeckSpawn.transform);
            hiddenCard.GetComponent<CardBehavior>().target = hiddencardSlot.gameObject;
            hiddenCardvalue = hiddenCard.GetComponent<CardBehavior>().cardValue;
            
            if(hiddenCard.tag == "Ace"){
                if(dealerScore < 11){
                    hiddenCardvalue = 11;
                }else{
                    hiddenCardvalue = 1;
                }
            }

            facedownCard = Instantiate(facedownPrefab, DeckSpawn.transform);
            facedownCard.GetComponent<CardBehavior>().target = hiddencardSlot.gameObject;

            currentDeck.RemoveAt(randomCard);

            //Goes back to the player's turn, and then deals a card towards the player
            playerturn = true;
            DrawCard();
        }
        
    }

    public void QuitGame()
    {
        //The method for quitting the game
        Application.Quit();
    }   

    void ToggleChoiceButtons(bool isActive)
    {   
        //If there's an ace, it sets all the ace buttons and texts active, and the draw/stay cards inactive so that the player is forced to make a choice first
        //This occurred enough times in the code for a separate method to be necessary to be clean
        aceText.gameObject.SetActive(isActive);
        buttonOne.SetActive(isActive);
        buttonEleven.SetActive(isActive);
        buttonDraw.SetActive(!isActive);
        buttonStay.SetActive(!isActive);
    }

    void ClearCards(Transform layout){
        foreach(Transform child in layout){
            Destroy(child.gameObject);
        }
        //When resetting the game, resets the cards that were instantiated, by using a for-loop and deleting all the cards that were instantiated
    }

    void playSoundFX(){
        AudioClip chosenSound = Random.Range(0, 2) == 0 ? drawCardsound : drawCardsoundtwo;
        audioSource.PlayOneShot(chosenSound);
        //Chooses between 2 random sound effect clips, then plays one.
    }
}
