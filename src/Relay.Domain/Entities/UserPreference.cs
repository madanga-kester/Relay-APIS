namespace Relay.Domain.Entities;

public sealed class UserPreference : AuditableEntity
{
    private UserPreference() { }

    public UserPreference(Guid userId, string valuesJson)
    {
        UserId = userId;
        ValuesJson = valuesJson;
    }

    public Guid UserId { get; private set; }
    public string ValuesJson { get; private set; } = "{}";

    public void Update(string valuesJson, DateTimeOffset now)
    {
        ValuesJson = valuesJson;
        Touch(now);
    }
}