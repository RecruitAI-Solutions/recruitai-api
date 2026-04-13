using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Companies;
using RecruitAI.Application.DTOs.Responses.Companies;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;

namespace RecruitAI.Application.Commands.Companies;

public class UpdateCompanyCommand : IRequest<CompanyResponseDto>
{
	public Guid Id { get; set; }
	public string? Name { get; set; }
	public string? Logo { get; set; }
	public string? Address { get; set; }
	public string? Website { get; set; }
	public Guid UserId { get; set; }
}

public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyResponseDto>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;
	private readonly ILogger<UpdateCompanyCommandHandler> _logger;

	public UpdateCompanyCommandHandler(IUnitOfWork uow, IMapper mapper, ILogger<UpdateCompanyCommandHandler> logger)
	{
		_uow = uow;
		_mapper = mapper;
		_logger = logger;
	}

	public async Task<CompanyResponseDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
	{
		var company = await _uow.Companies.GetByIdAsync(request.Id, cancellationToken);

		if (company == null)
			throw new BusinessException(ErrorCode.ResourceNotFound, "Company not found");

		// Chỉ cho phép người tạo hoặc Admin mới được sửa
		var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken);
		var isAdmin = user?.Role == UserRole.ADMIN;

		if (company.CreatedBy != request.UserId && !isAdmin)
			throw new BusinessException(ErrorCode.Forbidden, "You don't have permission to update this company");

		if (!string.IsNullOrWhiteSpace(request.Name))
		{
			company.Name = request.Name;
			company.Slug = GenerateSlug(request.Name);
		}

		if (request.Logo != null)
			company.Logo = request.Logo;

		if (request.Address != null)
			company.Address = request.Address;

		if (request.Website != null)
			company.Website = request.Website;

		company.UpdatedAt = DateTime.UtcNow;

		_uow.Companies.Update(company);
		await _uow.SaveChangesAsync(cancellationToken);

		return _mapper.Map<CompanyResponseDto>(company);
	}

	private string GenerateSlug(string name)
	{
		var slug = name.ToLower().Trim();
		slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
		slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
		return slug;
	}
}