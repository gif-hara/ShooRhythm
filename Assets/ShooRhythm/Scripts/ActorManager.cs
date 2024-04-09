using System.Collections.Generic;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ActorManager
    {
        private readonly Dictionary<Vector2Int, Actor> actors = new();

        public void AddActor(Vector2Int position, Actor actor)
        {
            actors[position] = actor;
        }

        public void RemoveActor(Vector2Int position)
        {
            actors.Remove(position);
        }

        public Actor TryGetActor(Vector2Int position)
        {
            return actors.TryGetValue(position, out var actor) ? actor : null;
        }
    }
}
