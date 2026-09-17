using System.Reflection;
using MassTransit;

namespace AeroTech.Ordering.Persistence.Tests.Inbox
{
    public class ConsumeContextStub : DispatchProxy
    {
        private readonly Dictionary<string, object?> _values = new(StringComparer.Ordinal);

        public static ConsumeContext<T> Create<T>(T message, Guid? messageId, string inputPath) where T : class
        {
            var receiveContext = Create<ReceiveContext, ConsumeContextStub>();
            ((ConsumeContextStub)(object)receiveContext)._values[$"get_{nameof(ReceiveContext.InputAddress)}"] = new Uri($"loopback://localhost{inputPath}");

            var consumeContext = Create<ConsumeContext<T>, ConsumeContextStub>();
            var stub = (ConsumeContextStub)(object)consumeContext;
            stub._values[$"get_{nameof(ConsumeContext<T>.Message)}"] = message;
            stub._values[$"get_{nameof(MessageContext.MessageId)}"] = messageId;
            stub._values[$"get_{nameof(ConsumeContext.ReceiveContext)}"] = receiveContext;
            stub._values[$"get_{nameof(PipeContext.CancellationToken)}"] = CancellationToken.None;

            return consumeContext;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod is null)
                return null;

            if (_values.TryGetValue(targetMethod.Name, out var value))
                return value;

            return targetMethod.ReturnType.IsValueType && targetMethod.ReturnType != typeof(void)
                ? Activator.CreateInstance(targetMethod.ReturnType)
                : null;
        }
    }
}
