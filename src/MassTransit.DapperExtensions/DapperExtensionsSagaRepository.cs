// Copyright 2012 Zarion Ltd.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using DapperExtensions;
using MassTransit.Saga;
using MassTransit.Logging;
using MassTransit.Pipeline;
using MassTransit.Util;
using MassTransit.Exceptions;

namespace MassTransit.DESagaRepository
{
    public class DapperExtensionsSagaRepository<TSaga>
        : ISagaRepository<TSaga> where TSaga : class, ISaga
    {
        static readonly ILog _log = Logger.Get(typeof(DapperExtensionsSagaRepository<TSaga>).ToFriendlyName());

        private readonly IDbProvider dbProvider;

        public DapperExtensionsSagaRepository(IDbProvider dbProvider)
        {
            this.dbProvider = dbProvider;
        }

        public IEnumerable<Action<IConsumeContext<TMessage>>> GetSaga<TMessage>(IConsumeContext<TMessage> context, Guid sagaId,
                                                                        InstanceHandlerSelector<TSaga, TMessage> selector,
                                                                        ISagaPolicy<TSaga, TMessage> policy) where TMessage : class
        {
            using (var db = dbProvider.Open())
            using (var transaction = db.BeginTransaction())
            {
                TSaga instance = db.Get<TSaga>(sagaId, transaction);
                Console.WriteLine("Got instance: {0}", instance == null ? "no instance" : instance.ToString());
                if (instance == null)
                {
                    if (policy.CanCreateInstance(context))
                    {
                        yield return x =>
                        {
                            if (_log.IsDebugEnabled)
                                _log.DebugFormat("SAGA: {0} Creating New {1} for {2}", typeof(TSaga).ToFriendlyName(), sagaId,
                                    typeof(TMessage).ToFriendlyName());

                            try
                            {
                                instance = policy.CreateInstance(x, sagaId);

                                foreach (var callback in selector(instance, x))
                                {
                                    callback(x);
                                }

                                if (!policy.CanRemoveInstance(instance))
                                {
                                    try
                                    {
                                        db.Insert(instance, transaction);
                                    }
                                    catch (Exception)
                                    {
                                        // if insert fails update
                                        db.Update(instance, transaction);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var sex = new SagaException("Create Saga Instance Exception", typeof(TSaga), typeof(TMessage), sagaId, ex);
                                if (_log.IsErrorEnabled)
                                    _log.Error(sex);
                                //if(transaction.IsActive) - Transaction is always active
                                transaction.Rollback();

                                throw sex;
                            }
                        };
                    }
                    else
                    {
                        if (_log.IsDebugEnabled)
                            _log.DebugFormat("SAGA: {0} Ignoring Missing {1} for {2}", typeof(TSaga).ToFriendlyName(), sagaId,
                                typeof(TMessage).ToFriendlyName());
                    }
                }
                else
                {
                    Console.WriteLine("Have existing instance " + instance.ToString());
                    if (policy.CanUseExistingInstance(context))
                    {
                        yield return x =>
                        {
                            if (_log.IsDebugEnabled)
                                _log.DebugFormat("SAGA: {0} Using Existing {1} for {2}", typeof(TSaga).ToFriendlyName(), sagaId,
                                    typeof(TMessage).ToFriendlyName());

                            try
                            {
                                foreach (var callback in selector(instance, x))
                                {
                                    callback(x);
                                }

                                if (policy.CanRemoveInstance(instance))
                                    db.Delete(instance, transaction);
                                else
                                    db.Update(instance, transaction);
                            }
                            catch (Exception ex)
                            {
                                var sex = new SagaException("Existing Saga Instance Exception", typeof(TSaga), typeof(TMessage), sagaId, ex);
                                if (_log.IsErrorEnabled)
                                    _log.Error(sex);
                                //if(transaction.IsActive) - Transaction is always active
                                transaction.Rollback();

                                throw sex;
                            }
                        };
                    }
                    else
                    {
                        if (_log.IsDebugEnabled)
                            _log.DebugFormat("SAGA: {0} Ignoring Existing {1} for {2}", typeof(TSaga).ToFriendlyName(), sagaId,
                                typeof(TMessage).ToFriendlyName());
                    }
                }
                transaction.Commit();
            }
        }

        public IEnumerable<Guid> Find(ISagaFilter<TSaga> filter)
        {
            return Where(filter, x => x.CorrelationId);
        }

        public IEnumerable<TSaga> Where(ISagaFilter<TSaga> filter)
        {
            //using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            using (var session = dbProvider.Open())
            {
                List<TSaga> result = session.GetList<TSaga>().AsQueryable()
                    .Where(filter.FilterExpression)
                    .ToList();

                //scope.Complete();

                return result;
            }
        }

        public IEnumerable<TResult> Where<TResult>(ISagaFilter<TSaga> filter, Func<TSaga, TResult> transformer)
        {
            return Where(filter).Select(transformer);
        }

        public IEnumerable<TResult> Select<TResult>(Func<TSaga, TResult> transformer)
        {
            //using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            using (var session = dbProvider.Open())
            {
                List<TResult> result = session.GetList<TSaga>().AsQueryable()
                    .Select(transformer)
                    .ToList();

                //scope.Complete();

                return result;
            }
        }
    }
}

