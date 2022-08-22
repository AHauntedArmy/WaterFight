using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
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
            if(!controller.isValid) {
                controller = InputDevices.GetDeviceAtXRNode(controllerNode);
            }

            if (ProcessInputs(ref grip, CommonUsages.gripButton)) {
                events?.InvokeGripEvent(new InputEventArgs(grip));
            }

            if (ProcessInputs(ref trigger, CommonUsages.triggerButton)) {
                // Debug.Log("invoking trigger event");
                events?.InvokeTriggerEvent(new InputEventArgs(trigger));
            }
        }


        private bool ProcessInputs(ref InputState buttonState, InputFeatureUsage<bool> button)
        {
            bool stateChange = false;
            if (controller.TryGetFeatureValue(button, out bool result)) {
                if (result) {
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

            } else {
                if (buttonState.pressed || buttonState.held) {
                    stateChange = true;
                }

                buttonState.pressed = false;
                buttonState.held = false;
                buttonState.released = true;
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
