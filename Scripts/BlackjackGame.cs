using System;
using System.Threading.Tasks;
using Godot;

public partial class BlackjackGame : Node
{
    [Export] private Deck deck;
    [Export] private Player player;
    [Export] private Dealer dealer;

    // UI Elements
    [Export] private Button hitButton;
    [Export] private Button stoodButton;
    [Export] private Button playButton; // Updated from newGameButton
    [Export] private LineEdit betInput; // Editable text field for betting
    [Export] private Label playerScoreLabel;
    [Export] private Label dealerScoreLabel;
    [Export] private Label resultLabel;
    [Export] private Label playerMoneyLabel; // Label to display player's money
    [Export] private VFlowContainer playerCardContainer;
    [Export] private VFlowContainer dealerCardContainer;

    // Animation
    [Export] private AnimationPlayer animationPlayer;

    // Audio player
    [Export] private AudioStreamPlayer2D audioPlayer;

    // Card scene
    private PackedScene cardDisplayScene = GD.Load<PackedScene>("res://Prefabs/card.tscn");

    // State and flags
    private bool isDealerCardHidden = true;
    private bool isAnimating = false;
    private bool isGameInProgress = false;

    public override void _Ready()
    {
        UpdatePlayerMoneyLabel(); // Initialize player's money label
    }

    private async void StartGame()
    {
        ResetGameUI();
        isGameInProgress = true;
        deck.CreateDeck();
        deck.ShuffleDeck();
        await DealInitialCardsWithAnimation();
        UpdateScoreLabels();
    }

    private void ResetGameUI()
    {
        hitButton.Disabled = false;
        stoodButton.Disabled = false;
        playButton.Disabled = true; // Disable Play button when starting a new game

        isDealerCardHidden = true;
        player.ClearHand();
        dealer.ClearHand();
        ClearCardContainer(playerCardContainer);
        ClearCardContainer(dealerCardContainer);
        playerScoreLabel.Text = string.Empty;
        dealerScoreLabel.Text = string.Empty;
        resultLabel.Text = string.Empty;
    }

    private async Task DealInitialCardsWithAnimation()
    {
        hitButton.Disabled = true;
        stoodButton.Disabled = true;

        for (int i = 0; i < 2; i++)
        {
            await DealCardWithAnimation(player, playerCardContainer, "DrawingCard");
            await DealCardWithAnimation(dealer, dealerCardContainer, "DealerDrawCard");
        }

        hitButton.Disabled = false;
        stoodButton.Disabled = false;
    }

    private async Task DealCardWithAnimation(Player recipient, VFlowContainer container, string animationName)
    {
        recipient.AddCard(deck.DrawCard());
        await PlayAnimation(animationName);
        UpdateHandDisplay(recipient, container, recipient == dealer && isDealerCardHidden);

        // Play sound effect when a card is dealt
        if (audioPlayer != null && audioPlayer.Stream != null)
        {
            audioPlayer.Play();
        }
    }

    private async Task PlayAnimation(string animationName)
    {
        isAnimating = true;
        animationPlayer.Play(animationName);
        await ToSignal(animationPlayer, "animation_finished");
        isAnimating = false;
    }

    private void UpdateHandDisplay(Player player, VFlowContainer container, bool hideSecondCard = false)
    {
        ClearCardContainer(container);
        for (int i = 0; i < player.Hand.Count; i++)
        {
            var cardDisplay = (CardDisplay)cardDisplayScene.Instantiate();
            cardDisplay.SetCard(player.Hand[i].Rank, player.Hand[i].Suit);
            if (i == 1 && hideSecondCard)
            {
                cardDisplay.SetHidden(true);
            }
            container.AddChild(cardDisplay);
        }
    }

    private void ClearCardContainer(VFlowContainer container)
    {
        foreach (Node child in container.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void UpdateScoreLabels()
    {
        var playerScore = player.CalculateScore();
        var dealerScore = dealer.CalculateScore();

        playerScoreLabel.Text = $"{playerScore.maxScore}/{playerScore.minScore}";
        dealerScoreLabel.Text = isDealerCardHidden ? "?" : $"{dealerScore.maxScore}/{dealerScore.minScore}";
    }
    private async Task ExecutePlayerAction(Func<Task> action)
    {
        if (isAnimating || !isGameInProgress) return; // Prevent action if animating or game not in progress

        isAnimating = true; // Indicate that an animation is currently playing
        await action(); // Execute the provided action
        isAnimating = false; // Reset animation state
    }
    public async void HandlePlayerHit()
    {
        if (isAnimating || !isGameInProgress) return;

        await ExecutePlayerAction(async () =>
        {
            await DealCardWithAnimation(player, playerCardContainer, "DrawingCard");
            UpdateScoreLabels();

            if (player.IsBusted())
            {
                EndGame("Player busted! Dealer's turn to play.");
                await DealerTurnAsync();
            }
            else if (player.IsTwentyOne())
            {
                EndGame("Player has 21! Dealer's turn to play.");
                await DealerTurnAsync();
            }
        });
    }

    private async void HandlePlayerStand()
    {
        if (isAnimating || !isGameInProgress) return;

        stoodButton.Disabled = true;
        isDealerCardHidden = false;

        UpdateHandDisplay(dealer, dealerCardContainer);
        UpdateScoreLabels();
        await DealerDrawCardsAsync();
        DetermineWinner();
    }

    private async Task DealerTurnAsync()
    {
        isDealerCardHidden = false; // Reveal the dealer's hidden card
        UpdateHandDisplay(dealer, dealerCardContainer);
        UpdateScoreLabels();

        await DealerDrawCardsAsync();
        DetermineWinner();
    }

    private async Task DealerDrawCardsAsync()
    {
        while (dealer.ShouldDrawCard())
        {
            await DealCardWithAnimation(dealer, dealerCardContainer, "DealerDrawCard");
            UpdateScoreLabels();
        }
    }

    private void DetermineWinner()
    {
        var playerScore = player.CalculateScore().maxScore;
        var dealerScore = dealer.CalculateScore().maxScore;

        int winner = GameLogic.DetermineWinner(playerScore, dealerScore);

        switch (winner)
        {
            case 1:
                EndGame("Player wins! Dealer busted.");
                player.WinBet(player.BetAmount); // Update player's money
                UpdatePlayerMoneyLabel();
                break;
            case 2:
                EndGame("Player wins!");
                player.WinBet(player.BetAmount); // Update player's money
                UpdatePlayerMoneyLabel();
                break;
            case 3:
                EndGame("Dealer wins!");
                UpdatePlayerMoneyLabel();
                break;
            case 4:
                EndGame("It's a tie!");
                player.ReturnBet(player.BetAmount); // Return the bet
                UpdatePlayerMoneyLabel();
                break;
        }
    }

    private void EndGame(string resultMessage)
    {
        resultLabel.Text = resultMessage;
        hitButton.Disabled = true;
        stoodButton.Disabled = true;
        playButton.Disabled = false; // Enable Play button at the end
        isGameInProgress = false; // Game ends
    }

    public void HandlePlay()
    {
        // Attempt to place a bet and start the game
        OnPlaceBetButtonPressed();
    }

    public void OnPlaceBetButtonPressed()
    {
        int betAmount;
        if (int.TryParse(betInput.Text, out betAmount))
        {
            if (player.Money >= betAmount && betAmount > 0) // Check for valid bet
            {
                player.PlaceBet(betAmount);
                UpdatePlayerMoneyLabel();
                StartGame(); // Start the game after placing a valid bet
            }
            else
            {
                GD.Print("Not enough money to place that bet or invalid bet amount.");
                resultLabel.Text = "Not enough money or invalid bet amount.";
            }
        }
        else
        {
            GD.Print("Invalid bet amount.");
            resultLabel.Text = "Invalid bet amount.";
        }
    }

    private void UpdatePlayerMoneyLabel()
    {
        playerMoneyLabel.Text = $"{player.Money}$"; // Update player's money label
    }

    // Button signals
    public void _on_hit_button_down() => HandlePlayerHit();
    public void _on_stood_button_down() => HandlePlayerStand();
    public void _on_play_btn_button_down() => HandlePlay();
}
