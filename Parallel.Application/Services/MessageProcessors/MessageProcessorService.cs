using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        private void LocationCreateLocationReady(MessageType4[] messages)
        {
            if (messages.Length > 1)
            {
                var calculator = _locationCalculatorRouter.GetCalculator(typeof(MessageType4));
                if (calculator.MinAnchorCount <= messages.Length)
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

                    if (messages[0].MobilNodeId == 35002)
                    {
                        ICoordinate coordinate = calculator.GetResult(messages.First().MobilNodeId, distances);
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
                        var currentAnchors = calculator.CurrentAnchors;
                        foreach (var distance in distances)
                        {
                            var currentAnchor = currentAnchors.FirstOrDefault(x => x.Id == distance.FromAnchorId);
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
                  
                    
                }
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