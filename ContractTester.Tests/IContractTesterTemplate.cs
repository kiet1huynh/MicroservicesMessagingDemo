using System;

namespace ContractTester.Tests
{
    public interface IContractTesterTemplate
    {
        Guid ContractId { get; set; }
        IMessageDetails Message { get; set; }
    }

    public interface IMessageDetails
    {
        Guid? Id { get; set; }
        DateTime Timestamp { get; set; }
    }
}