namespace FallingSand.ParticleTypes
{
    public abstract class Particle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public float Velocity { get; set; }
        public bool isHot { get; protected set; }
        public bool isCold { get; protected set; }

        protected Particle(int x, int y)
        {
            X = x;
            Y = y;
            Velocity = 0f;
            isHot = false;
            isCold = false;
        }

        public abstract void Update(float gravity, Particle[,] grid);
        public abstract void MoveSelf(Particle[,] grid, int newX, int newY);

        public Particle[] GetSurroundingParticles(Particle[,] grid)
        {
            Particle left = (X > 0) ? grid[X - 1, Y] : null;
            Particle right = (X < grid.GetLength(0) - 1) ? grid[X + 1, Y] : null;
            Particle above = (Y > 0) ? grid[X, Y - 1] : null;
            Particle below = (Y < grid.GetLength(1) - 1) ? grid[X, Y + 1] : null;

            Particle lowerLeft = (X > 0 && Y < grid.GetLength(1) - 1) ? grid[X - 1, Y + 1] : null;
            Particle lowerRight = (X < grid.GetLength(0) - 1 && Y < grid.GetLength(1) - 1) ? grid[X + 1, Y + 1] : null;

            return new Particle[] { left, right, above, below, lowerLeft, lowerRight };
        }

        public virtual bool IsInactive()
        {
            // A simple check could be that the particle is inactive if its velocity is very low.
            return Velocity < 0.1f;
        }

        /// <summary>
        /// Swaps the current particle with the one below it in the grid.
        /// </summary>
        /// <param name="grid">The grid containing all particles.</param>
        public void SwapWithBelow(Particle[,] grid)
        {
            // Ensure we can move below
            if (Y < grid.GetLength(1) - 1)
            {
                Particle below = grid[X, Y + 1];

                if (below != null)
                {
                    // Swap positions in the grid
                    grid[X, Y + 1] = this; // Move current particle down
                    grid[X, Y] = below;    // Move the particle below up

                    // Update their coordinates
                    below.Y = Y;
                    this.Y = Y + 1;
                }
                else
                {
                    // If "below" is null, just move the current particle there
                    grid[X, Y + 1] = this;
                    grid[X, Y] = null;
                    this.Y = Y + 1;
                }
            }
        }
    }
}