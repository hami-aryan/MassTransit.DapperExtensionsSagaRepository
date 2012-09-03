using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MassTransit;

namespace SagaRunner
{
    public interface NoddyMessage
        :CorrelatedBy<Guid>
    {
        string Text { get; }
    }
    public interface BigEarsMessage
        : CorrelatedBy<Guid>
    {
        string Text { get; }
    }
    public interface DriveLittleRedCarMessage
        : CorrelatedBy<Guid>
    {
        string Route { get; }
    }

    public class NoddyMessageImpl
        :NoddyMessage
    {
        public Guid CorrelationId { get; set; }
        public string Text { get; set; }
    }
    public class BigEarsMessageImpl
        :BigEarsMessage
    {
        public Guid CorrelationId { get; set; }
        public string Text { get; set; }
    }
    public class DriveLittleRedCarMessageImpl
        :DriveLittleRedCarMessage
    {
        public Guid CorrelationId { get; set; }
        public string Route { get; set; }
    }
}
