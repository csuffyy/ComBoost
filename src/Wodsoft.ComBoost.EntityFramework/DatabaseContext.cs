﻿using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wodsoft.ComBoost.Data.Entity;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Wodsoft.ComBoost.Data.Entity
{
    public class DatabaseContext : IDatabaseContext
    {
        private static ConcurrentDictionary<Type, IEnumerable<Type>> _CachedSupportTypes;
        private Dictionary<Type, object> _CachedEntityContext;

        static DatabaseContext()
        {
            _CachedSupportTypes = new ConcurrentDictionary<Type, IEnumerable<Type>>();
        }

        public DbContext InnerContext { get; private set; }

        public IEnumerable<Type> SupportTypes { get; private set; }

        public DatabaseContext(DbContext context)
        {
            _CachedEntityContext = new Dictionary<Type, object>();
            ((IObjectContextAdapter)context).ObjectContext.ObjectMaterialized += ObjectContext_ObjectMaterialized;
            InnerContext = context;
            SupportTypes = _CachedSupportTypes.GetOrAdd(context.GetType(), type =>
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(t => t.CanRead && t.CanWrite && t.PropertyType.IsConstructedGenericType && t.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    .Select(t => t.PropertyType.GetGenericArguments()[0]).ToList();
                return new System.Collections.ObjectModel.ReadOnlyCollection<Type>(properties);
            });
        }

        private void ObjectContext_ObjectMaterialized(object sender, System.Data.Entity.Core.Objects.ObjectMaterializedEventArgs e)
        {
            IEntity entity = e.Entity as IEntity;
            if (entity == null)
                return;
            object context;
            Type type = e.Entity.GetType();
            if (_CachedEntityContext.ContainsKey(type))
                context = _CachedEntityContext[type];
            else
            {
                context = this.GetDynamicContext(type);
                _CachedEntityContext.Add(type, context);
            }
            entity.EntityContext = (IEntityQueryContext<IEntity>)context;
        }

        public Task<int> SaveAsync()
        {
            return InnerContext.SaveChangesAsync();
        }

        public IEntityContext<T> GetContext<T>() where T : class, IEntity, new()
        {
            if (_CachedEntityContext.ContainsKey(typeof(T)))
                return (EntityContext<T>)_CachedEntityContext[typeof(T)];
            var context = new EntityContext<T>(this, InnerContext.Set<T>());
            _CachedEntityContext.Add(typeof(T), context);
            return context;
        }
    }
}