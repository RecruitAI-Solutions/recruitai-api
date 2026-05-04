using AutoMapper;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Domain.Entities;
using RecruitAI.Shared.Interfaces;

namespace RecruitAI.Application.Mappings.Resolvers
{
	public class JobStatusResolver : IValueResolver<Job, object, string>
	{
		private readonly IMessageService _msg;

		public JobStatusResolver(IMessageService msg)
		{
			_msg = msg;
		}

		public string Resolve(Job source, object destination, string destMember, ResolutionContext context)
		{
			return _msg.GetJobStatusDisplay(source.Status);
		}
	}
}