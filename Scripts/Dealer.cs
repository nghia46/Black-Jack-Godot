using Godot;

public partial class Dealer : Player
{
    public bool ShouldDrawCard()
    {
        var (maxScore, _) = CalculateScore();
        return maxScore < 17;
    }
}