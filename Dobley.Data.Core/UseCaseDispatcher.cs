using Dobley.Domain.Core.UseCases;
using MediatR;

namespace Dobley.Data.Core;

public class UseCaseDispatcher(IMediator mediator)
    : IUseCaseDispatcher
{
    public Task<TOut> DispatchAsync<TOut>(IUseCase<TOut> useCase, CancellationToken cancellationToken)
        => mediator.Send(useCase, cancellationToken);

    public Task DispatchAsync(IUseCase useCase, CancellationToken cancellationToken)
        => mediator.Send(useCase, cancellationToken);
}