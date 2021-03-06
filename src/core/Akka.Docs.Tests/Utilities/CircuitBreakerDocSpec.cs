﻿//-----------------------------------------------------------------------
// <copyright file="CircuitBreakerDocSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Akka.Pattern;

namespace DocsExamples.Utilities.CircuitBreakers
{
	#region circuit-breaker-usage
    public class DangerousActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public DangerousActor()
        {
            var breaker = new CircuitBreaker(
                    maxFailures: 5,
                    callTimeout: TimeSpan.FromSeconds(10),
                    resetTimeout: TimeSpan.FromMinutes(1))
                .OnOpen(NotifyMeOnOpen);
        }

        private void NotifyMeOnOpen()
        {
            _log.Warning("My CircuitBreaker is now open, and will not close for one minute");
        }
    }
    #endregion

    #region call-protection
    public class DangerousActorCallProtection : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public DangerousActorCallProtection()
        {
            var breaker = new CircuitBreaker(
                    maxFailures: 5,
                    callTimeout: TimeSpan.FromSeconds(10),
                    resetTimeout: TimeSpan.FromMinutes(1))
                .OnOpen(NotifyMeOnOpen);

            var dangerousCall = "This really isn't that dangerous of a call after all";

            Receive<string>(str => str.Equals("is my middle name"), msg =>
            {
                breaker.WithCircuitBreaker(() => Task.FromResult(dangerousCall)).PipeTo(Sender);
            });

            Receive<string>(str => str.Equals("block for me"), msg =>
            {
                Sender.Tell(breaker.WithSyncCircuitBreaker(() => dangerousCall));
            });
        }

        private void NotifyMeOnOpen()
        {
            _log.Warning("My CircuitBreaker is now open, and will not close for one minute");
        }
    }
    #endregion
}
