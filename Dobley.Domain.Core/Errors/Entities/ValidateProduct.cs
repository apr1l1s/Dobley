namespace Dobley.Domain.Core.Errors.Entities;

public class DomainValidateProductException(string message)
    : DomainValidateException(message);
