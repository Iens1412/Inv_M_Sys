using System;
using System.Collections.Generic;

namespace Inv_M_Sys.Helpers
{
    public class Messenger
    {
        private static readonly Messenger _default = new Messenger();
        public static Messenger Default => _default;

        private readonly Dictionary<Type, List<Delegate>> _recipients = new();

        /// <summary>
        /// Registers a listener for a specific message type.
        /// </summary>
        public void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            var messageType = typeof(TMessage);
            if (!_recipients.ContainsKey(messageType))
                _recipients[messageType] = new List<Delegate>();

            _recipients[messageType].Add(action);
        }

        /// <summary>
        /// Sends a message to all registered listeners of that message type.
        /// </summary>
        public void Send<TMessage>(TMessage message)
        {
            var messageType = typeof(TMessage);
            if (_recipients.ContainsKey(messageType))
            {
                foreach (var action in _recipients[messageType])
                {
                    if (action is Action<TMessage> typedAction)
                        typedAction(message);
                }
            }
        }

        /// <summary>
        /// Unregisters a listener for a specific message type.
        /// </summary>
        public void Unregister<TMessage>(object recipient, Action<TMessage> action)
        {
            var messageType = typeof(TMessage);
            if (_recipients.ContainsKey(messageType))
            {
                _recipients[messageType].Remove(action);
            }
        }
    }
}
