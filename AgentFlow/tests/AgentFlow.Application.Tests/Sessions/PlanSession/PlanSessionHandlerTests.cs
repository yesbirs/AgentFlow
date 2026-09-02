using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Planning;
using AgentFlow.Application.Sessions.PlanSession;
using AgentFlow.Domain.Entities;
using Moq;

namespace AgentFlow.Application.Tests.Sessions.PlanSession;

public class PlanSessionHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new Mock<IUnitOfWork>();

    [Fact]
    public async Task Throws_when_command_is_null()
    {
        var sessionRepository = new Mock<ISessionRepository>();
        var planner = new Mock<IPlanner>();

        var validator = new PlanValidator();

        var taskRepository = new Mock<IAgentTaskRepository>();
        var dependencyRepository = new Mock<ITaskDependencyRepository>();

        var planApplicationService = new PlanApplicationService(
            taskRepository.Object,
            dependencyRepository.Object,
            _unitOfWork.Object);

        var handler = new PlanSessionHandler(
            sessionRepository.Object,
            planner.Object,
            validator,
            planApplicationService);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.Handle(null!));
    }

    [Fact]
    public async Task Throws_when_session_does_not_exist()
    {
        var sessionRepository = new Mock<ISessionRepository>();
        var planner = new Mock<IPlanner>();

        sessionRepository
            .Setup(x => x.GetSessionByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        var validator = new PlanValidator();

        var taskRepository = new Mock<IAgentTaskRepository>();
        var dependencyRepository = new Mock<ITaskDependencyRepository>();

        var planApplicationService = new PlanApplicationService(
            taskRepository.Object,
            dependencyRepository.Object,
            _unitOfWork.Object);

        var handler = new PlanSessionHandler(
            sessionRepository.Object,
            planner.Object,
            validator,
            planApplicationService);

        var command = new PlanSessionCommand(Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command));

        planner.Verify(
            x => x.CreatePlanAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Applies_plan_and_returns_created_tasks()
    {
        var session = new Session("Test prompt");

        var sessionRepository = new Mock<ISessionRepository>();

        sessionRepository
            .Setup(x => x.GetSessionByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var planner = new FakePlanner();

        var validator = new PlanValidator();

        var taskRepository = new Mock<IAgentTaskRepository>();
        var dependencyRepository = new Mock<ITaskDependencyRepository>();

        var planApplicationService = new PlanApplicationService(
            taskRepository.Object,
            dependencyRepository.Object,
            _unitOfWork.Object);

        var handler = new PlanSessionHandler(
            sessionRepository.Object,
            planner,
            validator,
            planApplicationService);

        var command = new PlanSessionCommand(Guid.NewGuid());

        var result = await handler.Handle(command);

        // FakePlanner creates 4 tasks and 3 dependencies
        Assert.Equal(4, result.Count);

        var research = result.Single(t => t.Title == "Research requirements");
        Assert.Equal(session.Id, research.SessionId);

        // Ensure repository methods were invoked for each planned item
        taskRepository.Verify(
            x => x.AddAsync(
                It.IsAny<AgentTask>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(4));

        dependencyRepository.Verify(
            x => x.AddAsync(
                It.IsAny<TaskDependency>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }
}
