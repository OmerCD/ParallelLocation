using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Parallel.Application.Services.Interfaces;
using Parallel.Shared.DataTransferObjects;

namespace Parallel.Application.Services.MessageProcessors
{
    public class LocationCreateTimeLimiter<TMessage, TReaderId, TTagId> : ILocationCreateLimiter<TMessage> where TMessage: IDataCountMessage
    {
        private readonly Func<TMessage, TReaderId> _getReaderId;
        private readonly Func<TMessage, TTagId> _getTagId;
        private readonly IDictionary<TTagId, TagTimeController> _tagDictionary;

        public LocationCreateTimeLimiter(Func<TMessage, TReaderId> readerId, Func<TMessage, TTagId> tagId)
        {
            _getReaderId = readerId;
            _getTagId = tagId;
            _tagDictionary = new ConcurrentDictionary<TTagId, TagTimeController>();
        }

        public Action<TMessage[]> LocationReady { get; set; }
        public void AddLocationMessage(TMessage message)
        {
            TReaderId readerId = _getReaderId(message);
            
            TTagId tagId = _getTagId(message);
            
            if (!_tagDictionary.ContainsKey(tagId))
            {
                var tagTimeController = new TagTimeController(2000, tagId);
                _tagDictionary.Add(tagId, tagTimeController);
                tagTimeController.OnTagLocationReady+=TagTimeControllerOnTagLocationReady;
            }

            if (!_tagDictionary[tagId].ContainsKey(readerId))
            {
                _tagDictionary[tagId].TryAdd(readerId, new List<TMessage>());
            }

            _tagDictionary[tagId][readerId].Add(message);
        }

        private void TagTimeControllerOnTagLocationReady(TTagId tagId, IDictionary<TReaderId, ICollection<TMessage>> values)
        {
            var tempValues = new Dictionary<TReaderId, ICollection<TMessage>>(values);
            // LocationReady?.Invoke(values.Select(x=>x.Value.LastOrDefault()).ToArray());
            LocationReady?.Invoke(tempValues.Select(x=>x.Value.OrderByDescending(y=>y.DataCountNo).FirstOrDefault()).ToArray());
        }

        private class TagTimeController :ConcurrentDictionary<TReaderId, ICollection<TMessage>>, IDisposable
        {
            private readonly Timer _timer;
            private readonly TTagId _tagId;
            public event Action<TTagId, IDictionary<TReaderId, ICollection<TMessage>>> OnTagLocationReady;
            public TagTimeController(int timeLimit, TTagId tagId)
            {
                _tagId = tagId;
                _timer = new Timer(Callback, tagId, timeLimit, timeLimit);
            }
            
            private void Callback(object? state)
            {
                if (Values.Any())
                {
                    if (state is TTagId id)
                    {
                        OnTagLocationReady?.Invoke(id, this);
                    }
                    else
                    {
                        OnTagLocationReady?.Invoke(_tagId, this);
                    }
                    Clear();
                }
            }

            public void Dispose()
            {
                _timer.Dispose();
                Clear();
            }
        }
    }
}