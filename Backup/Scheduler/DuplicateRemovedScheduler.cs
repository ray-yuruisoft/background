using DotnetSpider.Core.Scheduler.Component;
using System.Collections.Generic;
using DotnetSpider.Core.Redial;

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// Remove duplicate urls and only push urls which are not duplicate.
	/// </summary>
	public abstract class DuplicateRemovedScheduler : BaseScheduler
	{
		/// <summary>
		/// ȥ����
		/// </summary>
		protected IDuplicateRemover DuplicateRemover { get; set; } = new HashSetDuplicateRemover();

		/// <summary>
		/// �ܵ�������
		/// </summary>
		public override long TotalRequestsCount => DuplicateRemover.TotalRequestsCount;


		/// <summary>
		/// Reset duplicate check.
		/// </summary>
		public abstract void ResetDuplicateCheck();


		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			DuplicateRemover.Dispose();
		}

		/// <summary>
		/// ������Ӳ����ظ��ľ���ӵ�������
		/// </summary>
		/// <param name="request">�������</param>
		protected abstract void PushWhenNoDuplicate(Request request);

		protected override void ImplPush(Request request)
		{
			if (!DuplicateRemover.IsDuplicate(request) || ShouldReserved(request))
			{
				PushWhenNoDuplicate(request);
			}
		}
	}
}