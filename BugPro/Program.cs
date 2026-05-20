using Stateless;

namespace BugPro;

public enum BugState
{
    New,
    Open,
    InProgress,
    Review,
    Resolved,
    Verified,
    Closed,
    Rejected,
    Reopened
}

public enum BugTrigger
{
    Open,
    Assign,
    StartWork,
    RequestReview,
    Resolve,
    Verify,
    Close,
    Reject,
    Reopen
}

public sealed class Bug
{
    private readonly StateMachine<BugState, BugTrigger> stateMachine;
    private readonly List<BugState> history;

    public Bug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Bug title is required.", nameof(title));
        }

        Title = title;
        history = new List<BugState> { BugState.New };
        stateMachine = new StateMachine<BugState, BugTrigger>(() => State, state => State = state);

        ConfigureWorkflow();
    }

    public string Title { get; }

    public BugState State { get; private set; } = BugState.New;

    public string? AssignedTo { get; private set; }

    public string? Resolution { get; private set; }

    public int ReopenCount { get; private set; }

    public IReadOnlyList<BugState> History => history;

    public bool IsFinished => State is BugState.Closed or BugState.Rejected;

    public bool CanFire(BugTrigger trigger) => stateMachine.CanFire(trigger);

    public void Open() => Fire(BugTrigger.Open);

    public void Assign(string developer)
    {
        if (string.IsNullOrWhiteSpace(developer))
        {
            throw new ArgumentException("Developer name is required.", nameof(developer));
        }

        Fire(BugTrigger.Assign);
        AssignedTo = developer;
    }

    public void StartWork() => Fire(BugTrigger.StartWork);

    public void RequestReview() => Fire(BugTrigger.RequestReview);

    public void Resolve(string resolution)
    {
        if (string.IsNullOrWhiteSpace(resolution))
        {
            throw new ArgumentException("Resolution is required.", nameof(resolution));
        }

        Fire(BugTrigger.Resolve);
        Resolution = resolution;
    }

    public void Verify() => Fire(BugTrigger.Verify);

    public void Close() => Fire(BugTrigger.Close);

    public void Reject() => Fire(BugTrigger.Reject);

    public void Reopen()
    {
        Fire(BugTrigger.Reopen);
        ReopenCount++;
    }

    private void ConfigureWorkflow()
    {
        stateMachine.OnTransitioned(transition => history.Add(transition.Destination));

        stateMachine.Configure(BugState.New)
            .Permit(BugTrigger.Open, BugState.Open)
            .Permit(BugTrigger.Reject, BugState.Rejected);

        stateMachine.Configure(BugState.Open)
            .PermitReentry(BugTrigger.Assign)
            .Permit(BugTrigger.StartWork, BugState.InProgress)
            .Permit(BugTrigger.Reject, BugState.Rejected);

        stateMachine.Configure(BugState.InProgress)
            .Permit(BugTrigger.RequestReview, BugState.Review)
            .Permit(BugTrigger.Resolve, BugState.Resolved);

        stateMachine.Configure(BugState.Review)
            .Permit(BugTrigger.Resolve, BugState.Resolved)
            .Permit(BugTrigger.Reopen, BugState.Reopened);

        stateMachine.Configure(BugState.Resolved)
            .Permit(BugTrigger.Verify, BugState.Verified)
            .Permit(BugTrigger.Reopen, BugState.Reopened);

        stateMachine.Configure(BugState.Verified)
            .Permit(BugTrigger.Close, BugState.Closed)
            .Permit(BugTrigger.Reopen, BugState.Reopened);

        stateMachine.Configure(BugState.Reopened)
            .PermitReentry(BugTrigger.Assign)
            .Permit(BugTrigger.StartWork, BugState.InProgress)
            .Permit(BugTrigger.Reject, BugState.Rejected);
    }

    private void Fire(BugTrigger trigger) => stateMachine.Fire(trigger);
}

public static class Program
{
    public static void Main()
    {
        var bug = new Bug("Application crashes on login");

        bug.Open();
        bug.Assign("Ivan");
        bug.StartWork();
        bug.RequestReview();
        bug.Resolve("Fixed null reference in login handler");
        bug.Verify();
        bug.Close();

        Console.WriteLine($"Bug: {bug.Title}");
        Console.WriteLine($"State: {bug.State}");
        Console.WriteLine($"Assigned to: {bug.AssignedTo}");
        Console.WriteLine($"Resolution: {bug.Resolution}");
        Console.WriteLine($"History: {string.Join(" -> ", bug.History)}");
    }
}
