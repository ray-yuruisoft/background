using System;
using System.Collections.Generic;

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// URL���ȶ���, ʵ�ֹ���Ż�������Ż�����, ʵ��URLȥ��, ���Ҷ�����Ҫ�ɱ����
	/// ��������ԭ��, ����û�к�ISpider�Ľ���, ���ͬһ��Scheduler���ܱ���ͬ��Spider��ʹ��
	/// </summary>
	public interface IScheduler : IDisposable, IMonitorable
	{
		bool IsDistributed { get; }

		/// <summary>
		/// �Ƿ��������
		/// </summary>
		TraverseStrategy TraverseStrategy { get; set; }

		/// <summary>
		/// �������
		/// </summary>
		int Depth { get; set; }

		/// <summary>
		/// ��ʼ������
		/// </summary>
		/// <param name="spider">�������</param>
		void Init(ISpider spider);

		/// <summary>
		/// ���������󵽶���
		/// </summary>
		/// <param name="request">�������</param>
		void Push(Request request);

		/// <summary>
		/// ȡ��һ����Ҫ������������
		/// </summary>
		/// <returns>�������</returns>
		Request Poll();

		/// <summary>
		/// ��������
		/// </summary>
		/// <param name="requests">�������</param>
		void Import(IEnumerable<Request> requests);

		/// <summary>
		/// ������������
		/// </summary>
		void Export();
	}
}
