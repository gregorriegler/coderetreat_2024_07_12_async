using NUnit.Framework.Constraints;

namespace coderetreat_2024_07_12_async;

public class Tests
{
    public async Task<string> ClockIn(Func<string, Task<string>> serverCall, string userName)
    {
        var result = await serverCall.Invoke(userName);
        switch (result)
        {
            case "Success": return "Success";
            case "User does not exist": return "Unknown User: " + userName;
            default: return "Error";
        }
    }
    
    public async Task<string> ClockInWithGps(Func<Task<int>> gpsCall, Func<string, int, Task<string>> clockInCall, string userName)
    {
        var gpsCoordinate= await gpsCall.Invoke();
        // hier würde noch ein if kommen: if GPS fails
        var result = await clockInCall.Invoke(userName, gpsCoordinate);
        switch (result)
        {
            case "Success": return "Success with GPS";
            case "User does not exist": return "Unknown User: " + userName;
            default: return "Error";
        }
    }

    // --- test
    
    [Test]
    public async Task ClockInSuccessful()
    {
        var serverCall = GivenServerWithUserCharles();
        var result = await ClockIn(serverCall, "Charles");
        Assert.That(result, Is.EqualTo("Success"));
    }
    
    [Test]
    public async Task ClockInFailsBecauseOfTechnicalFailure()
    {
        var serverCall = GivenServerReturnsTechnicalFailure();
        var result = await ClockIn(serverCall, "User");
        Assert.That(result, Is.EqualTo("Error"));
    }
    
    [Test]
    public async Task ClockInFailsBecauseUserDoesNotExist()
    {
        var serverCall = GivenServerReturnsUserDoesNotExist();
        var result = await ClockIn(serverCall, "Rosie");
        Assert.That(result, Is.EqualTo("Unknown User: Rosie"));
    }
    
    // GPS Required
    
    [Test]
    public async Task ClockInSendsGps()
    {
        var gpsCall = GivenGpsSuccess(7);
        var serverCall = GivenServerWithUserCharlesAndGps(7);
        var result = await ClockInWithGps(gpsCall, serverCall, "Charles");
        Assert.That(result, Is.EqualTo("Success with GPS"));
    }
    
    [Test]
    public async Task ClockInSendsGpsWithUnknownUser()
    {
        var gpsCall = GivenGpsSuccess(7);
        var serverCall = GivenServerWithUserCharlesAndGps(7);
        var result = await ClockInWithGps(gpsCall, serverCall, "Rosie");
        Assert.That(result, Is.EqualTo("Unknown User: Rosie"));
    }

    private Func<string, int, Task<string>> GivenServerWithUserCharlesAndGps(int givenCoordinate)
    {
        return async (userName, coordinate) => userName == "Charles" && coordinate == givenCoordinate ? "Success" : "User does not exist";
    }

    private Func<Task<int>> GivenGpsSuccess(int givenCoordinate)
    {
        return async () => givenCoordinate;
    }

    private Func<string,Task<string>> GivenServerReturnsUserDoesNotExist() => async _ => "User does not exist";

    private Func<string,Task<string>> GivenServerReturnsTechnicalFailure()
    {
        return async _ => "Technical Failure";
    }

    private Func<string, Task<string>> GivenServerWithUserCharles()
    {
        return async userName => userName == "Charles" ? "Success" : "Unknown";
    }
}