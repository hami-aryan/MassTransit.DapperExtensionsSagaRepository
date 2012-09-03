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
