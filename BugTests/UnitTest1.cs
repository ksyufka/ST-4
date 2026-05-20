using BugPro;

namespace BugTests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void NewBug_HasNewState()
    {
        var bug = CreateBug();

        Assert.AreEqual(BugState.New, bug.State);
    }

    [TestMethod]
    public void NewBug_IsNotFinished()
    {
        var bug = CreateBug();

        Assert.IsFalse(bug.IsFinished);
    }

    [TestMethod]
    public void NewBug_HistoryStartsWithNew()
    {
        var bug = CreateBug();

        CollectionAssert.AreEqual(new[] { BugState.New }, bug.History.ToArray());
    }

    [TestMethod]
    public void Constructor_EmptyTitle_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() => new Bug(" "));
    }

    [TestMethod]
    public void Open_FromNew_MovesToOpen()
    {
        var bug = CreateBug();

        bug.Open();

        Assert.AreEqual(BugState.Open, bug.State);
    }

    [TestMethod]
    public void Reject_FromNew_MovesToRejected()
    {
        var bug = CreateBug();

        bug.Reject();

        Assert.AreEqual(BugState.Rejected, bug.State);
        Assert.IsTrue(bug.IsFinished);
    }

    [TestMethod]
    public void Assign_FromOpen_SavesDeveloper()
    {
        var bug = OpenBug();

        bug.Assign("Anna");

        Assert.AreEqual("Anna", bug.AssignedTo);
        Assert.AreEqual(BugState.Open, bug.State);
    }

    [TestMethod]
    public void Assign_EmptyDeveloper_ThrowsArgumentException()
    {
        var bug = OpenBug();

        Assert.ThrowsException<ArgumentException>(() => bug.Assign(""));
    }

    [TestMethod]
    public void StartWork_FromOpen_MovesToInProgress()
    {
        var bug = OpenBug();

        bug.StartWork();

        Assert.AreEqual(BugState.InProgress, bug.State);
    }

    [TestMethod]
    public void RequestReview_FromInProgress_MovesToReview()
    {
        var bug = InProgressBug();

        bug.RequestReview();

        Assert.AreEqual(BugState.Review, bug.State);
    }

    [TestMethod]
    public void Resolve_FromInProgress_MovesToResolved()
    {
        var bug = InProgressBug();

        bug.Resolve("Fixed");

        Assert.AreEqual(BugState.Resolved, bug.State);
        Assert.AreEqual("Fixed", bug.Resolution);
    }

    [TestMethod]
    public void Resolve_FromReview_MovesToResolved()
    {
        var bug = ReviewBug();

        bug.Resolve("Approved fix");

        Assert.AreEqual(BugState.Resolved, bug.State);
    }

    [TestMethod]
    public void Resolve_EmptyResolution_ThrowsArgumentException()
    {
        var bug = InProgressBug();

        Assert.ThrowsException<ArgumentException>(() => bug.Resolve(" "));
    }

    [TestMethod]
    public void Verify_FromResolved_MovesToVerified()
    {
        var bug = ResolvedBug();

        bug.Verify();

        Assert.AreEqual(BugState.Verified, bug.State);
    }

    [TestMethod]
    public void Close_FromVerified_MovesToClosed()
    {
        var bug = VerifiedBug();

        bug.Close();

        Assert.AreEqual(BugState.Closed, bug.State);
        Assert.IsTrue(bug.IsFinished);
    }

    [TestMethod]
    public void Reopen_FromResolved_MovesToReopened()
    {
        var bug = ResolvedBug();

        bug.Reopen();

        Assert.AreEqual(BugState.Reopened, bug.State);
        Assert.AreEqual(1, bug.ReopenCount);
    }

    [TestMethod]
    public void Reopen_FromReview_MovesToReopened()
    {
        var bug = ReviewBug();

        bug.Reopen();

        Assert.AreEqual(BugState.Reopened, bug.State);
    }

    [TestMethod]
    public void ReopenedBug_CanBeAssignedAgain()
    {
        var bug = ReopenedBug();

        bug.Assign("Oleg");

        Assert.AreEqual("Oleg", bug.AssignedTo);
        Assert.AreEqual(BugState.Reopened, bug.State);
    }

    [TestMethod]
    public void ReopenedBug_CanReturnToWork()
    {
        var bug = ReopenedBug();

        bug.StartWork();

        Assert.AreEqual(BugState.InProgress, bug.State);
    }

    [TestMethod]
    public void ReopenedBug_CanBeRejected()
    {
        var bug = ReopenedBug();

        bug.Reject();

        Assert.AreEqual(BugState.Rejected, bug.State);
        Assert.IsTrue(bug.IsFinished);
    }

    [TestMethod]
    public void CanFire_ReturnsTrueForAllowedTrigger()
    {
        var bug = CreateBug();

        Assert.IsTrue(bug.CanFire(BugTrigger.Open));
    }

    [TestMethod]
    public void CanFire_ReturnsFalseForForbiddenTrigger()
    {
        var bug = CreateBug();

        Assert.IsFalse(bug.CanFire(BugTrigger.Close));
    }

    [TestMethod]
    public void History_ContainsAllTransitions()
    {
        var bug = VerifiedBug();

        CollectionAssert.AreEqual(
            new[]
            {
                BugState.New,
                BugState.Open,
                BugState.Open,
                BugState.InProgress,
                BugState.Resolved,
                BugState.Verified
            },
            bug.History.ToArray());
    }

    [TestMethod]
    public void Close_FromNew_ThrowsStatelessInvalidOperationException()
    {
        var bug = CreateBug();

        Assert.ThrowsException<InvalidOperationException>(() => bug.Close());
    }

    [TestMethod]
    public void Verify_FromOpen_ThrowsStatelessInvalidOperationException()
    {
        var bug = OpenBug();

        Assert.ThrowsException<InvalidOperationException>(() => bug.Verify());
    }

    [TestMethod]
    public void Open_FromClosed_ThrowsStatelessInvalidOperationException()
    {
        var bug = ClosedBug();

        Assert.ThrowsException<InvalidOperationException>(() => bug.Open());
    }

    [TestMethod]
    public void Reject_FromClosed_ThrowsStatelessInvalidOperationException()
    {
        var bug = ClosedBug();

        Assert.ThrowsException<InvalidOperationException>(() => bug.Reject());
    }

    private static Bug CreateBug()
    {
        return new Bug("Login form is broken");
    }

    private static Bug OpenBug()
    {
        var bug = CreateBug();
        bug.Open();
        return bug;
    }

    private static Bug InProgressBug()
    {
        var bug = OpenBug();
        bug.Assign("Anna");
        bug.StartWork();
        return bug;
    }

    private static Bug ReviewBug()
    {
        var bug = InProgressBug();
        bug.RequestReview();
        return bug;
    }

    private static Bug ResolvedBug()
    {
        var bug = InProgressBug();
        bug.Resolve("Fixed");
        return bug;
    }

    private static Bug VerifiedBug()
    {
        var bug = ResolvedBug();
        bug.Verify();
        return bug;
    }

    private static Bug ClosedBug()
    {
        var bug = VerifiedBug();
        bug.Close();
        return bug;
    }

    private static Bug ReopenedBug()
    {
        var bug = ResolvedBug();
        bug.Reopen();
        return bug;
    }
}