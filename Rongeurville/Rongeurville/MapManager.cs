﻿using MPI;
using Rongeurville.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rongeurville
{
    class MapManager
    {
        private Map map;
        private Intracommunicator comm;

        private ActorProcess[] rats;
        private ActorProcess[] cats;

        private bool ContinueExecution = true;

        class ActorProcess
        {
            public int Rank;
            public bool IsFinished;
            public Tile Position;
        }

        public MapManager(Intracommunicator comm, string mapFilePath, int numberOfRats, int numberOfCats)
        {
            this.comm = comm;

            if (!File.Exists(mapFilePath))
            {
                throw new FileNotFoundException("The map file was not found.");
            }

            map = Map.LoadMapFromFile(mapFilePath);

            if (map.Rats.Count != numberOfRats || map.Cats.Count != numberOfCats)
            {
                throw new Exception("Rat or cat counts does not match with map loaded.");
            }

            int count = 0;
            rats = map.Rats.Select(t => new ActorProcess
            {
                Rank = ++count,
                IsFinished = false,
                Position = t
            }).ToArray();

            cats = map.Cats.Select(t => new ActorProcess
            {
                Rank = ++count,
                IsFinished = false,
                Position = t
            }).ToArray();
        }

        public void Start()
        {
            // Send map to everyone and start the game
            StartSignal startSignal = new StartSignal { Map = map };
            comm.Broadcast(ref startSignal, 0);

            while (ContinueExecution)
            {
                // Receive next message and handle it
                Message message = comm.Receive<Message>(Communicator.anySource, 0);

                if (message is Communication.Request)
                {
                    if (message.GetType() == typeof(MoveRequest))
                    {
                        HandleMovePlayer((MoveRequest)message);
                    }
                    else if (message.GetType() == typeof(MeowRequest))
                    {
                        HandleMeow((MeowRequest)message);
                    }
                    else if (message.GetType() == typeof(DeathConfirmation))
                    {
                        HandleDeath((DeathConfirmation)message);
                        if (AreAllActorsFinished())
                        {
                            // Stop the MapManager
                            ContinueExecution = false;
                        }
                    }
                    else
                    {
                        // TODO This is an unsupported request, log it
                    }
                }
                else
                {
                    // TODO This is an invalid message, only request are accepted, log it
                }
            }
        }

        /// <summary>
        /// Handles a move request by a player and signal it to the other processes
        /// </summary>
        /// <param name="moveRequest"></param>
        private void HandleMovePlayer(MoveRequest moveRequest)
        {

        }

        /// <summary>
        /// Handles a meow and signal it to other processes
        /// </summary>
        /// <param name=""></param>
        private void HandleMeow(MeowRequest meowRequest)
        {

        }

        /// <summary>
        /// Handles a death
        /// </summary>
        /// <param name="deathConfirmation"></param>
        private void HandleDeath(DeathConfirmation deathConfirmation)
        {
            ActorProcess dyingActorProcess = GetActorProcessByRank(deathConfirmation.Rank);
            dyingActorProcess.IsFinished = true;
        }

        /// <summary>
        /// Check if all actors are finished
        /// </summary>
        /// <returns></returns>
        private bool AreAllActorsFinished()
        {
            return !(rats.Any(rat => !rat.IsFinished) || cats.Any(cat => !cat.IsFinished));
        }

        /// <summary>
        /// Get the ActorProcess linked to the rank
        /// </summary>
        /// <param name="rank"></param>
        /// <returns></returns>
        private ActorProcess GetActorProcessByRank(int rank)
        {
            return cats.First(c => c.Rank == rank) ?? rats.First(r => r.Rank == rank);
        }
    }
}
