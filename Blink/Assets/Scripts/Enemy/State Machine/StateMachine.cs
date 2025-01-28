using UnityEngine;

public class StateMachine
{
    public BaseState currentState {  get; set; }

    public void Initialize(BaseState startingState)
    {
        currentState = startingState;
        currentState.Enter();
    }

    public void ChangeState(BaseState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
}
