using Lab1.DiningPhilosophers;

namespace PhilosopherService.Services;

public class PhilosopherStateService
{
    private PhilosopherState _state = PhilosopherState.Thinking;
    private bool _leftForkAcquired = false;
    private bool _rightForkAcquired = false;
    private DateTime _stateStartTime = DateTime.Now;
    private int _millisecondsRemainingInState = 0;
    private DateTime _hungryStartTime;
    private long _totalWaitingTimeMs = 0;
    private int _mealsEaten = 0;
    private ActionType _lastAction = ActionType.None;

    public PhilosopherState State => _state;
    public ActionType LastAction => _lastAction;
    public int MillisecondsRemainingInState => _millisecondsRemainingInState;
    public int MealsEaten => _mealsEaten;
    public long TotalWaitingTimeMs => _totalWaitingTimeMs;
    public bool LeftForkAcquired => _leftForkAcquired;
    public bool RightForkAcquired => _rightForkAcquired;

    public void SetLeftForkAcquired(bool acquired)
    {
        _leftForkAcquired = acquired;
        if (acquired)
            _lastAction = ActionType.TakeLeftFork;
    }

    public void SetRightForkAcquired(bool acquired)
    {
        _rightForkAcquired = acquired;
        if (acquired)
            _lastAction = ActionType.TakeRightFork;
    }

    public void ReleaseForks()
    {
        _leftForkAcquired = false;
        _rightForkAcquired = false;
        _lastAction = ActionType.ReleaseLeftFork;
    }

    public void ChangeState(PhilosopherState newState, int durationMs)
    {
        var oldState = _state;
        
        if (_state == PhilosopherState.Hungry && newState == PhilosopherState.Eating)
        {
            _totalWaitingTimeMs += (long)(DateTime.Now - _hungryStartTime).TotalMilliseconds;
        }

        if (oldState == PhilosopherState.Eating && newState == PhilosopherState.Thinking)
        {
            _mealsEaten++;
        }

        _state = newState;
        _stateStartTime = DateTime.Now;
        _millisecondsRemainingInState = durationMs;

        if (newState == PhilosopherState.Hungry)
        {
            _hungryStartTime = DateTime.Now;
        }
    }

    public void RealiseThePassageOfTime()
    {
        var elapsed = (DateTime.Now - _stateStartTime).TotalMilliseconds;
        _millisecondsRemainingInState = Math.Max(0, _millisecondsRemainingInState - (int)elapsed);
        _stateStartTime = DateTime.Now;
    }

    public bool IsBusy() => _millisecondsRemainingInState > 0;
}

