using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

using Retroherz.Components;

namespace Retroherz.Systems
{
    // DrawSystem => RenderSystemEX??
    public class RenderSystemEX : EntityDrawSystem, IDisposable
    {
        private bool _isDisposed = false;

        private readonly SpriteBatch _spriteBatch;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly OrthographicCamera _camera;
        private readonly RenderTarget2D _renderTarget;

        private ComponentMapper<SpriteComponent> _spriteComponentMapper;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;

        public RenderSystemEX(GraphicsDevice graphicsDevice, OrthographicCamera camera)
            : base(Aspect
                .All(typeof(SpriteComponent), typeof(PhysicsComponent)))
        {
            _camera = camera;
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _spriteComponentMapper = mapperService.GetMapper<SpriteComponent>();
            _physicsComponentMapper = mapperService.GetMapper<PhysicsComponent>();
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.Additive,
                samplerState: SamplerState.PointClamp,
                transformMatrix: _camera.GetViewMatrix());

            var deltaTime = gameTime.GetElapsedSeconds();
            foreach(var entityId in ActiveEntities)
            {
                var sprite = _spriteComponentMapper.Get(entityId);
                var physics = _physicsComponentMapper.Get(entityId);

                // SpriteSystem
                sprite.Scale = new Vector2(physics.Size.X, physics.Size.Y);
                sprite.Position = physics.Position;
                sprite.Update(gameTime);
                sprite.Draw(_spriteBatch);

                // Bounding rectangle
                var bounds = new RectangleF(physics.Position, physics.Size);
                _spriteBatch.DrawRectangle(bounds, Color.Green);

                // Pilot rectangle
                var pilot = new RectangleF(physics.Position + physics.Velocity * deltaTime, physics.Size);
                _spriteBatch.DrawRectangle(pilot, Color.Red);

                // Search area
                var minimum = Vector2.Min(bounds.TopLeft, pilot.TopLeft);
                var maximum = Vector2.Max(bounds.BottomRight, pilot.BottomRight);
                var inflated = new RectangleF(minimum, maximum - minimum);
                _spriteBatch.DrawRectangle(inflated, Color.Yellow);

                // Contac info
                var union = new RectangleF(physics.Position, physics.Size);
                foreach (var contact in physics.ContactInfo)
                {
                    // Normals
                    _spriteBatch.DrawPoint(contact.Item2, Color.BlueViolet, 4);
                    _spriteBatch.DrawPoint(contact.Item2, Color.Red, 4);
                    _spriteBatch.DrawLine(contact.Item2, contact.Item2 + contact.Item3 * 8, Color.Red);

                    // Rays
                    _spriteBatch.DrawLine(contact.Item2, contact.Item1.Position + contact.Item1.Origin, Color.BlueViolet);

                    // Inflated
                    _spriteBatch.DrawRectangle(contact.Item1.Position - physics.Origin, contact.Item1.Size + physics.Size, Color.BlueViolet);
                    
                    // Contacts
                    _spriteBatch.FillRectangle(contact.Item1.Position, contact.Item1.Size, Color.Yellow);

                    union = union.Union(new RectangleF(contact.Item1.Position, contact.Item1.Size));
                }
                _spriteBatch.DrawRectangle(union, Color.GreenYellow);


                // Embellish the "in contact" rectangles in yellow
                for (int i = 0; i < 4; i++)
                {
                    if (physics.Contact[i] != null)
                        _spriteBatch.DrawPoint(
                        physics.Contact[i].Position + physics.Contact[i].Origin, Color.Red, physics.Contact[i].Size.Length());
                    physics.Contact[i] = null;
                }
            }

            _spriteBatch.End();
        }

        public virtual void Dispose(bool disposing)
        {
            if(_isDisposed)
                return;

            if(disposing)
            {
                _graphicsDevice.Dispose();
                _spriteBatch.Dispose();
                _renderTarget.Dispose();
            }

            _isDisposed = true;
        }

    }
}