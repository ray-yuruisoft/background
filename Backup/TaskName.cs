﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 任务名称
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class TaskName : Attribute
	{
		/// <summary>
		/// 任务名称
		/// </summary>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="name">任务名称</param>
		public TaskName(string name)
		{
			Name = name;
		}
	}
}
