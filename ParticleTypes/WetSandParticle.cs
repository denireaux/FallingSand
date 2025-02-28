using Microsoft.Xna.Framework;
using System;

namespace FallingSand.ParticleTypes
{
    public class WetSandParticle : Particle
    {
        private float sinkTimer = 0f;
        private const float SinkDelay = 0.04f;

        public WetSandParticle(int x, int y) : base(x, y)
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
            Particle[] particlesNear = GetSurroundingParticles(grid);

            Particle particleLeft = particlesNear[0];
            Particle particleRight = particlesNear[1];
            Particle particleBelow = particlesNear[3];


            if (particleBelow is WaterParticle)
            {
                sinkTimer += 1f / 60f; 
                if (sinkTimer >= SinkDelay)
                {
                    sinkTimer = 0f; 
                    SinkIntoWater(grid, newX, newY);
                }
                return;
            }

            sinkTimer = 0f; 
            if (particleBelow == null)
            {
                MoveDown(grid, newX, newY);
                return;
            }

            if (particleBelow is SandParticle) { DampenBelow(grid); }

            // Handle downward+diagonal movement
            // TODO: Refactor into class methods
            // Potentially an abstract-class method, or a separate abstract class for just solid particles
            else if (X + 1 < grid.GetLength(0) && Y + 1 < grid.GetLength(1) && particleRight == null && grid[X + 1, Y + 1] == null) { MoveDownRight(grid); }
            else if (X - 1 >= 0 && Y + 1 < grid.GetLength(1) && particleLeft == null && grid[X - 1, Y + 1] == null) { MoveDownLeft(grid); }
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

        private bool IsWithinBounds(Particle[,] grid, int x, int y)
        {
            return x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1);
        }

        private void MoveDown(Particle[,] grid, int newX, int newY)
        {
            if (!IsWithinBounds(grid, newX, newY)) return;

            grid[X, Y] = null;
            grid[newX, newY] = this;
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

        private void DampenBelow(Particle[,] grid)
        {
            if (!IsWithinBounds(grid, X, Y + 1)) return;

            grid[X, Y + 1] = new WetSandParticle(X, Y + 1);
        }
    }
}
