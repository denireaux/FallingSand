using Microsoft.Xna.Framework;
using System;

namespace FallingSand.ParticleTypes
{
    public class SandParticle : Particle
    {
        
        private float sinkTimer = 0f;
        private const float SinkDelay = 0.25f;
        
        public SandParticle(int x, int y) : base(x, y)
        {
            Velocity = 0f;
        }

        public override void Update(float gravity, Particle[,] grid)
        {
            Velocity += gravity * 1.0f;
            int newY = (int)(Y + Velocity);

            if (newY >= Game1.gridHeight)
                newY = Game1.gridHeight - 1;

            if (newY < Game1.gridHeight)
            {
                MoveSelf(grid, X, Y + 1);
            }
        }

        public override void MoveSelf(Particle[,] grid, int newX, int newY)
        {
            // Boundary check 
            bool isInBounds = IsWithinBounds(grid, newX, newY);
            if (!isInBounds) { return; }


            Particle[] particlesNear = GetSurroundingParticles(grid);
            Particle particleLeft = particlesNear[0];
            Particle particleRight = particlesNear[1];
            Particle particleBelow = particlesNear[3];

            // Down Movement
            if (particleBelow == null) { MoveDown(grid, X, Y + 1); }

            // TODO: Fix non-sinking logic
            // I don't know why this is happening
            else if (particleBelow is WaterParticle) 
            { 
                MakeWetSand(grid); 
                sinkTimer += 1f / 60f;
                if (sinkTimer >= SinkDelay)
                {
                    sinkTimer = 0f;
                    SinkIntoWater(grid, newX, newY);
                }
                return;
            } 

            // Handle downward+diagonal movement
            // TODO: Refactor into class methods
            // Potentially an abstract-class method, or a separate abstract class for just solid particles
            else if (X + 1 < grid.GetLength(0) && Y + 1 < grid.GetLength(1) && particleRight == null && grid[X + 1, Y + 1] == null) { MoveDownRight(grid); }
            else if (X - 1 >= 0 && Y + 1 < grid.GetLength(1) && particleLeft == null && grid[X - 1, Y + 1] == null) { MoveDownLeft(grid); }

            else { return; }
        }

        private bool IsWithinBounds(Particle[,] grid, int x, int y)
        {
            return x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1);
        }

        private void MoveDown(Particle[,] grid, int newX, int newY)
        {
            grid[newX, newY] = this;
            grid[X, Y] = null;
            X = newX;
            Y = newY;
        }

        private void MoveDownRight(Particle[,] grid)
        {
            grid[X, Y] = null;
            grid[X + 1, Y + 1] = this;
            X++;
            Y++;
        }

        private void MoveDownLeft(Particle[,] grid)
        {
            grid[X, Y] = null;
            grid[X - 1, Y + 1] = this;
            X--;
            Y++;
        }

        private void MakeWetSand(Particle[,] grid)
        {
            grid[X, Y] = null;
            grid[X, Y] = new WetSandParticle(X, Y);
        }

        private void SinkIntoWater(Particle[,] grid, int newX, int newY)
        {
            if (!IsWithinBounds(grid, newX, newY)) return;

            Particle waterParticle = grid[newX, newY];

            grid[X, Y] = waterParticle;
            grid[newX, newY] = this;

            if (waterParticle != null)
            {
                waterParticle.X = X;
                waterParticle.Y = Y;
            }

            X = newX;
            Y = newY;
        }
    }
}