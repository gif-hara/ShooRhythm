using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TinyStateMachine
{
    /// <summary>
    /// Represents a decorator for a TinyStateMachine.
    /// </summary>
    public interface ITinyStateMachineDecorator
    {
        /// <summary>
        /// Decorates the specified TinyStateMachine.
        /// </summary>
        /// <param name="stateMachine">The TinyStateMachine to decorate.</param>
        void Decorate(TinyStateMachine stateMachine);
    }

    /// <summary>
    /// Represents a decorator for a TinyStateMachine that uses an enum to define states.
    /// </summary>
    /// <typeparam name="TEnum">The enum type representing the states.</typeparam>
    public sealed class TinyStateMachineEnum<TEnum> : ITinyStateMachineDecorator
    {
        private readonly Dictionary<Func<CancellationToken, UniTask>, TEnum> map;

        /// <summary>
        /// Gets the current state of the TinyStateMachine.
        /// </summary>
        public TEnum CurrentState { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TinyStateMachineEnum class with the specified state map.
        /// </summary>
        /// <param name="map">The dictionary that maps states to functions.</param>
        public TinyStateMachineEnum(Dictionary<Func<CancellationToken, UniTask>, TEnum> map)
        {
            this.map = map;
        }

        /// <summary>
        /// Decorates the specified TinyStateMachine.
        /// </summary>
        /// <param name="stateMachine">The TinyStateMachine to decorate.</param>
        public void Decorate(TinyStateMachine stateMachine)
        {
            stateMachine.OnChangeState += ChangeState;
        }

        private void ChangeState(Func<CancellationToken, UniTask> state)
        {
            CurrentState = map[state];
        }
    }

    /// <summary>
    /// Represents a decorator for a TinyStateMachine that records the states.
    /// </summary>
    public sealed class TinyStateMachineRecorder : ITinyStateMachineDecorator
    {
        private readonly List<Func<CancellationToken, UniTask>> states = new();

        /// <summary>
        /// Gets the recorded states.
        /// </summary>
        public IReadOnlyList<Func<CancellationToken, UniTask>> States => states;

        /// <summary>
        /// Decorates the specified TinyStateMachine.
        /// </summary>
        /// <param name="stateMachine">The TinyStateMachine to decorate.</param>
        public void Decorate(TinyStateMachine stateMachine)
        {
            stateMachine.OnChangeState += RecordState;
        }

        private void RecordState(Func<CancellationToken, UniTask> state)
        {
            states.Add(state);
        }
    }
}