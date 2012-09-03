using System;
using Dapper;
using MassTransit;
using MassTransit.Saga;
using MassTransit.DESagaRepository;
using System.Configuration;
using System.Threading;
namespace SagaRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MassTransitNoddy"].ConnectionString;

            var dbProvider = new DbProvider(connectionString);

            DropnCreateSagaTable(dbProvider);

            Bus.Initialize(b =>
            {
                b.UseRabbitMq();
                b.UseRabbitMqRouting();
                b.ReceiveFrom("rabbitmq://localhost/OptimusServiceConsole");
                b.Subscribe(h =>
                {
                    h.Saga<NoddySaga>(new DapperExtensionsSagaRepository<NoddySaga>(dbProvider)).Transient();
                    //h.Saga<NoddySaga>(new InMemorySagaRepository<NoddySaga>()).Transient();
                    h.Handler<DriveLittleRedCarMessage>(msg => Console.WriteLine("Drive {0}", msg.Route)).Transient();
                });

            });

            Thread.Sleep(TimeSpan.FromSeconds(1));
            var guid = Guid.NewGuid();
            Bus.Instance.Publish(new NoddyMessageImpl { CorrelationId = guid, Text = "Lets go to the playground" });

            Thread.Sleep(TimeSpan.FromSeconds(1));
            Bus.Instance.Publish(new BigEarsMessageImpl { CorrelationId = guid, Text = "then lets go to the bakery" });

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            Bus.Shutdown();

        }

        private static void DropnCreateSagaTable(DbProvider dbProvider)
        {
            using (var db = dbProvider.Open())
            {
                // Drop 
                db.Execute(@"
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NoddySaga]') AND type in (N'U'))
DROP TABLE [dbo].[NoddySaga]");

                // Create
                db.Execute(@"
CREATE TABLE [dbo].[NoddySaga]
(
    [CorrelationId] [uniqueidentifier] NOT NULL,
    [NoddyText] [nvarchar](50) NULL,
    [BigEarText] [nvarchar](255) NULL,
PRIMARY KEY (CorrelationId)
)
create unique index NoddySaga_pk on NoddySaga (CorrelationId)");
            }
        }
    }

}

