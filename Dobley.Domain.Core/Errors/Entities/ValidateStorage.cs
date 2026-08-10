namespace Dobley.Domain.Core.Errors.Entities;

public class DomainValidateStorageException(string message)
    : DomainValidateException(message);
