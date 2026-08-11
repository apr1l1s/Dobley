namespace Dobley.Domain.Core.Errors.Entities;

public class DomainValidateNotificationException(string message)
    : DomainValidateException(message);
