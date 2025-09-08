using System.ComponentModel;
using AiChatClient.Common.Models;

namespace AiChatClient.Common;

public class InventoryService
{
	readonly IReadOnlyList<Wine> _wines =
	[
		new("Trefethen", "Petit Verdot", "Oak Knoll AVA of Napa Valley", new DateOnly(2019, 9, 1)),
		new("Trefethen", "Pinot Noir", "Oak Knoll AVA of Napa Valley", new DateOnly(2022, 9, 1)),
		new("Trefethen", "Red Blend", "Oak Knoll AVA of Napa Valley", new DateOnly(2021, 9, 1), "The Cowgirl and The Pilot"),
		new("Trefethen", "Merlot", "Oak Knoll AVA of Napa Valley", new DateOnly(2022, 9, 1)),
		new("Trefethen", "Cabernet Sauvignon", "Oak Knoll AVA of Napa Valley", new DateOnly(2022, 9, 1), "Block 296"),
		new("Trefethen", "Red Blend", "Oak Knoll AVA of Napa Valley", new DateOnly(2022, 9, 1), "OKD Two"),
		new("Matthiasson", "Pinot Muenier", "Oakville AVA of Napa Valley", new DateOnly(2023, 9, 1)),
		new("Matthiasson", "Pinot Muenier", "Oakville AVA of Napa Valley", new DateOnly(2023, 9, 1)),
		new("Matthiasson", "Pinot Muenier", "Oakville AVA of Napa Valley", new DateOnly(2023, 9, 1)),
		new("Matthiasson", "Pinot Muenier", "Oakville AVA of Napa Valley", new DateOnly(2023, 9, 1)),
		new("Matthiasson", "Pinot Muenier", "Oakville AVA of Napa Valley", new DateOnly(2023, 9, 1)),
		new("Matthiasson", "Pinot Muenier", "Oakville AVA of Napa Valley", new DateOnly(2023, 9, 1)),
		new("Matthiasson", "Merlot", "Los Carneros AVA of Napa Valley", new DateOnly(2022, 9, 1)),
	];

	[Description("Retrieves all wines currently in our inventory")]
	public IReadOnlyList<Wine> GetWines() => [.. _wines];
}