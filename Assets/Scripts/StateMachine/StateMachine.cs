using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    StateNode current;
    Dictionary<IState, StateNode> nodes = new();
    HashSet<ITransition> anyTransitions = new();

    public void Update()
    {
        var transition = GetTransition();
        if (transition != null)
            ChangeState(transition.To);

        current.State?.Update();
    }

    public void FixedUpdate()
    {
        current.State?.FixedUpdate();
    }

    public void SetState(IState state)
    {
        current = GetOrAddNode(state);
        current.State?.OnEnter();
    }

    public void Reset()
    {
        current.State?.OnExit();
        current = null;
    }

    void ChangeState(IState state)
    {
        Debug.Log($"Transitioning to state: {state.GetType().Name}");

        if (state == current.State) return;

        var previousState = current.State;
        var nextState = nodes[state].State;

        previousState?.OnExit();
        nextState?.OnEnter();
        current = nodes[state];
    }

    ITransition GetTransition()
    {
        foreach (var transition in anyTransitions)
            if (transition.Condition.Evaluate())
                return transition;

        foreach (var transition in current.Transitions)
            if (transition.Condition.Evaluate())
                return transition;

        return null;
    }

    public void AddTransition(IState from, IState to, IPredicate condition)
    {
        GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
    }

    public void AddAnyTransition(IState to, IPredicate condition)
    {
        anyTransitions.Add(new Transition(GetOrAddNode(to).State, condition));
    }

    StateNode GetOrAddNode(IState state)
    {
        var node = nodes.GetValueOrDefault(state);

        if (node == null)
        {
            node = new StateNode(state);
            nodes.Add(state, node);
        }

        return node;
    }

    class StateNode
    {
        public IState State { get; }
        public HashSet<ITransition> Transitions { get; }

        public StateNode(IState state)
        {
            State = state;
            Transitions = new HashSet<ITransition>();
        }

        public void AddTransition(IState to, IPredicate condition)
        {
            Transitions.Add(new Transition(to, condition));
        }
    }
}
