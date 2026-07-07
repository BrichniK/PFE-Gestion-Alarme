using CollectManagement.Domain.Common;

namespace CollectManagement.Domain.Utilisateurs.Enums;

public enum NavigationAction
{
    [DisplayAj(Name = "Ajouter")]
    Add,
    [DisplayAj(Name = "Modifier")]
    Edit,
    [DisplayAj(Name = "Supprimer")]
    Delete,
    [DisplayAj(Name = "Imprimer")]
    Print,
    [DisplayAj(Name = "Dupliquer")]
    Duplicate,
}