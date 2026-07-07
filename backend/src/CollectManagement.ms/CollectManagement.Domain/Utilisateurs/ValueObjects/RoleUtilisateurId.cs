using CollectManagement.Domain.Common;

namespace CollectManagement.Domain.Utilisateurs.ValueObjects;

public record RoleUtilisateurId(Ulid Value) : IStronglyTypedId;