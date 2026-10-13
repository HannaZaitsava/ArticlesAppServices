using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ArticlesService.Application.Common.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse>(
     IUnitOfWork unitOfWork,
     ILogger<TransactionBehavior<TRequest, TResponse>> logger)
     : IPipelineBehavior<TRequest, TResponse>
     where TRequest : IRequest<TResponse>
    {       
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var response = default(TResponse);
            var typeName = request.GetGenericTypeName();

            try
            {
                if (request.GetType().Name.EndsWith("Query"))
                {
                    return await next(cancellationToken);
                }              

                return await unitOfWork.ExecuteInTransactionAsync(
                    action: async (ct) =>  // этот токен будет сгенерирован самой стратегией повторов
                    {
                        var transactionId = unitOfWork.CurrentTransactionId ?? Guid.Empty;

                        using (logger.BeginScope(new List<KeyValuePair<string, object>> { new("TransactionContext", transactionId) }))
                        {
                            logger.LogInformation("Begin transaction {TransactionId} for {CommandName} ({@Command})", transactionId, typeName, request);

                            response = await next(ct);

                            logger.LogInformation("Commit transaction {TransactionId} for {CommandName}", transactionId, typeName);
                        }

                        return response;
                    },
                    cancellationToken: cancellationToken);                                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error Handling transaction for {CommandName} ({@Command})", typeName, request);

                throw;
            }
        }
    }
}
