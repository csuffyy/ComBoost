﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Wodsoft.ComBoost
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class FromValueAttribute : FromAttribute
    {
        public FromValueAttribute() { IsRequired = true; }

        public FromValueAttribute(bool isRequired) { IsRequired = isRequired; }

        public bool IsRequired { get; private set; }

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
