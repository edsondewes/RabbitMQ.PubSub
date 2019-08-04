using System.Diagnostics;

namespace RabbitMQ.PubSub.Diagnostics
{
    internal static class MessageDiagnostics
    {
        private const string ListenerName = "RabbitMQ.PubSub";
        private const string MessageInEvent = ListenerName + ".MessageIn";
        private const string MessageOutEvent = ListenerName + ".MessageOut";
        private const string MessageDeserializeEvent = ListenerName + ".Deserialize";
        private const string MessageSerializeEvent = ListenerName + ".Serialize";

        private static readonly DiagnosticListener _diagnostic = new DiagnosticListener(ListenerName);

        public static Activity StartMessageDeserialize()
        {
            Activity activity = null;

            if (_diagnostic.IsEnabled() && _diagnostic.IsEnabled(MessageDeserializeEvent))
            {
                activity = new Activity(MessageDeserializeEvent);
                _diagnostic.StartActivity(activity, null);
            }

            return activity;
        }

        public static Activity StartMessageIn(MessageContext context)
        {
            Activity activity = null;

            if (_diagnostic.IsEnabled() && _diagnostic.IsEnabled(MessageInEvent))
            {
                activity = new Activity(MessageInEvent);
                _diagnostic.StartActivity(activity, new { Context = context });
            }

            return activity;
        }

        public static Activity StartMessageOut(PublishOptions options)
        {
            Activity activity = null;

            if (_diagnostic.IsEnabled() && _diagnostic.IsEnabled(MessageOutEvent))
            {
                activity = new Activity(MessageOutEvent);
                _diagnostic.StartActivity(activity, new { Options = options });
            }

            return activity;
        }

        public static Activity StartMessageSerialize()
        {
            Activity activity = null;

            if (_diagnostic.IsEnabled() && _diagnostic.IsEnabled(MessageSerializeEvent))
            {
                activity = new Activity(MessageSerializeEvent);
                _diagnostic.StartActivity(activity, null);
            }

            return activity;
        }

        public static void StopMessageDeserialize(Activity activity)
        {
            if (activity != null)
            {
                _diagnostic.StopActivity(activity, null);
            }
        }

        public static void StopMessageIn(Activity activity, MessageContext context)
        {
            if (activity != null)
            {
                _diagnostic.StopActivity(activity, new { Context = context });
            }
        }

        public static void StopMessageOut(Activity activity, PublishOptions options)
        {
            if (activity != null)
            {
                _diagnostic.StopActivity(activity, new { Options = options });
            }
        }

        public static void StopMessageSerialize(Activity activity)
        {
            if (activity != null)
            {
                _diagnostic.StopActivity(activity, null);
            }
        }       
    }
}
