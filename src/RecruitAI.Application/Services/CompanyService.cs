using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Application.Services;

public class CompanyService : ICompanyService
{
	private readonly IUnitOfWork _uow;
	private readonly ILogger<CompanyService> _logger;

	public CompanyService(IUnitOfWork uow, ILogger<CompanyService> logger)
	{
		_uow = uow;
		_logger = logger;
	}

	public async Task<Company> CreateOrGetCompanyAsync(string companyName, string? website, string? address, Guid createdBy, CancellationToken cancellationToken = default)
	{
		var existingCompany = await _uow.Companies.GetByNameAsync(companyName, cancellationToken);

		if (existingCompany != null)
		{
			return existingCompany;
		}

		var newCompany = new Company
		{
			Id = Guid.NewGuid(),
			Name = companyName,
			Slug = GenerateSlug(companyName),
			Website = website,
			Address = address,
			CreatedAt = DateTime.UtcNow,
			CreatedBy = createdBy
		};

		await _uow.Companies.AddAsync(newCompany, cancellationToken);

		_logger.LogInformation("Created new company: {CompanyName}", companyName);

		return newCompany;
	}

	// Hàm tạo slug từ tên công ty
	private string GenerateSlug(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return string.Empty;

		var slug = name.ToLower().Trim();

		// Bỏ dấu tiếng Việt đơn giản
		var mapping = new Dictionary<char, string>
			{
				{'á', "a"}, {'à', "a"}, {'ả', "a"}, {'ã', "a"}, {'ạ', "a"},
				{'ă', "a"}, {'ắ', "a"}, {'ằ', "a"}, {'ẳ', "a"}, {'ẵ', "a"}, {'ặ', "a"},
				{'â', "a"}, {'ấ', "a"}, {'ầ', "a"}, {'ẩ', "a"}, {'ẫ', "a"}, {'ậ', "a"},
				{'đ', "d"},
				{'é', "e"}, {'è', "e"}, {'ẻ', "e"}, {'ẽ', "e"}, {'ẹ', "e"},
				{'ê', "e"}, {'ế', "e"}, {'ề', "e"}, {'ể', "e"}, {'ễ', "e"}, {'ệ', "e"},
				{'í', "i"}, {'ì', "i"}, {'ỉ', "i"}, {'ĩ', "i"}, {'ị', "i"},
				{'ó', "o"}, {'ò', "o"}, {'ỏ', "o"}, {'õ', "o"}, {'ọ', "o"},
				{'ô', "o"}, {'ố', "o"}, {'ồ', "o"}, {'ổ', "o"}, {'ỗ', "o"}, {'ộ', "o"},
				{'ơ', "o"}, {'ớ', "o"}, {'ờ', "o"}, {'ở', "o"}, {'ỡ', "o"}, {'ợ', "o"},
				{'ú', "u"}, {'ù', "u"}, {'ủ', "u"}, {'ũ', "u"}, {'ụ', "u"},
				{'ư', "u"}, {'ứ', "u"}, {'ừ', "u"}, {'ử', "u"}, {'ữ', "u"}, {'ự', "u"},
				{'ý', "y"}, {'ỳ', "y"}, {'ỷ', "y"}, {'ỹ', "y"}, {'ỵ', "y"}
			};

		foreach (var pair in mapping)
		{
			slug = slug.Replace(pair.Key, pair.Value[0]);
		}

		// Thay khoảng trắng bằng dấu gạch ngang, xóa ký tự đặc biệt
		slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
		slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");

		return slug;
	}
}