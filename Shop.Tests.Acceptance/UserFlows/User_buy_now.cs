﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GridDomain.Common;
using GridDomain.CQRS.Messaging;
using GridDomain.Tests.Framework;
using NUnit.Framework;

namespace Shop.Tests.Acceptance.UserFlows
{
    [TestFixture]
    public class User_buy_now : ExtendedNodeCommandTest
    {
        protected override TimeSpan Timeout { get; }
        protected override IContainerConfiguration CreateConfiguration()
        {
            throw new NotImplementedException();
        }

        protected override IMessageRouteMap CreateMap()
        {
            throw new NotImplementedException();
        }
    }
}