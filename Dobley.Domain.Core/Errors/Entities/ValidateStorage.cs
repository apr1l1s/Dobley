namespace Dobley.Domain.Core.Errors.Entities;

public class DomainValidateStorageException(string message) : Exception(message);