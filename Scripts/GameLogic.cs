public partial class GameLogic
{
    public static int DetermineWinner(int playerScore, int dealerScore)
    {
        if (playerScore > 21)
        {
            if (dealerScore > 21)
            {
                return 4; // Tie
            }
            return 3; // Dealer wins
        }
        else if (dealerScore > 21)
        {
            return 1; // Player wins
        }
        else if (playerScore > dealerScore)
        {
            return 2; // Player wins
        }
        else if (playerScore < dealerScore)
        {
            return 3; // Dealer wins
        }
        return 4; // Tie
    }
}
