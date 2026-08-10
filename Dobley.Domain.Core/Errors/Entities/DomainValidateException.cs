namespace Dobley.Domain.Core.Errors.Entities;

public abstract class DomainValidateException(string message) : Exception(message);
