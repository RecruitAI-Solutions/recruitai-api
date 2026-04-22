using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.CVs;
using RecruitAI.Application.DTOs.Responses;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Common.CVs;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;
using System;

namespace RecruitAI.Application.Queries.CVs;

public class GetUserCVsQuery : IRequest<PaginationResponseDto<CVListDto>>
{
	public Guid UserId { get; set; }
	public CVFilterDto Filter { get; set; } = new();
}

public class GetUserCVsQueryHandler : IRequestHandler<GetUserCVsQuery, PaginationResponseDto<CVListDto>>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;
	private readonly ILogger<GetUserCVsQueryHandler> _logger;
	private readonly IMessageService _msg;

	public GetUserCVsQueryHandler(
		IUnitOfWork uow,
		IMapper mapper,
		ILogger<GetUserCVsQueryHandler> logger,
		IMessageService messageService)
	{
		_uow = uow;
		_mapper = mapper;
		_logger = logger;
		_msg = messageService;
	}

	public async Task<PaginationResponseDto<CVListDto>> Handle(GetUserCVsQuery request, CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation(_msg.Log("GettingUserCVs"), request.UserId);

			// Map từ DTO sang Domain Filter
			var domainFilter = new CVFilter
			{
				Status = request.Filter.Status,
				FromDate = request.Filter.FromDate,
				ToDate = request.Filter.ToDate,
				FileName = request.Filter.FileName,
				SortBy = request.Filter.SortBy,
				SortOrder = request.Filter.SortOrder ?? "desc",
				Page = request.Filter.Page > 0 ? request.Filter.Page : 1,
				PageSize = request.Filter.PageSize > 0 ? request.Filter.PageSize : 10
			};

			(IEnumerable<CVList> cvs, int total) = await _uow.CVs.GetUserCVsAsync(
			   userId: request.UserId,
			   filter: domainFilter,
			   cancellationToken: cancellationToken);

			var cvDtos = _mapper.Map<List<CVListDto>>(cvs);

			_logger.LogInformation(_msg.Log("FoundUserCVs"), total, request.UserId);

			return new PaginationResponseDto<CVListDto>
			{
				Data = cvDtos,
				Total = total,
				Page = domainFilter.Page,
				PageSize = domainFilter.PageSize
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, _msg.Log("GetUserCVsError"), request.UserId);
			throw;
		}
	}
}