using System;

namespace Tug.Server.Providers
{
    public class IntegTestDscHandler : BasicDscHandler
    {
        public IntegTestDscHandler()
        {
            Console.WriteLine("CONSTRUCTING INTEG-TEST HANDLER");
        }

        public override void RegisterDscAgent(Guid agentId, Model.RegisterDscAgentRequestBody detail)
        {
            Console.WriteLine($"CALLING REGISTER: {detail.AgentInformation["foo"]}");
            base.RegisterDscAgent(agentId, detail);
        }
    }
}