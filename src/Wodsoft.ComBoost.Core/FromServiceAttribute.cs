﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Wodsoft.ComBoost
{
    /// <summary>
    /// 服务来源特性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class FromServiceAttribute : FromAttribute
    {
        /// <summary>
        /// 实例化服务来源特性。
        /// </summary>
        public FromServiceAttribute() { IsRequired = true; }

        /// <summary>
        /// 实例化服务来源特性。
        /// </summary>
        /// <param name="isRequired">是否必须存在值。</param>
        public FromServiceAttribute(bool isRequired) { IsRequired = isRequired; }

        /// <summary>
        /// 获取是否必须存在值。默认为True。
        /// </summary>
        public bool IsRequired { get; private set; }

        /// <summary>
        /// 获取值。
        /// </summary>
        /// <param name="domainContext">领域上下文。</param>
        /// <param name="parameter">参数信息。</param>
        /// <returns>返回值。</returns>
        public override object GetValue(IDomainContext domainContext, ParameterInfo parameter)
        {
            var service = domainContext.GetService(parameter.ParameterType);
            if (service == null)
                if (parameter.HasDefaultValue)
                    service = parameter.DefaultValue;
                else if (IsRequired)
                    throw new ArgumentNullException("获取" + parameter.Name + "参数的值为空。");
            return service;
        }
    }
}
