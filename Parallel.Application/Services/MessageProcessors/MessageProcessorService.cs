using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Parallel.Application.Entities.Database.Mongo;
using Parallel.Application.Services.Interfaces;
using Parallel.Location;
using Parallel.Repository;
using Parallel.Shared.DataTransferObjects;

#nullable enable
namespace Parallel.Application.Services.MessageProcessors
{
    public class LocationCreatorService : IMessageProcessor<LocationCreatorService>
    {
        private readonly LocationCreateTimeLimiter<MessageType4, ushort, ushort> _locationCreate;
        private readonly IRepository<LocationRecord> _locationRepository;
        private readonly ILocationCalculatorRouter<Type> _locationCalculatorRouter;
        private readonly IDatabaseContext _databaseContext;
        private readonly ILogger<LocationCreatorService> _logger;


        public LocationCreatorService(
            ILocationCalculatorRouter<Type> locationCalculatorRouter, IDatabaseContext databaseContext,
            ILogger<LocationCreatorService> logger)
        {
            _locationRepository = databaseContext.GetSet<LocationRecord>();
            _locationCalculatorRouter = locationCalculatorRouter;
            _databaseContext = databaseContext;
            _logger = logger;
            _locationCreate = new LocationCreateTimeLimiter<MessageType4, ushort, ushort>(
                x => x.ReaderNodeId,
                x => x.MobilNodeId)
            {
                LocationReady = LocationCreateLocationReady
            };
        }

        private void LocationCreateLocationReady(MessageType4[] messages)
        {
            if (messages.Length > 1)
            {
                var calculator = _locationCalculatorRouter.GetCalculator(typeof(MessageType4));
                var distances = new DistanceBase[messages.Length];
                var messageRecords = new MessageRecord[messages.Length];
                for (int i = 0; i < messages.Length; i++)
                {
                    distances[i] = (new DistanceBase(messages[i].ReaderNodeId, messages[i].ACCZ * 0.01));
                    messageRecords[i] = new MessageRecord
                    {
                        Data = null,
                        Name = "Power 8",
                        DataObject = messages[i]
                    };
                }

                ICoordinate coordinate = calculator.GetResult(distances);
                var locationEntity = new LocationRecord
                {
                    X = coordinate.X,
                    Y = coordinate.Y,
                    Z = coordinate.Z,
                    MessageRecords = messageRecords,
                    TagId = messages[0].MobilNodeId
                };
                _locationRepository.Add(locationEntity);
                _databaseContext.SaveChanges();
            }
        }

        public bool HandleConditionSatisfied(object message)
        {
            return message is MessageType4 messageType4 && messageType4.Power == 8;
        }

        public void Handle(object message)
        {
            _locationCreate.AddLocationMessage((MessageType4) message);
        }
    }
}