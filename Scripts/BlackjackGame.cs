using System;
using Godot;

public partial class BlackjackGame : Node
{
    [Export] private Deck deck;
    [Export] private Player player;
    [Export] private Dealer dealer;

    // UI Elements
    [Export] private Button hitButton;
    [Export] private Button stoodButton;
    [Export] private Label playerScoreLabel;
    [Export] private Label dealerScoreLabel;
    [Export] private Label resultLabel;
    [Export] private VFlowContainer playerCardContainer;
    [Export] private VFlowContainer dealerCardContainer;

    // Load the card display scene for instancing cards in the UI
    private PackedScene cardDisplayScene = GD.Load<PackedScene>("res://Prefabs/card.tscn");

    public override void _Ready()
    {
        StartGame();
    }

    private void StartGame()
    {
        // Enable the buttons at the start of a new game
        stoodButton.Disabled = false;
        hitButton.Disabled = false;

        // Reset player and dealer hands
        player.ClearHand();
        dealer.ClearHand();

        // Reset and shuffle the deck
        deck.CreateDeck();
        deck.ShuffleDeck();

        // Deal initial cards
        player.AddCard(deck.DrawCard());
        player.AddCard(deck.DrawCard());
        dealer.AddCard(deck.DrawCard());
        dealer.AddCard(deck.DrawCard());

        // Display both player and dealer hands in the UI
        DisplayPlayerHand();
        DisplayDealerHand();
        UpdateScoreLabels();
    }

    private void DisplayPlayerHand()
    {
        // Clear existing displayed cards first to avoid duplicates
        foreach (Node child in playerCardContainer.GetChildren())
        {
            child.QueueFree();
        }

        // Add each card in the player's hand to the UI
        foreach (var card in player.Hand)
        {
            CreateAndAddCardDisplay(card, playerCardContainer);
        }
    }

    private void DisplayDealerHand()
    {
        // Clear existing displayed cards first to avoid duplicates
        foreach (Node child in dealerCardContainer.GetChildren())
        {
            child.QueueFree();
        }

        // Add each card in the dealer's hand to the UI
        foreach (var card in dealer.Hand)
        {
            CreateAndAddCardDisplay(card, dealerCardContainer);
        }
    }

    private void CreateAndAddCardDisplay(Card card, VFlowContainer container)
    {
        // Instantiate a new CardDisplay node and set its properties
        var cardDisplay = (CardDisplay)cardDisplayScene.Instantiate();
        cardDisplay.SetCard(card.Rank, card.Suit);
        container.AddChild(cardDisplay);
    }

    private void UpdateScoreLabels()
    {
        // Calculate scores once and avoid redundant calls
        var playerScore = player.CalculateScore();
        var dealerScore = dealer.CalculateScore();

        // Update score labels
        playerScoreLabel.Text = $"{playerScore.maxScore}/{playerScore.minScore}";
        dealerScoreLabel.Text = $"{dealerScore.maxScore}/{dealerScore.minScore}";
    }

    public void HandlePlayerHit()
    {
        // Add a new card to the player's hand
        player.AddCard(deck.DrawCard());

        // Update the player's hand display and score
        DisplayPlayerHand();
        UpdateScoreLabels();

        // Check if the player busted
        if (player.IsBusted())
        {
            GD.Print("Player busted!");
            EndGame("Dealer wins! Player busted.");
        }
    }

    public void HandlePlayerStand()
    {
        // Disable the buttons to prevent further actions
        stoodButton.Disabled = true;
        hitButton.Disabled = true;

        // Dealer's turn to draw cards if needed
        while (dealer.ShouldDrawCard())
        {
            dealer.AddCard(deck.DrawCard());
        }

        // Update the dealer's hand and score display
        DisplayDealerHand();
        UpdateScoreLabels();

        // Determine the winner based on final scores
        DetermineWinner();
    }

    private void DetermineWinner()
    {
        var playerScore = player.CalculateScore().maxScore;
        var dealerScore = dealer.CalculateScore().maxScore;

        if (dealer.IsBusted())
        {
            EndGame("Player wins! Dealer busted.");
        }
        else if (playerScore > dealerScore)
        {
            EndGame("Player wins!");
        }
        else if (playerScore < dealerScore)
        {
            EndGame("Dealer wins!");
        }
        else
        {
            EndGame("It's a tie!");
        }
    }

    private void EndGame(string resultMessage)
    {
        // Print the result and disable the hit and stand buttons
        resultLabel.Text = resultMessage;
        stoodButton.Disabled = true;
        hitButton.Disabled = true;

        // Optionally, display the result message in the UI (e.g., using a Label or Popup)
    }

    public void HandleNewGame()
    {
        resultLabel.Text = string.Empty;
        // Reset the game and UI for a new round
        StartGame();
    }

    // Called when the Hit button is pressed
    public void _on_hit_button_down()
    {
        HandlePlayerHit();
    }

    // Called when the Stand button is pressed
    public void _on_stood_button_down()
    {
        HandlePlayerStand();
    }

    // Called when the New Game button is pressed
    public void _on_new_game_btn_button_down()
    {
        HandleNewGame();
    }
}
