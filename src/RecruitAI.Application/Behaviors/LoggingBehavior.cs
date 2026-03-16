using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace RecruitAI.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{
	private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

	public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
	{
		_logger = logger;
	}

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		var requestName = typeof(TRequest).Name;
		var requestId = Guid.NewGuid().ToString();

		_logger.LogInformation(
			"[{RequestId}] Processing request {RequestName} at {Time}",
			requestId,
			requestName,
			DateTime.UtcNow);

		var stopwatch = Stopwatch.StartNew();

		try
		{
			var response = await next();

			stopwatch.Stop();

			_logger.LogInformation(
				"[{RequestId}] Completed request {RequestName} in {ElapsedMs}ms",
				requestId,
				requestName,
				stopwatch.ElapsedMilliseconds);

			return response;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();

			_logger.LogError(
				ex,
				"[{RequestId}] Error processing request {RequestName} after {ElapsedMs}ms: {ErrorMessage}",
				requestId,
				requestName,
				stopwatch.ElapsedMilliseconds,
				ex.Message);

			throw;
		}
	}
}