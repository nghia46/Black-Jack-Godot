using Godot;

[Tool]
public partial class Card : Resource
{
    [Export] public string Suit { get; set; }
    [Export] public string Rank { get; set; }
    
    // Exported property for the suit image
    public int GetValue()
    {
        return Rank switch
        {
            "A" => 11,
            "K" => 10,
            "Q" => 10,
            "J" => 10,
            _ => int.Parse(Rank)
        };
    }
}
