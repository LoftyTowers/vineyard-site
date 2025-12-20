namespace VineyardApi.Models.Requests;

public record RevertRequest(Guid Id, Guid ChangedById);
