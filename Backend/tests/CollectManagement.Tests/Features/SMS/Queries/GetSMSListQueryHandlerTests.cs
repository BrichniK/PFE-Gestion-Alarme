using CollectManagement.Application.Features.SMS.Queries.GetSMSList;
using CollectManagement.Application.Interfaces.Repositories.SMS;
using FluentAssertions;
using Moq;
using SMSEntity = CollectManagement.Domain.SMS.SMS;

namespace CollectManagement.Tests.Features.SMS.Queries;

public class GetSMSListQueryHandlerTests
{

    private readonly Mock<ISMSRepository> _repository;


    public GetSMSListQueryHandlerTests()
    {
        _repository = new Mock<ISMSRepository>();
    }


    [Fact]
    public async Task Handle_Should_Return_SMS_List()
    {

        _repository
            .Setup(x => x.GetPagedListAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<SMSEntity>().AsReadOnly() as IReadOnlyList<SMSEntity>, 0));

        var handler = new GetSMSListQueryHandler(_repository.Object);

        var query = new GetSMSListQuery(null, null, null, 1, 10);


        var result = await handler.Handle(query, CancellationToken.None);


        result.Should().NotBeNull();

        result.SMSs.Should().BeEmpty();

        result.Length.Should().Be(0);
    }
}
