using Microsoft.Xna.Framework;
using System;
using System.Data;
using System.Xml.Schema;

namespace FallingSand.ParticleTypes
{
    public class VaporParticle : Particle
    {
        // Factor by which velocity is reduced upon bouncing
        private float energyLossFactor = 0.8f;

        // Flag indicating whether altitude is high enough for the SmokeParticle to 'condense' into a WaterParticle
        private bool canCondense;

        // Flag indicating whether condensation should occur or not
        private bool willCondense;

        public VaporParticle(int x, int y) : base(x, y)
        {
            Velocity = -0.2f; // Very slow upward movement
        }

        public override void Update(float gravity, Particle[,] grid)
        {
            // Check Condensation via cooling
            CondenseFromCooling(grid);

            // Check altitude
            canCondense = CheckAltitude();

            // Check if the SmokeParticle will 'condense' into a WaterParticle
            willCondense = RainFactor();

            // Make WaterParticle if altitude is high enough, and RainFactor is true
            if (canCondense && willCondense) { MakeWater(grid); return; }

            // Apply the velocity to move the smoke upwards
            int newY = (int)(Y + Velocity);

            // Handle boundary collision for the Y-axis (top and bottom boundaries)
            if (newY < 0)
            {
                newY = 0; // Clamp to the top of the screen
                Velocity = -Velocity * energyLossFactor; // Invert velocity and reduce it
            }
            else if (newY >= Game1.gridHeight)
            {
                newY = Game1.gridHeight - 1; // Clamp to the bottom of the screen
                Velocity = -Velocity * energyLossFactor; // Invert velocity and reduce it
            }

            // Handle boundary collision for the X-axis (left and right boundaries)
            if (X <= 0)
            {
                X = 0; // Clamp to the left boundary
                Velocity = -Velocity * energyLossFactor; // Invert velocity and reduce it
            }
            else if (X >= Game1.gridWidth - 1)
            {
                X = Game1.gridWidth - 1; // Clamp to the right boundary
                Velocity = -Velocity * energyLossFactor; // Invert velocity and reduce it
            }

            // If the position above is empty, move the smoke particle up
            if (grid[X, newY] == null)
            {
                grid[X, Y] = null;
                Y = newY;
                grid[X, Y] = this;
            }
            else if (grid[X, newY] is VaporParticle)
            {
                // Clump with other smoke particles
                Random rand = new Random();
                int direction = rand.Next(0, 2) * 2 - 1;

                // Try to move left or right if blocked
                if (X + direction >= 0 && X + direction < Game1.gridWidth && grid[X + direction, Y] == null)
                {
                    grid[X, Y] = null;
                    X += direction;
                    grid[X, Y] = this;
                }
                else if (X - direction >= 0 && X - direction < Game1.gridWidth && grid[X - direction, Y] == null)
                {
                    grid[X, Y] = null;
                    X -= direction;
                    grid[X, Y] = this;
                }
            }

            // Slightly reduce the velocity to slow down over time
            Velocity *= 0.999f;
        }

        private void MakeWater(Particle[,] grid)
        {
            // Ensure we're not replacing an existing water particle
            if (grid[X, Y] is WaterParticle) return;

            // Remove existing VaporParticle
            grid[X, Y] = null;

            int newY = Y + 1; // Prefer placing water one cell below

            // Ensure we stay in bounds and check if the space below is empty
            if (newY < Game1.gridHeight && grid[X, newY] == null)
            {
                grid[X, newY] = new WaterParticle(X, newY);
                Game1.AddActiveParticle(grid[X, newY]); // Ensure the new water particle updates
            }
            else
            {
                // If no empty space below, create water in place
                grid[X, Y] = new WaterParticle(X, Y);
                Game1.AddActiveParticle(grid[X, Y]);
            }
        }


        private bool CheckAltitude()
        {
            // If the SmokeParticle is 75% game window height, return true
            if (Y <= Game1.gridHeight * .25) { return true; }
            return false;
        }

        private bool RainFactor()
        {
            // Generate a random number between 0 and 100
            Random random = new Random();

            // 10% Chance for the SmokeParticle to condense into a WaterParticle
            if (random.Next(0, 100) < 3) { return true; }
            return false;
        }

        private void CondenseFromCooling(Particle[,] grid)
        {
            // Check whether there is a cooling particle nearby
            Particle[] particlesNear = GetSurroundingParticles(grid);

            foreach (Particle particle in particlesNear) 
            {
                if (particle != null && particle.isCold)
                {
                    MakeWater(grid);
                    return; 
                }
            }
        }
        public override void MoveSelf(Particle[,] grid, int newX, int newY) {}
    }
}
