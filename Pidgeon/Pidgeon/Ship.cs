#region File Description

//-----------------------------------------------------------------------------
// Ship.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion File Description

#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

#endregion Using Statements

namespace Pidgeon
{
    internal class Ship : CModel
    {
        #region Fields

        public const float MAX_SPEED = 3.0f;

        Random _random = new Random();

        private const float MinimumAltitude = 350.0f;

        private string cooEffect;
        private TimeSpan _cooTime;

        public float thrustAmount;

        /// <summary>
        /// Location of ship in world space.
        /// </summary>
        public Vector3 Position;

        public float SpawnHeight;

        /// <summary>
        /// Direction ship is facing.
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// Ship's up vector.
        /// </summary>
        public Vector3 Up;

        private Vector3 right;

        /// <summary>
        /// Ship's right vector.
        /// </summary>
        public Vector3 Right
        {
            get { return right; }
        }

        /// <summary>
        /// Full speed at which ship can rotate; measured in radians per second.
        /// </summary>
        private const float RotationRate = 1.5f;

        /// <summary>
        /// Mass of ship.
        /// </summary>
        private const float Mass = 1.0f;

        /// <summary>
        /// Minimum force that can be applied along the ship's direction
        /// </summary>
        private const float ThrustForceMinimum = 0.25f;

        /// <summary>
        /// Maximum force that can be applied along the ship's direction.
        /// </summary>
        private const float ThrustForce = 24000.0f;

        /// <summary>
        /// Velocity scalar to approximate drag.
        /// </summary>
        private const float DragFactor = 0.97f;

        /// <summary>
        /// Current ship velocity.
        /// </summary>
        public Vector3 Velocity;

        public bool inverted;

        #endregion Fields

        #region Initialization

        public Ship(Model model, GraphicsDevice graphicsDevice, BasicEffect effect)
            : base(model, graphicsDevice, Matrix.Identity, effect)
        {
            inverted = true;
            Reset();
            cooEffect = "Sounds/Pidgeon/coo";
            SoundManager.LoadSound(cooEffect);
            _cooTime = TimeSpan.FromSeconds(5);
        }

        /// <summary>
        /// Restore the ship to its original starting state
        /// </summary>
        public void Reset()
        {
            Position = new Vector3(0, SpawnHeight, 0);
            Direction = Vector3.Forward;
            Up = Vector3.Up;
            right = Vector3.Right;
            Velocity = Vector3.Zero;

            thrustAmount = ThrustForceMinimum;
        }

        #endregion Initialization

        /// <summary>
        /// Applies a simple rotation to the ship and animates position based
        /// on simple linear motion physics.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float rotation = 1.0f;

            // Determine rotation amount from input
            Vector2 rotationAmount = Vector2.Zero;

            // get default input for keyboard
            if (keyboardState.IsKeyDown(Keys.Left))
                rotationAmount.X = rotation;
            if (keyboardState.IsKeyDown(Keys.Right))
                rotationAmount.X = -rotation;

            if (inverted)
            {
                if (keyboardState.IsKeyDown(Keys.Up))
                    rotationAmount.Y = -rotation;
                if (keyboardState.IsKeyDown(Keys.Down))
                    rotationAmount.Y = rotation;
            }
            else
            {
                if (keyboardState.IsKeyDown(Keys.Up))
                    rotationAmount.Y = rotation;
                if (keyboardState.IsKeyDown(Keys.Down))
                    rotationAmount.Y = -rotation;
            }

            // now override with gamepad input if connected
            if (gamePadState.IsConnected &&
                (gamePadState.ThumbSticks.Left.X != 0 ||
                gamePadState.ThumbSticks.Left.Y != 0 ||
                gamePadState.Triggers.Right != 0 ||
                gamePadState.Triggers.Left != 0)
                )
            {
                // turn left/right
                rotationAmount.X = -rotation * gamePadState.ThumbSticks.Left.X;

                // turn up/down
                rotationAmount.Y = -rotation * gamePadState.ThumbSticks.Left.Y;
            }

            if (rotationAmount.Y > MathHelper.ToRadians(80))
                rotationAmount.Y = MathHelper.ToRadians(80);

            // Scale rotation amount to radians per second
            rotationAmount = rotationAmount * RotationRate * elapsed;

            // Correct the X axis steering when the ship is upside down
            if (Up.Y < 0)
                rotationAmount.X = -rotationAmount.X;

            // Create rotation matrix from rotation amount
            Matrix rotationMatrix =
                Matrix.CreateFromAxisAngle(Right, rotationAmount.Y) *
                Matrix.CreateRotationY(rotationAmount.X);

            // Rotate orientation vectors
            Direction = Vector3.TransformNormal(Direction, rotationMatrix);
            Up = Vector3.TransformNormal(Up, rotationMatrix);

            // Re-normalize orientation vectors
            // Without this, the matrix transformations may introduce small rounding
            // errors which add up over time and could destabilize the ship.
            Direction.Normalize();
            Up.Normalize();

            // Re-calculate Right
            right = Vector3.Cross(Direction, Up);

            // The same instability may cause the 3 orientation vectors may
            // also diverge. Either the Up or Direction vector needs to be
            // re-computed with a cross product to ensure orthagonality
            Up = Vector3.Cross(Right, Direction);

            // Determine thrust amount from input

            // use keyboard by default
            if (keyboardState.IsKeyDown(Keys.W))
            {
                thrustAmount += 0.01f;
                if (thrustAmount > MAX_SPEED)
                    thrustAmount = MAX_SPEED;
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                thrustAmount -= 0.01f;
                if (thrustAmount < ThrustForceMinimum)
                    thrustAmount = ThrustForceMinimum;
            }

            // override keyboard with gamepad
            if (gamePadState.IsConnected &&
                (gamePadState.ThumbSticks.Left.X != 0 ||
                gamePadState.ThumbSticks.Left.Y != 0 ||
                gamePadState.Triggers.Right != 0 ||
                gamePadState.Triggers.Left != 0)
                )
            {
                thrustAmount += 0.01f * gamePadState.Triggers.Right;
                if (thrustAmount > MAX_SPEED)
                    thrustAmount = MAX_SPEED;

                thrustAmount -= 0.01f * gamePadState.Triggers.Left;
                if (thrustAmount < ThrustForceMinimum)
                    thrustAmount = ThrustForceMinimum;
            }

            // Calculate force from thrust amount
            Vector3 force = Direction * thrustAmount * ThrustForce;

            // Apply acceleration
            Vector3 acceleration = force / Mass;
            Velocity += acceleration * elapsed;

            // Apply psuedo drag
            Velocity *= DragFactor;

            // Apply velocity
            Position += Velocity * elapsed;

            // Prevent ship from flying under the ground
            Position.Y = Math.Max(Position.Y, MinimumAltitude);

            // Reconstruct the ship's world matrix
            World = Matrix.Identity;

            World.Forward = Direction;
            World.Up = Up;
            World.Right = right;
            World.Translation = Position;

            boundingBoxes = new List<BoundingBox>();
            Matrix[] transforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix meshTransform = transforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(Position);
                boundingBoxes.Add(BuildBoundingBox(mesh, meshTransform));
            }

            // Play sound shit
            //
            _cooTime -= gameTime.ElapsedGameTime;
            if (_cooTime.TotalMilliseconds <= 0)
            {
                SoundManager.PlaySound(cooEffect);
                _cooTime = TimeSpan.FromSeconds(_random.Next(5, 10));
            }
        }
    }
}
