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
using MassTransit;
using MassTransit.Saga;
using log4net;
using Magnum.StateMachine;

namespace SagaRunner
{
    public class NoddySagaMap
    : DapperExtensions.Mapper.ClassMapper<NoddySaga>
    {
        public NoddySagaMap()
        {
            Table("NoddySaga");
            Map(s => s.CorrelationId).Column("CorrelationId").Key(DapperExtensions.Mapper.KeyType.Assigned);
            Map(s => s.NoddyText).Column("NoddyText");
            Map(s => s.BigEarText).Column("BigEarText");
            Map(s => s.CurrentState.Enter.Name).Column("StateEnter");
            Map(s => s.CurrentState.Leave.Name).Column("StateLeave");
            Map(s => s.CurrentState.Name).Column("StateName");
        }
    }

    public class NoddySaga
        : SagaStateMachine<NoddySaga>,
          ISaga
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NoddySaga));

        static NoddySaga()
        {
            Define(() =>
                {
                    Initially(
                        When(Noddy)
                        .Then((saga, msg) =>
                            {
                                saga.NoddyText = msg.Text;
                                Console.WriteLine("When(Noddy) {0} Have Noddy text {1}", msg.CorrelationId, saga.NoddyText);
                            })
                        .TransitionTo(Open)
                        );

                    During(Open,
                        When(BigEars)
                        .Then((saga, msg) =>
                            {
                                saga.BigEarText = msg.Text;
                                Console.WriteLine("When(BigEars) Have Big Ears text " + saga.BigEarText);
                            })
                        );

                    Combine(Noddy, BigEars).Into(GoDrive, saga => saga.ReadyFlags);

                    During(Open,
                        When(GoDrive)
                        .Publish(saga =>
                            {
                                Console.WriteLine("When(GoDrive) Publishing DriveLittleRedCarMessage");
                                return new DriveLittleRedCarMessageImpl { CorrelationId = saga.CorrelationId, Route = saga.NoddyText + " " + saga.BigEarText };
                            })
                        .TransitionTo(Completed)
                        );
                });
        }
        public static State Initial { get; set; }
        public static State Open { get; set; }
        public static State Completed { get; set; }

        public static Event<NoddyMessage> Noddy { get; set; }
        public static Event<BigEarsMessage> BigEars { get; set; }
        public static Event GoDrive { get; set; }

        public string NoddyText { get; set; }
        public string BigEarText { get; set; }
        public Guid CorrelationId { get; private set; }

        public int ReadyFlags { get; set; }

        public MassTransit.IServiceBus Bus { get; set; }

        public NoddySaga(Guid correlationId)
            : this()
        {
            CorrelationId = correlationId;
        }
        public NoddySaga()
        {
            NoddyText = string.Empty;
            BigEarText = string.Empty;
        }

        public override string ToString()
        {
            return "Id " + CorrelationId.ToString() + " NoddyText = '" + NoddyText + "' BigEarText = '" + BigEarText + "'";
        }
    }
}
