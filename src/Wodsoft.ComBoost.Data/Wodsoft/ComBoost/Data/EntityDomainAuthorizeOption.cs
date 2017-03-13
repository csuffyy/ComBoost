﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wodsoft.ComBoost.Data.Entity.Metadata;
using Wodsoft.ComBoost.Security;

namespace Wodsoft.ComBoost.Data
{
    public class EntityDomainAuthorizeOption
    {
        public static readonly EntityDomainAuthorizeOption Empty = new EntityDomainAuthorizeOption();
        public static readonly EntityDomainActionAuthorizeOption View = new EntityDomainActionAuthorizeOption(metadata => metadata.ViewRoles, metadata => metadata.ViewProperties);
        public static readonly EntityDomainActionAuthorizeOption Create = new EntityDomainActionAuthorizeOption(metadata => metadata.AddRoles, metadata => metadata.CreateProperties);
        public static readonly EntityDomainActionAuthorizeOption Edit = new EntityDomainActionAuthorizeOption(metadata => metadata.EditRoles, metadata => metadata.EditProperties);
        public static readonly EntityDomainActionAuthorizeOption Remove = new EntityDomainActionAuthorizeOption(metadata => metadata.RemoveRoles, metadata => new IPropertyMetadata[0]);
        public static readonly EntityDomainActionAuthorizeOption Detail = new EntityDomainActionAuthorizeOption(metadata => metadata.ViewRoles, metadata => metadata.DetailProperties);

        public virtual void Validate(IEntityMetadata metadata, IAuthentication authentication)
        {
            if (!metadata.AllowAnonymous && !authentication.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("未登录。");
        }

        public virtual IEnumerable<IPropertyMetadata> GetProperties(IEntityMetadata metadata, IAuthentication authentication)
        {
            return metadata.Properties;
        }
    }

    public class EntityDomainActionAuthorizeOption : EntityDomainAuthorizeOption
    {
        public EntityDomainActionAuthorizeOption(Func<IEntityMetadata, IEnumerable<object>> rolesSelector, Func<IEntityMetadata, IEnumerable<IPropertyMetadata>> propertiesSelector)
        {
            if (rolesSelector == null)
                throw new ArgumentNullException(nameof(rolesSelector));
            if (propertiesSelector == null)
                throw new ArgumentNullException(nameof(propertiesSelector));
            RolesSelector = rolesSelector;
            PropertiesSelector = propertiesSelector;
        }

        public Func<IEntityMetadata, IEnumerable<object>> RolesSelector { get; private set; }

        public Func<IEntityMetadata, IEnumerable<IPropertyMetadata>> PropertiesSelector { get; private set; }

        public override void Validate(IEntityMetadata metadata, IAuthentication authentication)
        {
            base.Validate(metadata, authentication);
            if (metadata.AuthenticationRequiredMode == AuthenticationRequiredMode.All)
            {
                if (RolesSelector(metadata).Any(t => !authentication.IsInRole(t)))
                    throw new UnauthorizedAccessException("权限不足。");
            }
            else
            {
                if (RolesSelector(metadata).All(t => !authentication.IsInRole(t)))
                    throw new UnauthorizedAccessException("权限不足。");
            }
        }

        public override IEnumerable<IPropertyMetadata> GetProperties(IEntityMetadata metadata, IAuthentication authentication)
        {
            return PropertiesSelector(metadata).Where(t =>
           {
               if (!t.AllowAnonymous && !authentication.Identity.IsAuthenticated)
                   return false;
               if (t.AuthenticationRequiredMode == AuthenticationRequiredMode.All)
                   return t.ViewRoles.All(r => authentication.IsInRole(r));
               else
                   return t.ViewRoles.Any(r => authentication.IsInRole(r));
           });
        }
    }
}
