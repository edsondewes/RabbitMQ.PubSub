using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.PubSub
{
    public class PipelineManager<TObj>
    {
        private int _index;
        private TObj _obj;
        private MessageContext _context;

        private readonly Func<Task> _next;
        private readonly Func<TObj, MessageContext, CancellationToken, Func<Task>, Task>[] _pipelines;
        private readonly Func<TObj, MessageContext, CancellationToken, Task> _service;
        private readonly CancellationToken _cancellationToken;

        public PipelineManager(
            IBackgroundConsumer<TObj> service,
            IEnumerable<IConsumerPipeline<TObj>> pipelines,
            CancellationToken cancellationToken
            )
        {
            _service = service.Consume;
            _pipelines = pipelines.Select(p => (Func<TObj, MessageContext, CancellationToken, Func<Task>, Task>)p.Handle).ToArray();
            _cancellationToken = cancellationToken;

            _next = () =>
            {
                if (_index == _pipelines.Length)
                {
                    return _service(_obj, _context, _cancellationToken);
                }

                return _pipelines[_index++](_obj, _context, _cancellationToken, _next);
            };
        }

        public Task Run(TObj obj, MessageContext context)
        {
            _index = 0;
            _obj = obj;
            _context = context;

            return _next();
        }
    }
}
