﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FallingSand.ParticleTypes;

namespace FallingSand
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public const int gridWidth = 800;
        public const int gridHeight = 600;
        const int cellSize = 2;
        Particle[,] grid = new Particle[gridWidth, gridHeight]; // Unified grid for all particles
        Texture2D pixel;
        SpriteFont font; // SpriteFont for text labels
        const float gravity = 0.8f;
        Random rand = new Random();
        string currentParticleType = "Sand"; // Default particle type

        // Define palette area
        Dictionary<string, Rectangle> particleButtons;
        Dictionary<string, Color> particleColors;
        List<Particle> activeParticles = new List<Particle>(); // List of active particles for selective updating

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            // Set the target framerate
            TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 90.0f); // FPS
            _graphics.SynchronizeWithVerticalRetrace = false; // Disable VSync to allow the set framerate to be used
            _graphics.PreferredBackBufferWidth = gridWidth * cellSize;
            _graphics.PreferredBackBufferHeight = gridHeight * cellSize + 80; // Extra space for palette and labels
        }

        protected override void Initialize()
        {
            // Create a 1x1 white pixel texture for drawing particles
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            // Initialize particle colors
            particleColors = new Dictionary<string, Color>
            {
                { "Sand", Color.Yellow },
                { "Water", Color.Blue },
                { "WetSand", new Color(169, 132, 46) },
                { "Fire", Color.Red },
                { "Lava", new Color(253, 83, 21) },
                { "Stone", new Color(191, 191, 191) },
                { "Vapor", Color.White },
                { "Soil", new Color(62, 49, 23) },
                { "Heater", new Color(255, 165, 0) },
                { "Cooler", new Color(0, 162, 232) },
                { "Snow", Color.White },
                { "Powder", new Color(40, 40, 40) },
                { "Smoke", Color.White },
                { "Acid", Color.Green }
            };

            // Initialize particle buttons
            particleButtons = new Dictionary<string, Rectangle>
            {
                { "Sand", new Rectangle(10, gridHeight * cellSize + 10, 50, 30) },
                { "Water", new Rectangle(110, gridHeight * cellSize + 10, 50, 30) },
                { "WetSand", new Rectangle(210, gridHeight * cellSize + 10, 50, 30) },
                { "Fire", new Rectangle(310, gridHeight * cellSize + 10, 50, 30) },
                { "Lava", new Rectangle(410, gridHeight * cellSize + 10, 50, 30) },
                { "Stone", new Rectangle(510, gridHeight * cellSize + 10, 50, 30) },
                { "Vapor", new Rectangle(610, gridHeight * cellSize + 10, 50, 30) },
                { "Soil", new Rectangle(710, gridHeight * cellSize + 10, 50, 30) },
                { "Heater", new Rectangle(810, gridHeight * cellSize + 10, 50, 30) },
                { "Cooler", new Rectangle(910, gridHeight * cellSize + 10, 50, 30) },
                { "Snow", new Rectangle(1010, gridHeight * cellSize + 10, 50, 30) },
                { "Powder", new Rectangle(1110, gridHeight * cellSize + 10, 50, 30) },
                { "Smoke", new Rectangle(1210, gridHeight * cellSize + 10, 50, 30) },
                { "Acid", new Rectangle(1310, gridHeight * cellSize + 10, 50, 30) }
            };

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the font for labels
            font = Content.Load<SpriteFont>("DefaultFont");
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                // Check if the click is within the palette area
                foreach (var button in particleButtons)
                {
                    if (button.Value.Contains(mouseState.Position))
                    {
                        currentParticleType = button.Key;
                        break;
                    }
                }

                // Handle dropping multiple particles into the grid
                int mouseX = mouseState.X / cellSize;
                int mouseY = mouseState.Y / cellSize;

                for (int offsetX = -5; offsetX <= 5; offsetX++)
                {
                    for (int offsetY = -5; offsetY <= 5; offsetY++)
                    {
                        int particleX = mouseX + offsetX;
                        int particleY = mouseY + offsetY;

                        if (particleX >= 0 && particleX < gridWidth && particleY >= 0 && particleY < gridHeight)
                        {
                            if (grid[particleX, particleY] == null)
                            {
                                Particle newParticle = CreateParticle(currentParticleType, particleX, particleY);
                                if (newParticle != null)
                                {
                                    grid[particleX, particleY] = newParticle;
                                    activeParticles.Add(newParticle);
                                }
                            }
                        }
                    }
                }
            }

            // Update particles based on type and order of execution

            // First pass: Update vapor and smoke particles (top-down)
            for (int i = 0; i < activeParticles.Count; i++)
            {
                if (activeParticles[i] is VaporParticle || activeParticles[i] is SmokeParticle)
                {
                    activeParticles[i].Update(gravity, grid);
                }
            }

            // Second pass: Update water particles (bottom-up)
            for (int i = activeParticles.Count - 1; i >= 0; i--)
            {
                if (activeParticles[i] is WaterParticle)
                {
                    activeParticles[i].Update(gravity, grid);
                }
            }

            // Third pass: Update all other particles (bottom-up)
            for (int i = activeParticles.Count - 1; i >= 0; i--)
            {
                if (!(activeParticles[i] is WaterParticle || activeParticles[i] is VaporParticle || activeParticles[i] is SmokeParticle))
                {
                    activeParticles[i].Update(gravity, grid);
                }
            }

            // Remove inactive particles
            // Removed, now the vapor works
            // activeParticles.RemoveAll(p => p.IsInactive());

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            // Draw particles
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y] != null)
                    {
                        string typeName = grid[x, y].GetType().Name.Replace("Particle", "");
                        if (particleColors.ContainsKey(typeName))
                        {
                            Color particleColor = particleColors[typeName];
                            int particleSize = (typeName == "Sand" || typeName == "Water" || typeName == "WetSand" || typeName == "Vapor" || typeName == "Soil" || typeName == "Snow" || typeName == "Powder" || typeName == "Smoke" || typeName == "Acid") ? cellSize * 2 : cellSize;
                            _spriteBatch.Draw(pixel, new Rectangle(x * cellSize, y * cellSize, particleSize, particleSize), particleColor);
                        }
                    }
                }
            }

            // Draw the palette buttons
            foreach (var button in particleButtons)
            {
                _spriteBatch.Draw(pixel, button.Value, particleColors[button.Key]);
                if (font != null)
                {
                    _spriteBatch.DrawString(font, button.Key, new Vector2(button.Value.X, button.Value.Y + button.Value.Height + 5), Color.White);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private Particle CreateParticle(string type, int x, int y)
        {
            switch (type)
            {
                case "Sand": return new SandParticle(x, y);
                case "Water": return new WaterParticle(x, y);
                case "WetSand": return new WetSandParticle(x, y);
                case "Fire": return new FireParticle(x, y);
                case "Lava": return new LavaParticle(x, y);
                case "Stone": return new StoneParticle(x, y);
                case "Vapor": return new VaporParticle(x, y);
                case "Soil": return new SoilParticle(x, y);
                case "Heater": return new HeaterParticle(x, y);
                case "Cooler": return new CoolerParticle(x, y);
                case "Snow": return new SnowParticle(x, y);
                case "Powder": return new GunpowderParticle(x, y);
                case "Smoke": return new SmokeParticle(x, y);
                default: return null;
            }
        }
    }
}