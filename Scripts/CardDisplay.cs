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
}
