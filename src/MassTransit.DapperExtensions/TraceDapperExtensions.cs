#define USE_DB

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using DapperExtensions;
using System.Data;
using DapperExtensions.Mapper;

namespace MassTransit.DESagaRepository
{
#if USE_DB
    /// <summary>
    /// Workaround for 
    /// </summary>
    public static class TraceDapperExtensions
    {
        public static void Update<T>(this IDbConnection db, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            Console.WriteLine("Update {0}", entity);
            DapperExtensions.DapperExtensions.Update(db, entity, transaction, commandTimeout);
        }

        public static void Delete<T>(this IDbConnection db, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            Console.WriteLine("Delete {0}", entity);
            DapperExtensions.DapperExtensions.Delete(db, entity, transaction, commandTimeout);
        }

        public static void Insert<T>(this IDbConnection db, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            Console.WriteLine("Insert {0}", entity);
            DapperExtensions.DapperExtensions.Insert(db, entity, transaction, commandTimeout);
        }

        public static T Get<T>(this IDbConnection db, Guid id, IDbTransaction transaction = null, int? commandTimeout = null) where T:class
        {
            Console.WriteLine("Get {0}", id);
            return DapperExtensions.DapperExtensions.Get<T>(db, id, transaction, commandTimeout);
        }

        public static IEnumerable<T> GetList<T>(this IDbConnection db, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            Console.WriteLine("GetAll ");
            return DapperExtensions.DapperExtensions.GetList<T>(db, null, null, transaction, commandTimeout);
        }
    }
#else
    public static class TraceDapperExtensions
    {
        private static Dictionary<Guid, object> repo = new Dictionary<Guid, object>();
        public static void Update<T>(this IDbConnection db, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class, CorrelatedBy<Guid>
        {
            lock (repo)
            {
                Console.WriteLine("Update {0}", entity);
                //DapperExtensions.DapperExtensions.Update(db, entity, transaction, commandTimeout);
                repo[entity.CorrelationId] = entity;
            }
        }

        public static void Delete<T>(this IDbConnection db, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class, CorrelatedBy<Guid>
        {
            lock (repo)
            {
                Console.WriteLine("Delete {0}", entity);
                repo.Remove(entity.CorrelationId);
            }
        }

        public static void Insert<T>(this IDbConnection db, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class, CorrelatedBy<Guid>
        {
            lock (repo)
            {
                Console.WriteLine("Insert {0}", entity);
                repo.Add(entity.CorrelationId, entity);
            }
        }

        public static T Get<T>(this IDbConnection db, Guid id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class, CorrelatedBy<Guid>
        {
            lock (repo)
            {
                Console.WriteLine("Get {0}", id);
                object o;
                if (repo.TryGetValue(id, out o))
                    return (T)o;
                else
                    return null;
            }
        }

        public static IEnumerable<T> GetList<T>(this IDbConnection db, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            lock (repo)
            {
                Console.WriteLine("GetAll ");
                return repo.Select(kv => (T)kv.Value);
            }
        }
    }

#endif
}
