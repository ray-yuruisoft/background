using DotnetSpider.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// Basic Scheduler implementation.
	/// </summary>
	public class QueueDuplicateRemovedScheduler : DuplicateRemovedScheduler
	{
		private readonly object _lock = new object();
		private List<Request> _queue = new List<Request>();
		private readonly AutomicLong _successCounter = new AutomicLong(0);
		private readonly AutomicLong _errorCounter = new AutomicLong(0);

		public override bool IsDistributed => false;

		/// <summary>
		/// �Ƿ��ʹ�û�����
		/// </summary>
		protected override bool UseInternet { get; set; } = false;

		/// <summary>
		/// ������Ӳ����ظ��ľ���ӵ�������
		/// </summary>
		/// <param name="request">�������</param>
		protected override void PushWhenNoDuplicate(Request request)
		{
			lock (_lock)
			{
				_queue.Add(request);
			}
		}

		/// <summary>
		/// Reset duplicate check.
		/// </summary>
		public override void ResetDuplicateCheck()
		{
			lock (_lock)
			{
				_queue.Clear();
			}
		}

		/// <summary>
		/// ȡ��һ����Ҫ������������
		/// </summary>
		/// <returns>�������</returns>
		public override Request Poll()
		{
			lock (_lock)
			{
				if (_queue.Count == 0)
				{
					return null;
				}
				else
				{
					Request request;
					switch (TraverseStrategy)
					{
						case TraverseStrategy.DFS:
							{
								request = _queue.Last();
								_queue.RemoveAt(_queue.Count - 1);
								break;
							}
						case TraverseStrategy.BFS:
							{
								request = _queue.First();
								_queue.RemoveAt(0);
								break;
							}
						default:
							{
								throw new NotImplementedException();
							}
					}

					return request;
				}
			}
		}

		/// <summary>
		/// ʣ��������
		/// </summary>
		public override long LeftRequestsCount
		{
			get
			{
				lock (_lock)
				{
					return _queue.Count;
				}
			}
		}

		/// <summary>
		/// �ɼ��ɹ���������
		/// </summary>
		public override long SuccessRequestsCount => _successCounter.Value;

		/// <summary>
		/// �ɼ�ʧ�ܵĴ���, ����������, ���һ�����Ӳɼ���ζ�ʧ�ܻ��¼���
		/// </summary>
		public override long ErrorRequestsCount => _errorCounter.Value;

		/// <summary>
		/// �ɼ��ɹ����������� 1
		/// </summary>
		public override void IncreaseSuccessCount()
		{
			_successCounter.Inc();
		}

		/// <summary>
		/// �ɼ�ʧ�ܵĴ����� 1
		/// </summary>
		public override void IncreaseErrorCount()
		{
			_errorCounter.Inc();
		}

		/// <summary>
		/// ��������
		/// </summary>
		/// <param name="requests">�������</param>
		public override void Import(IEnumerable<Request> requests)
		{
			lock (_lock)
			{
				_queue = new List<Request>(requests);
			}
		}

		/// <summary>
		/// ȡ�ö��������е��������
		/// </summary>
		public IReadOnlyCollection<Request> All
		{
			get
			{
				lock (_lock)
				{
					return new ReadOnlyEnumerable<Request>(_queue.ToArray());
				}
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			lock (_lock)
			{
				_successCounter.Set(0);
				_errorCounter.Set(0);
				_queue.Clear();
			}

			base.Dispose();
		}
	}
}