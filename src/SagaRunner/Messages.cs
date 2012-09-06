// Copyright 2012 Zarion Ltd.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MassTransit;

namespace SagaRunner
{
    public interface NoddyMessage
        :CorrelatedBy<Guid>
    {
        string Text { get; }
    }
    public interface BigEarsMessage
        : CorrelatedBy<Guid>
    {
        string Text { get; }
    }
    public interface DriveLittleRedCarMessage
        : CorrelatedBy<Guid>
    {
        string Route { get; }
    }

    public class NoddyMessageImpl
        :NoddyMessage
    {
        public Guid CorrelationId { get; set; }
        public string Text { get; set; }
    }
    public class BigEarsMessageImpl
        :BigEarsMessage
    {
        public Guid CorrelationId { get; set; }
        public string Text { get; set; }
    }
    public class DriveLittleRedCarMessageImpl
        :DriveLittleRedCarMessage
    {
        public Guid CorrelationId { get; set; }
        public string Route { get; set; }
    }
}
