using UnityEngine;

namespace RGSSUnity
{
    using System;
    using UnityEngine.InputSystem;

    public class GameInputManager : MonoBehaviour
    {
        public void HandleA(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.A);

        public void HandleB(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.B);

        public void HandleC(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.C);

        public void HandleX(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.X);

        public void HandleY(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.Y);

        public void HandleZ(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.Z);

        public void HandleL(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.L);

        public void HandleR(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.R);

        public void HandleShift(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.SHIFT);

        public void HandleCtrl(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.CTRL);

        public void HandleAlt(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.ALT);

        public void HandleF5(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.F5);

        public void HandleF6(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.F6);

        public void HandleF7(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.F7);

        public void HandleF8(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.F8);

        public void HandleF9(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.F9);

        public void HandleUp(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.UP);

        public void HandleDown(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.DOWN);

        public void HandleLeft(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.LEFT);

        public void HandleRight(InputAction.CallbackContext callbackContext) => SetInputState(callbackContext, InputStateRecorder.InputKey.RIGHT);
        
        public void HandleLeftStick(InputAction.CallbackContext callbackContext)
        {
        }

        private static void SetInputState(InputAction.CallbackContext callbackContext, InputStateRecorder.InputKey key)
        {
            // check trigger/repeat/press of key A
            if (callbackContext.started)
            {
                InputStateRecorder.Instance.SetPress(key);
            }

            if (callbackContext.canceled)
            {
                InputStateRecorder.Instance.SetRelease(key);
            }
        }
    }
}