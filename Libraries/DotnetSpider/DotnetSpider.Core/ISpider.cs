using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using System;
using System.Collections.Generic;
using System.Net;

namespace DotnetSpider.Core
{
	/// <summary>
	/// ����ӿڶ���
	/// </summary>
	public interface ISpider : IDisposable, IControllable, IAppBase
	{
		/// <summary>
		/// �ɼ�վ�����Ϣ����
		/// </summary>
		Site Site { get; }

		IScheduler Scheduler { get; }

		/// <summary>
		/// ������
		/// </summary>
		IDownloader Downloader { get; set; }

		/// <summary>
		/// ҳ�������
		/// </summary>
		IReadOnlyCollection<IPageProcessor> PageProcessors { get; }

		/// <summary>
		/// ���ݹܵ�
		/// </summary>
		IReadOnlyCollection<IPipeline> Pipelines { get; }

		/// <summary>
		/// ��ؽӿ�
		/// </summary>
		IMonitor Monitor { get; set; }
	}
}
