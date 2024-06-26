﻿using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace APUSH_Game.Helpers
{
    /// <summary>
    /// A utility class for handling input states
    /// </summary>
    internal static class InputHelper
    {
        public static float DebugX => (float)MouseLocation.X / Globals.WindowSize.X;
        public static float DebugY => (float)MouseLocation.Y / Globals.WindowSize.Y;

        /// <summary>
        /// Gets the current state of the keyboard.
        /// </summary>
        public static KeyboardState KeyboardState { get; private set; }

        /// <summary>
        /// Gets the current state of the mouse.
        /// </summary>
        public static MouseState MouseState { get; private set; }

        /// <summary>
        /// Gets the current state of the touchpad
        /// </summary>
        public static TouchCollection Touches { get; private set; }



        /// <summary>
        /// Gets the state of the keyboard on the previous tick.
        /// </summary>
        public static KeyboardState PrevKeyboardState { get; private set; }

        /// <summary>
        /// Gets the state of the mouse on the previous tick.
        /// </summary>
        public static MouseState PrevMouseState { get; private set; }

        /// <summary>
        /// Gets the current state of the touchpad
        /// </summary>
        public static TouchCollection PrevTouches { get; private set; }



        /// <summary>
        /// Gets the state of the mouse on the previous tick.
        /// </summary>
        public static int DeltaScroll => MouseState.ScrollWheelValue - PrevMouseState.ScrollWheelValue;

        /// <summary>
        /// Gets the state of the mouse on the previous tick.
        /// </summary>
        public static Point MouseLocation => MouseState.Position;

        public static int LeftMouseDownTime { get; private set; }
        public static int RightMouseDownTime { get; private set; }

        public static Point LeftMouseDownLoc { get; private set; }

        /// <summary>
        /// Updates the input states by capturing the current keyboard and mouse states.
        /// This method should be called at the beginning of each frame
        /// </summary>
        public static void TickUpdate()
        {
            PrevKeyboardState = KeyboardState;
            PrevMouseState = MouseState;
            PrevTouches = Touches;

            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
            Touches = TouchPanel.GetState();

            if (RisingEdge(MouseButton.Left))
                LeftMouseDownLoc = MouseLocation;

            if (!GameRoot.Game.IsActive)
            {
                PrevKeyboardState = default;
                PrevMouseState = default;
                KeyboardState = default;
                MouseState = default;
            }
        }

        public static void LateUpdate()
        {
            LeftMouseDownTime = Down(MouseButton.Left) ? LeftMouseDownTime + 1 : 0;
            RightMouseDownTime = Down(MouseButton.Right) ? RightMouseDownTime + 1 : 0;
        }

        /// <summary>
        /// Checks if a specific key has a rising edge (transition from released to pressed) in the current frame.
        /// </summary>
        /// <param name="key">The key to check for a rising edge.</param>
        /// <returns>True if the key has a rising edge, otherwise false.</returns>
        public static bool RisingEdge(Keys key)
        {
            return KeyboardState.IsKeyDown(key) && !PrevKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if a specific key has a falling edge (transition from pressed to released) in the current frame.
        /// </summary>
        /// <param name="key">The key to check for a falling edge.</param>
        /// <returns>True if the key has a falling edge, otherwise false.</returns>
        public static bool FallingEdge(Keys key)
        {
            return !KeyboardState.IsKeyDown(key) && PrevKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if the left or right mouse button has a rising edge (transition from released to pressed) in the current frame.
        /// </summary>
        /// <param name="IsLeftButton">True for the left mouse button, false for the right mouse button.</param>
        /// <returns>True if the specified mouse button has a rising edge, otherwise false.</returns>
        public static bool RisingEdge(MouseButton Button)
        {
            if (Button == MouseButton.Left)
            {
                return MouseState.LeftButton != PrevMouseState.LeftButton && MouseState.LeftButton == ButtonState.Pressed;
            }
            return MouseState.RightButton != PrevMouseState.RightButton && MouseState.RightButton == ButtonState.Pressed;
        }

        public static bool LeftMouseRisingEdgeUnSticky(int threshold = 10)
        {
            return RisingEdge(MouseButton.Left) && LeftMouseDownTime < threshold;
        }

        public static bool LeftMouseFallingEdgeUnSticky(int threshold = 10)
        {
            return FallingEdge(MouseButton.Left) && LeftMouseDownTime < threshold;
        }

        /// <summary>
        /// Checks if the left or right mouse button has a falling edge (transition from pressed to released) in the current frame.
        /// </summary>
        /// <param name="IsLeftButton">True for the left mouse button, false for the right mouse button.</param>
        /// <returns>True if the specified mouse button has a falling edge, otherwise false.</returns>
        public static bool FallingEdge(MouseButton Button)
        {
            if (Button == MouseButton.Left)
            {
                return MouseState.LeftButton != PrevMouseState.LeftButton && MouseState.LeftButton == ButtonState.Released;
            }
            return MouseState.RightButton != PrevMouseState.RightButton && MouseState.RightButton == ButtonState.Released;
        }

        /// <summary>
        /// Checks if a specific key is currently being held down.
        /// </summary>
        /// <param name="key">The key to check for being held down.</param>
        /// <returns>True if the key is currently held down, otherwise false.</returns>
        public static bool Down(Keys key)
        {
            return KeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if the left or right mouse button is currently being held down.
        /// </summary>
        /// <param name="IsLeftButton">True for the left mouse button, false for the right mouse button.</param>
        /// <returns>True if the specified mouse button is currently held down, otherwise false.</returns>
        public static bool Down(MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                return MouseState.LeftButton == ButtonState.Pressed;
            }
            return MouseState.RightButton == ButtonState.Pressed;
        }
    }

    /// <summary>
    /// Indicates the mouse button pressed
    /// </summary>
    internal enum MouseButton : byte
    {
        Left = 0, Right = 1, None = 2,
    }
}
