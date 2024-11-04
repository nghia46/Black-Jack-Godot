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
    [Export] private Label playerScoreLabel;
    [Export] private Label dealerScoreLabel;
    [Export] private Label resultLabel;
    [Export] private VFlowContainer playerCardContainer;
    [Export] private VFlowContainer dealerCardContainer;

    // Animation
    [Export] private AnimationPlayer animationPlayer;

    // Card scene
    private PackedScene cardDisplayScene = GD.Load<PackedScene>("res://Prefabs/card.tscn");

    // State and flags
    private bool isDealerCardHidden = true;
    private bool isAnimating = false;

    public override void _Ready()
    {
        StartGame();
    }

    private async void StartGame()
    {
        ResetGameUI();
        deck.CreateDeck();
        deck.ShuffleDeck();
        await DealInitialCardsWithAnimation();
        UpdateScoreLabels();
    }

    private void ResetGameUI()
    {
        hitButton.Disabled = false;
        stoodButton.Disabled = false;
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
        for (int i = 0; i < 2; i++)
        {
            await DealCardWithAnimation(player, playerCardContainer, "DrawingCard");
            await DealCardWithAnimation(dealer, dealerCardContainer, "DealerDrawCard");
        }
    }

    private async Task DealCardWithAnimation(Player recipient, VFlowContainer container, string animationName)
    {
        recipient.AddCard(deck.DrawCard());
        await PlayAnimation(animationName);
        UpdateHandDisplay(recipient, container, recipient == dealer && isDealerCardHidden);
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

    public async void HandlePlayerHit()
    {
        if (isAnimating) return;

        await ExecutePlayerAction(async () =>
        {
            await DealCardWithAnimation(player, playerCardContainer, "DrawingCard");
            UpdateScoreLabels();

            if (player.IsBusted())
            {
                EndGame("Dealer wins! Player busted.");
            }
        });
    }

    public async void HandlePlayerStand()
    {
        if (isAnimating) return;

        stoodButton.Disabled = true;
        isDealerCardHidden = false;

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
        resultLabel.Text = resultMessage;
        hitButton.Disabled = true;
        stoodButton.Disabled = true;
    }

    public void HandleNewGame()
    {
        StartGame();
    }

    private async Task ExecutePlayerAction(Func<Task> action)
    {
        hitButton.Disabled = true;
        stoodButton.Disabled = true;

        await action();

        hitButton.Disabled = false;
        stoodButton.Disabled = false;
    }

    // Button signals
    public void _on_hit_button_down() => HandlePlayerHit();
    public void _on_stood_button_down() => HandlePlayerStand();
    public void _on_new_game_btn_button_down() => HandleNewGame();
}
