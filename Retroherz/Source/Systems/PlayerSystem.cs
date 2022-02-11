using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

using Retroherz.Components;

namespace Retroherz.Systems
{
    public class PlayerSystem : EntityProcessingSystem
    {
        private readonly OrthographicCamera _camera;
        private ComponentMapper<ColliderComponent> _colliderComponentMapper;
        private ComponentMapper<PlayerComponent> _playerComponentMapper;
        private ComponentMapper<SpriteComponent> _spriteComponentMapper;
        private ComponentMapper<TransformComponent> _transformComponentMapper;

        public PlayerSystem(OrthographicCamera camera)
            : base(Aspect
                .All(
                    typeof(ColliderComponent),
                    typeof(PlayerComponent),
                    typeof(SpriteComponent),
                    typeof(TransformComponent)))
        {
            _camera = camera;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
            _playerComponentMapper = mapperService.GetMapper<PlayerComponent>();
            _spriteComponentMapper = mapperService.GetMapper<SpriteComponent>();
            _transformComponentMapper = mapperService.GetMapper<TransformComponent>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();
            var deltaTime = gameTime.GetElapsedSeconds();

            var collider = _colliderComponentMapper.Get(entityId);           
            var player = _playerComponentMapper.Get(entityId);
            var sprite = _spriteComponentMapper.Get(entityId);
            var transform = _transformComponentMapper.Get(entityId);

            var direction = Vector2.Zero;
            if (keyboardState.GetPressedKeyCount() > 0)
            {
                if (keyboardState.IsKeyDown(Keys.Up))
                    direction += -Vector2.UnitY;
                if (keyboardState.IsKeyDown(Keys.Down))
                    direction += Vector2.UnitY;
                if (keyboardState.IsKeyDown(Keys.Left))
                    direction += -Vector2.UnitX;
                if (keyboardState.IsKeyDown(Keys.Right))
                    direction += Vector2.UnitX;

                direction.Normalize();
                if(direction.IsNaN())
                    System.Console.WriteLine("direction.isNaN()");

                // Important! Must check if is NaN
                if(direction.IsNaN()) direction = Vector2.Zero;

                collider.Velocity += direction * player.MaxSpeed * deltaTime * 2;

               /* if (keyboardState.IsKeyDown(Keys.Space))
                    collider.Velocity = new Vector2(collider.Velocity.X, -100f);*/

                sprite.Play("Walk");
            }
            else if (mouseState.LeftButton == ButtonState.Pressed)
            {
                // Accelerate
                direction = Vector2.Normalize(_camera.ScreenToWorld(
                    new Vector2(mouseState.X, mouseState.Y)) - transform.Position - collider.Origin);

                collider.Velocity += direction * player.MaxSpeed * deltaTime;
                
                /*collider.Velocity = new Vector2((
                    MathHelper.Clamp(collider.Velocity.X, -player.MaxSpeed, player.MaxSpeed)),
                    MathHelper.Clamp(collider.Velocity.Y, -player.MaxSpeed, player.MaxSpeed));*/

                sprite.Play("Walk");
            }
            else sprite.Play("Idle");

            // Slow down
            /*var factor = deltaTime * -1;
            collider.Velocity = new Vector2(
                MathHelper.LerpPrecise(0, collider.Velocity.X, MathF.Pow(2, factor)),
                MathHelper.LerpPrecise(0, collider.Velocity.Y, MathF.Pow(2, factor)));*/
            
            //collider.Velocity = new Vector2(collider.Velocity.X, collider.Velocity.Y + 9.81f);
            
            // Update camera
            _camera.LookAt(transform.Position + collider.Origin);
        }
    }
}