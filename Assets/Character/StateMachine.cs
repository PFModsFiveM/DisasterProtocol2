using UnityEngine;
using System;
using System.Collections.Generic;

public class StateMachine : MonoBehaviour
{
    private Dictionary<string, State> states = new Dictionary<string, State>();
    private State activeState;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void AddState(string name, State state)
    {
        state.animator = animator;
        state.stateMachine = this;
        states[name] = state;
    }

    public void SetState(string name)
    {
        if (!states.ContainsKey(name))
        {
            Debug.LogWarning($"State '{name}' doesn't exist");
            return;
        }

        activeState?.OnExit();
        activeState = states[name];
        activeState.OnEnter();
    }

    public void Update()
    {
        activeState?.Tick();
    }

    public void FixedUpdate()
    {
        activeState?.FixedTick();
    }

    public string GetCurrentStateName()
    {
        foreach (var kvp in states)
        {
            if (kvp.Value == activeState)
                return kvp.Key;
        }
        return "None";
    }

    public abstract class State
    {
        public Animator animator;
        public StateMachine stateMachine;

        public virtual void OnEnter() { }
        public virtual void Tick() { }
        public virtual void FixedTick() { }
        public virtual void OnExit() { }

        protected void SetAnimatorFloat(string name, float value, float dampTime = 0.1f)
        {
            animator.SetFloat(name, value, dampTime, Time.deltaTime);
        }

        protected void SetAnimatorBool(string name, bool value)
        {
            animator.SetBool(name, value);
        }

        protected void SetAnimatorTrigger(string name)
        {
            animator.SetTrigger(name);
        }

        protected void SetAnimatorInteger(string name, int value)
        {
            animator.SetInteger(name, value);
        }
    }
}
