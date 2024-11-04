using System;
using System.Collections.Generic;
using Godot;

public partial class Deck : Node
{
    private readonly List<Card> cards = new List<Card>();
    private readonly Random random = new Random();

    // Create a new deck of cards
    public void CreateDeck()
    {
        string[] suits = { "♥️", "♦️", "♣️", "♠️" };
        string[] ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

        foreach (var suit in suits)
        {
            foreach (var rank in ranks)
            {
                cards.Add(new Card { Suit = suit, Rank = rank });
            }
        }
    }

    // Shuffle the deck using the Fisher-Yates algorithm
    public void ShuffleDeck()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (cards[j], cards[i]) = (cards[i], cards[j]);
        }
    }

    // Draw a card from the deck
    public Card DrawCard()
    {
        Card card = cards[0];
        cards.RemoveAt(0);
        return card;
    }
}
