using System;
using System.Collections.Generic;
using Godot;

public partial class Player : Node
{
    public List<Card> Hand { get; private set; } = new List<Card>();
    public bool HasStood { get; set; } = false;
    
    // Method to add a card to the player's hand
    public void AddCard(Card card)
    {
        Hand.Add(card);
    }

    // Calculate the score of the player's hand returning the total score
    public (int maxScore, int minScore) CalculateScore()
    {
        int score = 0;
        int aceCount = 0;

        // Calculate the score of the player's hand and count the number of aces
        foreach (var card in Hand)
        {
            score += card.GetValue();
            if (card.Rank == "A") aceCount++;
        }

        // Calculate the minimum score by counting all aces as 1
        int minScore = score - (aceCount * 10); // Each ace counted as 1 instead of 11

        // The max score will be the score with all aces treated as 11 if possible
        int maxScore = score;

        // If max score is greater than 21, we reduce the value of each ace to 1 until it's <= 21
        while (maxScore > 21 && aceCount > 0)
        {
            maxScore -= 10;
            aceCount--;
        }

        return (maxScore, minScore);
    }

    public bool IsBusted()
    {
        var (maxScore, _) = CalculateScore();
        return maxScore > 21;
    }

    public bool IsTwentyOne()
    {
        return CalculateScore().maxScore == 21;
    }

    public bool IsBlackJack()
    {
        return Hand.Count == 2 && CalculateScore().maxScore == 21;
    }

    public void ClearHand()
    {
        Hand.Clear();
    }
}
