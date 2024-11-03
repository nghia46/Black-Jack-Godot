using Godot;

public partial class BlackjackGame : Node
{
    private Deck deck;
    private Player player;
    private Dealer dealer;
    // Button 
    private Button hitButton;
    private Button stoodButton;

    [Export] public RichTextLabel PlayerCardLabel;
    [Export] public RichTextLabel DealerCardLabel;

    [Export] public RichTextLabel MessageLabel;

    public override void _Ready()
    {
        deck = GetNode<Deck>("Deck");
        player = GetNode<Player>("Player");
        dealer = GetNode<Dealer>("Dealer");
        PlayerCardLabel = GetNode<RichTextLabel>("UI/Panel/PlayerCardTxt");
        DealerCardLabel = GetNode<RichTextLabel>("UI/Panel/DealerCardTxt");
        MessageLabel = GetNode<RichTextLabel>("UI/Panel/MessageTxt");
        // Button
        hitButton = GetNode<Button>("UI/Panel/Hit");
        stoodButton = GetNode<Button>("UI/Panel/Stood");
        StartGame();
    }

    private void StartGame()
    {
        stoodButton.Disabled = false;
        hitButton.Disabled = false;
        deck.CreateDeck();
        deck.ShuffleDeck();
        player.AddCard(deck.DrawCard());
        player.AddCard(deck.DrawCard());
        dealer.AddCard(deck.DrawCard());
        dealer.AddCard(deck.DrawCard());
        UpdateCardLabel();
    }
    public void DetermineWinner()
    {
        var (playerMaxScore, playerMinScore) = player.CalculateScore();
        var (dealerMaxScore, dealerMinScore) = dealer.CalculateScore();

        if (playerMaxScore > 21)
        {
            MessageLabel.Text = "Player busted! Dealer wins!";
        }
        else if (dealerMaxScore > 21)
        {
            MessageLabel.Text = "Dealer busted! Player wins!";
        }
        else if (playerMaxScore == dealerMaxScore)
        {
            MessageLabel.Text = "It's a tie!";
        }
        else if (playerMaxScore > dealerMaxScore)
        {
            MessageLabel.Text = "Player wins!";
        }
        else
        {
            MessageLabel.Text = "Dealer wins!";
        }

    }
    public void UpdateCardLabel()
    {
        PlayerCardLabel.Text = "";
        DealerCardLabel.Text = "";
        foreach (var card in player.Hand)
        {
            PlayerCardLabel.Text += $"{card.Rank}/{card.Suit}, ";
        }

        foreach (var card in dealer.Hand)
        {
            DealerCardLabel.Text += $"{card.Rank}/{card.Suit}, ";
        }

        PlayerCardLabel.Text += $"({player.CalculateScore().maxScore}, {player.CalculateScore().minScore})";
        DealerCardLabel.Text += $"({dealer.CalculateScore().maxScore}, {dealer.CalculateScore().minScore})";
    }
    // Called when the Hit button is pressed
    public void _on_hit_button_down()
    {
        player.AddCard(deck.DrawCard());
        if (player.IsBusted())
        {
            MessageLabel.Text = "Player busted! Dealer wins!";
            stoodButton.Disabled = true;
            hitButton.Disabled = true;
        }
        UpdateCardLabel();
    }
    public void _on_stood_button_down()
    {
        stoodButton.Disabled = true;
        hitButton.Disabled = true;
        player.HasStood = true;
        while (dealer.ShouldDrawCard())
        {
            dealer.AddCard(deck.DrawCard());
        }
        UpdateCardLabel();
        DetermineWinner();
    }

    public void _on_new_game_btn_button_down()
    {
        player.ClearHand();
        dealer.ClearHand();
        StartGame();
        MessageLabel.Text = "";
    }

}
