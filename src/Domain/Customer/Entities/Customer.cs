// =============================================================================
// DDD - Aggregate Root: Customer (Bounded Context: Customer)
// OO - Herança: Customer → AggregateRoot → Entity
// OO - Encapsulamento: dados pessoais protegidos por setters privados.
// SOLID - SRP: gerencia dados e ciclo de vida do cliente.
// =============================================================================

using Domain.Common;
using Domain.Customer.ValueObjects;

namespace Domain.Customer.Entities;

/// <summary>
/// Aggregate Root do Bounded Context de Clientes.
/// Representa um cliente do e-commerce.
/// </summary>
public sealed class Customer : AggregateRoot
{
    public string FullName { get; private set; }
    public Email Email { get; private set; }
    public Cpf Cpf { get; }
    public DateTime BirthDate { get; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; }

    public Customer(Guid id, string fullName, Email email, Cpf cpf, DateTime birthDate) : base(id)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Nome completo é obrigatório.");
        if (email is null) throw new ArgumentNullException(nameof(email));
        if (cpf is null) throw new ArgumentNullException(nameof(cpf));

        var age = DateTime.Today.Year - birthDate.Year;
        if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;
        if (age < 18)
            throw new InvalidOperationException("Cliente deve ter pelo menos 18 anos.");

        FullName = fullName.Trim();
        Email = email;
        Cpf = cpf;
        BirthDate = birthDate.Date;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateEmail(Email newEmail)
    {
        Email = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
    }

    public void UpdateFullName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Nome não pode ser vazio.");
        FullName = newName.Trim();
    }

    public void Activate()  => IsActive = true;
    public void Deactivate() => IsActive = false;
}
