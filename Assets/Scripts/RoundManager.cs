using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Name: Vianney Susanto
//Date Made: April 12th
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
    private bool dealerturnStart = true;
    private int playerScore = 0;
    private int dealerScore = 0;
    private int totalgames; 
    private int totalwins;
    private int hiddenCardvalue;
    private float carddelayTimer = 0f;
    private float titlescreendelay = 0f;
    /*

    */

    public GameObject buttonOne;
    public GameObject buttonEleven;
    public GameObject playAgainButton;
    public GameObject quitButton;
    public GameObject currentcard;
    public GameObject buttonDraw;
    public GameObject buttonStay;
    public GameObject CardSlots;
    public GameObject hiddenCard;
    public GameObject facedownCard;
    public GameObject facedownPrefab;
    public CanvasGroup titleScreen;

    public Text playerWins;
    public Text dealerWins;
    public Text Tie;
    public Text playerScoretext;
    public Text dealerScoretext;
    public Text totalgamestext;
    public Text totalwinstext;
    public Text aceText;

    public AudioSource audioSource;
    public AudioClip drawCardsound;
    public AudioClip drawCardsoundtwo;
    public AudioClip finalCardsound;

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
    }

    void Start(){
        StartCoroutine(TitleScreenIntro());
    }

    void Update(){
        {
            if(!playerturn){
                switch (playerScore){
                    case int n when n > 21:
                        GameResults(false);
                        break;
                    case int n when n == 21:
                        GameResults(true);
                        break;
                    default:
                        break;
                    }
                
                dealerScore += hiddenCardvalue;
                hiddenCardvalue = 0;
                Destroy(facedownCard);

                if(dealerScore > playerScore && dealerScore < 22){
                    GameResults(false);
                }

                carddelayTimer += Time.deltaTime;
                if(dealerScore <= 16 && carddelayTimer >= 1f){
                    DrawCard();
                    carddelayTimer = 0f;
                }else if(dealerScore > 16){
                    buttonDraw.SetActive(false);
                    buttonStay.SetActive(false);

                    switch (dealerScore){
                    case int n when n > playerScore && n < 22:
                        GameResults(false);
                        break;
                    case int n when n < playerScore && playerScore < 22:
                        GameResults(true);
                        break;
                    case int n when n > 21:
                        GameResults(true);
                        break;
                    case int n when n == playerScore:
                        Tie.gameObject.SetActive(true);
                        playAgainButton.SetActive(true);
                        quitButton.SetActive(true);
                        break;
                    }
                }   
            }else if(playerScore >= 21){
                playerturn = false;
            }
        }

        {
            if (Input.GetKeyDown(KeyCode.Space) && buttonDraw.activeInHierarchy == true){
                DrawCard();
            }else if(Input.GetKeyDown(KeyCode.Return) && buttonStay.activeInHierarchy == true){
                Stay();
            }
        }

        {
            playerScoretext.text = "Your Score: " + playerScore;
            dealerScoretext.text = "Dealer Score: " + dealerScore;

            totalwinstext.text = "Total Wins: " + totalwins;
            totalgamestext.text = "Total Games: " + totalgames;
        }
    }

    void GameResults(bool win){
        if(dealerturnStart == true){
            if(win){
                playerWins.gameObject.SetActive(true);
                playAgainButton.SetActive(true);
                quitButton.SetActive(true);

                buttonDraw.SetActive(false);
                buttonStay.SetActive(false);
                totalgames++;
                totalwins++;
            }else{
                dealerWins.gameObject.SetActive(true);
                playAgainButton.SetActive(true);
                quitButton.SetActive(true);

                buttonDraw.SetActive(false);
                buttonStay.SetActive(false);
                totalgames++;
            }
            dealerturnStart = false;
        }
    }
    
    public void chooseOne(){
        CardBehavior cardPoints = currentcard.GetComponent<CardBehavior>();
        playerScore += 1;

        ToggleChoiceButtons(false);
    }

    public void chooseEleven(){
        CardBehavior cardPoints = currentcard.GetComponent<CardBehavior>();
        playerScore += 11;

        ToggleChoiceButtons(false);
    }

    public void DrawCard(){
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
        }
    }

    public void HandleAce(){
        CardBehavior cardPoints = currentcard.GetComponent<CardBehavior>();
        {
            if(playerScore < 11 && playerturn){
                ToggleChoiceButtons(true);
            }else{
                cardPoints.cardValue = 1;
            }
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
        }
    }

    public void Stay(){
        if(playerturn == true){
            playerturn = false;
        }
    }
    
    public void playAgain(){
        {
            ClearCards(DeckSpawn);
            ClearCards(playercardsLayout);
            ClearCards(dealercardsLayout);
        }

        {
            playAgainButton.gameObject.SetActive(false);
            playerWins.gameObject.SetActive(false);
            dealerWins.gameObject.SetActive(false);
            Tie.gameObject.SetActive(false);
            quitButton.SetActive(false);
            ToggleChoiceButtons(false);
        }

        {
            playerScore = 0;
            dealerScore = 0;
        }

        {
            currentDeck = new List<GameObject>(masterDeck);
            audioSource.PlayOneShot(finalCardsound);
        }

        {
            dealerturnStart = true;
            playerturn = false;
            DrawCard();
        }

        {
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
            playerturn = true;
            DrawCard();
        }
        
    }

    public void QuitGame()
    {
        Application.Quit();
    }   

    void ToggleChoiceButtons(bool isActive)
    {
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
    }

    void playSoundFX(){
        AudioClip chosenSound = Random.Range(0, 2) == 0 ? drawCardsound : drawCardsoundtwo;
        audioSource.PlayOneShot(chosenSound);
    }
}
