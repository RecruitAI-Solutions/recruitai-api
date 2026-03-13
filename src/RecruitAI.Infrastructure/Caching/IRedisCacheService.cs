// RecruitAI.Infrastructure/Caching/IRedisCacheService.cs
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RecruitAI.Infrastructure.Caching
{
	public interface IRedisCacheService
	{
		/// <summary>
		/// Lấy dữ liệu từ cache
		/// </summary>
		/// <typeparam name="T">Kiểu dữ liệu</typeparam>
		/// <param name="key">Key của cache</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Dữ liệu từ cache hoặc default nếu không có</returns>
		Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

		/// <summary>
		/// Lưu dữ liệu vào cache
		/// </summary>
		/// <typeparam name="T">Kiểu dữ liệu</typeparam>
		/// <param name="key">Key của cache</param>
		/// <param name="value">Giá trị cần lưu</param>
		/// <param name="expiration">Thời gian hết hạn</param>
		/// <param name="cancellationToken">Cancellation token</param>
		Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default);

		/// <summary>
		/// Xóa dữ liệu khỏi cache
		/// </summary>
		/// <param name="key">Key của cache</param>
		/// <param name="cancellationToken">Cancellation token</param>
		Task RemoveAsync(string key, CancellationToken cancellationToken = default);
	}
}