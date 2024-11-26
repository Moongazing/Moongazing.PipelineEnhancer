namespace Doing.Retail.Core.Application.Pipelines.Authorization;

public interface ISecuredRequest
{
    public string[] Roles { get; }
}