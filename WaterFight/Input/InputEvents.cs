using System;
using System.Collections.Generic;
using System.Text;

using Utilla;

namespace WaterFight.Input
{
    public struct InputState
    {
        public bool held;
        public bool pressed;
        public bool released;
    }

    public class InputEventArgs : EventArgs
    {
        public bool Held { get;  private set; } = false;
        public bool Pressed { get; private set; } = false;
        public bool Released { get; private set; } = false;

        public InputEventArgs(in InputState inputs) : base()
        {
            Held = inputs.held;
            Pressed = inputs.pressed;
            Released = inputs.released;
        }
    }

    public class ControllerInputEvents
    {
        public event EventHandler<InputEventArgs> Grip;
        public event EventHandler<InputEventArgs> Trigger;

        public void InvokeTriggerEvent(InputEventArgs inputs) => Trigger?.SafeInvoke(this, inputs);
        public void InvokeGripEvent(InputEventArgs inputs) => Grip?.SafeInvoke(this, inputs);
    }

    internal class InputEvents
    {
        public static ControllerInputEvents RightController = new ControllerInputEvents();
        public static ControllerInputEvents LeftController = new ControllerInputEvents();
    }
}
