using System.Text.Json.Serialization;

public class Member
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("birthday")]
    public DateOnly Birthday { get; set; }

    [JsonPropertyName("giftIdea")]
    public string GiftIdea { get; set; } = default!;

    [JsonPropertyName("avoidMembers")]
    public List<string> AvoidMembers { get; set; } = new List<string>();

    [JsonPropertyName("giveToName")]
    public string GiveToName { get; set; } = default!;

    [JsonPropertyName("giveToGiftIdea")]
    public string GiveToGiftIdea { get; set; } = default!;

    public string GetUniqueName(string familyName) => $"{familyName}/{Name}";
}
