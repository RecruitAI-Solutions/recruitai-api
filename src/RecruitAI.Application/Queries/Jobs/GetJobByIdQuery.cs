using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Domain.Interfaces;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Application.Queries.Jobs;

public class GetJobByIdQuery : IRequest<JobDetailDto>
{
	public Guid Id { get; set; }
}

public class GetJobByIdQueryHandler : IRequestHandler<GetJobByIdQuery, JobDetailDto>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;
	private readonly ILogger<GetJobByIdQueryHandler> _logger;

	public GetJobByIdQueryHandler(
		IUnitOfWork uow,
		IMapper mapper,
		ILogger<GetJobByIdQueryHandler> logger)
	{
		_uow = uow;
		_mapper = mapper;
		_logger = logger;
	}

	public async Task<JobDetailDto> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation("Getting job by ID: {JobId}", request.Id);

			var job = await _uow.Jobs.GetByIdAsync(request.Id, cancellationToken);

			if (job == null)
			{
				throw new BusinessException(
					ErrorCode.ResourceNotFound,
					"Không tìm thấy công việc");
			}

			// Tăng lượt xem (có thể thực hiện ở background)
			job.IncrementViews();
			await _uow.SaveChangesAsync(cancellationToken);

			return _mapper.Map<JobDetailDto>(job);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting job by ID: {JobId}", request.Id);
			throw;
		}
	}
}