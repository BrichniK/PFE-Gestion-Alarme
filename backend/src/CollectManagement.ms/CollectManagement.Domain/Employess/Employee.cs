using CollectManagement.Domain.Common;
using CollectManagement.Domain.Employess.ObjectValues;

namespace CollectManagement.Domain.Employess;

public class Employee : AuditableEntity
{
    public EmployeeId EmployeeId { get; private set; }
    
    public string Nom { get; private set; }

    public string Prenom { get; private set; }
    
    public int Phone { get; private set; }
    
    public string Rfid { get; private set; }
    
    public string? Email { get; private set; }
    
    public string? LogoPath { get; private set; }

    private Employee(
        EmployeeId employeeId,
        string nom,
        string prenom,
        int phone,
        string rfid,
        string? email,
        string? logoPath)
    {
        EmployeeId = employeeId;
        Nom = nom;
        Prenom = prenom;
        Phone = phone;
        Rfid = rfid;
        Email = email;
        LogoPath = logoPath;
    }

    public static Employee Create(
        EmployeeId employeeId,
        string nom,
        string prenom,
        int phone,
        string rfid,
        string? email,
        string? logoPath)
    {
        return new Employee(
            employeeId,
            nom,
            prenom,
            phone,
            rfid,
            email,
            logoPath);
    }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Employee() { }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}