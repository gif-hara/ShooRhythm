using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TinyStateMachine
{
    /// <summary>
    /// Represents a tiny state machine that allows changing states asynchronously.
    /// </summary>
    public sealed class TinyStateMachine : IDisposable
    {
        private CancellationTokenSource scope = null;

        private readonly Dictionary<Type, ITinyStateMachineDecorator> decorates = new();

        private bool isDisposed = false;

        /// <summary>
        /// Event that is triggered when the state changes.
        /// </summary>
        public event Action<Func<CancellationToken, UniTask>> OnChangeState;

        ~TinyStateMachine()
        {
            Dispose();
        }

        /// <summary>
        /// Changes the state asynchronously.
        /// </summary>
        /// <param name="state">The state to change to.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the state change.</param>
        /// <returns>A <see cref="UniTask"/> representing the asynchronous operation.</returns>
        public async UniTask ChangeAsync(Func<CancellationToken, UniTask> state, CancellationToken cancellationToken = default)
        {
            if (isDisposed)
            {
                return;
            }

            Clear();
            scope = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Wait for one frame to ensure that the previous state has completed before starting the next state.
            await UniTask.NextFrame(scope.Token);
            OnChangeState?.Invoke(state);
            await state(scope.Token);
        }

        /// <summary>
        /// Changes the state synchronously.
        /// </summary>
        /// <param name="state">The state to change to.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the state change.</param>
        public void Change(Func<CancellationToken, UniTask> state, CancellationToken cancellationToken = default)
        {
            ChangeAsync(state, cancellationToken).Forget();
        }

        /// <summary>
        /// Decorates the state machine with a decorator.
        /// </summary>
        /// <typeparam name="TDecorator">The type of the decorator.</typeparam>
        /// <param name="decorator">The decorator instance.</param>
        public void Decorate<TDecorator>(TDecorator decorator) where TDecorator : class, ITinyStateMachineDecorator
        {
            decorator.Decorate(this);
            decorates.Add(typeof(TDecorator), decorator);
        }

        /// <summary>
        /// Gets the decorator of the specified type.
        /// </summary>
        /// <typeparam name="TDecorator">The type of the decorator.</typeparam>
        /// <returns>The decorator instance if found; otherwise, null.</returns>
        public TDecorator GetDecorator<TDecorator>() where TDecorator : class, ITinyStateMachineDecorator
        {
            if (decorates.TryGetValue(typeof(TDecorator), out var decorate))
            {
                return decorate as TDecorator;
            }

            return null;
        }

        /// <summary>
        /// Disposes the state machine and cancels any ongoing state change.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            Clear();
            isDisposed = true;
        }

        /// <summary>
        /// Cancels any ongoing state change and clears the state machine.
        /// </summary>
        public void Clear()
        {
            scope?.Cancel();
            scope?.Dispose();
            scope = null;
        }
    }
}