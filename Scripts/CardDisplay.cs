using Godot;

public partial class CardDisplay : Panel
{
	[Export] private Label rankLabel;
	[Export] private Label suitIconLabel;

	// Method to set card properties
	public void SetCard(string rank, string suit)
	{
		rankLabel.Text = rank;
		suitIconLabel.Text = suit;
	}
	 // Method to hide or show the card's details
    public void SetHidden(bool isHidden)
    {
        if (isHidden)
        {
            rankLabel.Text = "";  // Set to empty to "hide" the card
            suitIconLabel.Text = ""; // Alternatively, you could use placeholder text like "?"
        }
    }
}
