using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR;

using Utilla;

namespace WaterFight.Input
{
    internal class ControllerInputs
    {
        private XRNode controllerNode;
        private InputDevice controller;
        private InputState grip;
        private InputState trigger;

        private ControllerInputEvents events = null;

        public ControllerInputs(XRNode cNode, ControllerInputEvents eventTarget)
        {
            this.events = eventTarget;
            this.controllerNode = cNode;
            this.controller = InputDevices.GetDeviceAtXRNode(controllerNode);
        }

        public void UpdateAndSendEvent()
        {
            if (!controller.isValid) {
                controller = InputDevices.GetDeviceAtXRNode(controllerNode);
            }

            if (ProcessInputs(ref grip, CommonUsages.gripButton)) {
                events?.InvokeGripEvent(new InputEventArgs(grip));
            }

            if (ProcessInputsDynamic(ref trigger, CommonUsages.triggerButton, CommonUsages.trigger)) {
                // Debug.Log("invoking trigger event");
                events?.InvokeTriggerEvent(new InputEventArgs(trigger));
            }
        }


        private bool ProcessInputs(ref InputState buttonState, in InputFeatureUsage<bool> button)
        {
            if (controller.TryGetFeatureValue(button, out bool pressed)) {
                return UpdatebuttonState(ref buttonState, pressed);

            } else {
                return UpdatebuttonState(ref buttonState, false);
            }
        }

        private bool ProcessInputsDynamic(ref InputState buttonState, in InputFeatureUsage<bool> button, in InputFeatureUsage<float> buttonValue)
        {
            if (controller.TryGetFeatureValue(button, out bool pressed) && pressed && controller.TryGetFeatureValue(buttonValue, out float position) && position > 0.8f) {
                return UpdatebuttonState(ref buttonState, pressed);

            } else {
                return UpdatebuttonState(ref buttonState, false);
            }
        }

        private bool UpdatebuttonState(ref InputState buttonState, in bool state)
        {
            bool stateChange = false;

            if (state) {
                if (!buttonState.pressed && !buttonState.held) {
                    buttonState.pressed = true;
                    buttonState.held = false;
                    buttonState.released = false;

                    stateChange = true;

                } else if (buttonState.pressed && !buttonState.held) {
                    buttonState.pressed = false;
                    buttonState.held = true;
                    buttonState.released = false;


                    /* currently no use for this to fire each frame while held, might in the future
                    stateChange = true;
                    
                } else if (!buttonState.pressed && buttonState.held) {
                    stateChange = true;
                    */
                }

            } else {
                if (buttonState.pressed || buttonState.held) {
                    buttonState.pressed = false;
                    buttonState.held = false;
                    buttonState.released = true;

                    stateChange = true;

                } else if (buttonState.released == true) {
                    buttonState.pressed = false;
                    buttonState.held = false;
                    buttonState.released = false;
                }
            }

            return stateChange;
        }
    }

    internal class InputHandler : MonoBehaviour
    {
        protected ControllerInputs rightController;
        protected ControllerInputs leftController;

        void Awake()
        {
            rightController = new ControllerInputs(XRNode.RightHand, InputEvents.RightController);
            leftController = new ControllerInputs(XRNode.LeftHand, InputEvents.LeftController);
        }

        void Update()
        {
            rightController?.UpdateAndSendEvent();
            leftController?.UpdateAndSendEvent();
        }
    }
}
