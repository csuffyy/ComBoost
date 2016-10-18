﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Wodsoft.ComBoost
{
    /// <summary>
    /// 值提供器来源特性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class FromValueAttribute : FromAttribute
    {
        /// <summary>
        /// 实例化值提供器来源特性。
        /// </summary>
        public FromValueAttribute() { IsRequired = true; }

        /// <summary>
        /// 实例化值提供器来源特性。
        /// </summary>
        /// <param name="isRequired">是否必须存在值。</param>
        public FromValueAttribute(bool isRequired) { IsRequired = isRequired; }

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
            IValueProvider provider = domainContext.GetRequiredService<IValueProvider>();
            object value = provider.GetValue(parameter.Name, parameter.ParameterType);
            if (value == null)
                if (parameter.HasDefaultValue)
                    value = parameter.DefaultValue;
                else if (IsRequired)
                    throw new ArgumentNullException("获取" + parameter.Name + "参数的值为空。");
            return value;
        }
    }
}