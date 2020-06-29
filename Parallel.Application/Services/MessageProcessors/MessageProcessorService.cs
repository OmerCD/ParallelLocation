using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Parallel.Application.Entities.Database.Mongo;
using Parallel.Application.Hubs;
using Parallel.Application.Services.Interfaces;
using Parallel.Application.ValueObjects;
using Parallel.Location;
using Parallel.Repository;
using Parallel.Shared.DataTransferObjects;
using Parallel.Shared.DataTransferObjects.Client;

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
        private readonly IHubContext<LocationHub> _locationHubContext;
        private readonly AppSettings _appSettings;
        private readonly ILocationCalculator _type4Calculator;
        private static readonly ManualResetEvent ResetEvent = new ManualResetEvent(true);

        public LocationCreatorService(
            ILocationCalculatorRouter<Type> locationCalculatorRouter, IDatabaseContext databaseContext,
            ILogger<LocationCreatorService> logger, IHubContext<LocationHub> locationHubContext,
            AppSettings appSettings)
        {
            _locationRepository = databaseContext.GetSet<LocationRecord>();
            _locationCalculatorRouter = locationCalculatorRouter;
            _databaseContext = databaseContext;
            _logger = logger;
            _locationHubContext = locationHubContext;
            _appSettings = appSettings;
            _locationCreate = new LocationCreateTimeLimiter<MessageType4, ushort, ushort>(
                x => x.ReaderNodeId,
                x => x.MobilNodeId)
            {
                LocationReady = LocationCreateLocationReady
            };
            _type4Calculator = _locationCalculatorRouter.GetCalculator(typeof(MessageType4));
        }

        private void LocationCreateLocationReady(MessageType4[] messages)
        {
            if (messages.Length > 1)
            {
                if (_type4Calculator.MinAnchorCount <= messages.Length)
                {
                    var distances = new DistanceBase[messages.Length];
                    var messageRecords = new MessageRecord[messages.Length];
                    for (int i = 0; i < messages.Length; i++)
                    {
                        distances[i] = (new DistanceBase(messages[i].ReaderNodeId, messages[i].ACCZ));
                        messageRecords[i] = new MessageRecord
                        {
                            Data = null,
                            Name = "Power 8",
                            DataObject = messages[i]
                        };
                    }

                    ResetEvent.WaitOne();
                    ResetEvent.Reset();
                    try
                    {
                        ICoordinate coordinate = _type4Calculator.GetResult(messages.First().MobilNodeId, distances);
                        if (coordinate == null)
                        {
                            return;
                        }

                        var locationEntity = new LocationRecord
                        {
                            X = coordinate.X,
                            Y = coordinate.Y,
                            Z = coordinate.Z,
                            MessageRecords = messageRecords,
                            TagId = messages[0].MobilNodeId
                        };
                        // _locationRepository.Add(locationEntity);
                        // _databaseContext.SaveChanges();
                        var anchors = new List<AnchorInfo>();
                        foreach (var distance in distances)
                        {
                            var currentAnchor = _type4Calculator[distance.FromAnchorId];
                            anchors.Add(new AnchorInfo
                            {
                                Radius = distance.Distance,
                                X = currentAnchor.X,
                                Y = currentAnchor.Y,
                                Z = currentAnchor.Z,
                                Name = currentAnchor.Id.ToString()
                            });
                        }

                        _locationHubContext.Clients.All.SendCoreAsync(_appSettings.SignalRHub,
                            new object[]
                            {
                                new LocationInfo(locationEntity.TagId, locationEntity.X, locationEntity.Z,
                                    locationEntity.Y, anchors)
                            });
                    }
                    finally
                    {
                        ResetEvent.Set();
                    }
                }
            }
        }

        public bool HandleConditionSatisfied(object message)
        {
            return message is MessageType4 messageType4 && messageType4.Power == 8;
        }

        public void Handle(object message)
        {
            if (message is MessageType4 messageType4)
            {
                var anchor = _type4Calculator[messageType4.ReaderNodeId];

                if (anchor.MaxReadDistance > 0 && anchor.MaxReadDistance < messageType4.ACCZ)
                {
                    return;
                }

                _locationCreate.AddLocationMessage(messageType4);
            }
        }
    }
}