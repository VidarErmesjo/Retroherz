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
        private ComponentMapper<SpriteComponent> _spriteComponentMapper;
        private ComponentMapper<PlayerComponent> _playerComponentMapper;
        private ComponentMapper<ColliderComponent> _colliderComponentMapper;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;

        public PlayerSystem(OrthographicCamera camera)
            : base(Aspect
                .All(
                    typeof(SpriteComponent),
                    typeof(PlayerComponent),
                    typeof(PhysicsComponent))
                .One(typeof(ColliderComponent)))
        {
            _camera = camera;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _spriteComponentMapper = mapperService.GetMapper<SpriteComponent>();
            _playerComponentMapper = mapperService.GetMapper<PlayerComponent>();
            _colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
            _physicsComponentMapper = mapperService.GetMapper<PhysicsComponent>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var mouseState = Mouse.GetState();
            var deltaTime = gameTime.GetElapsedSeconds();
            
            var player = _playerComponentMapper.Get(entityId);
            var sprite = _spriteComponentMapper.Get(entityId);
            var physics = _physicsComponentMapper.Get(entityId);
            var collider = _colliderComponentMapper.Get(entityId);

            // Calculate velocity from direction
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                //var halfExtents = physics. / 2;
                physics.Direction = Vector2.Normalize(_camera.ScreenToWorld(
                    new Vector2(mouseState.X, mouseState.Y)) - physics.Position - physics.Origin);

                physics.Velocity += physics.Direction * player.MaxSpeed * ((float)deltaTime);
                //ClampVelocity(ref physics);
                sprite.Play("Walk");
            }
            else
            {
                physics.Velocity = new Vector2(
                    MathHelper.LerpPrecise(physics.Velocity.X, 0, 1f * deltaTime),
                    MathHelper.LerpPrecise(physics.Velocity.Y, 0, 1f * deltaTime));

                sprite.Play("Idle");
            }

            // Can haz floatie camera??
           /*physics.Position = new Vector2(
                MathHelper.LerpPrecise(physics.Position.X, _camera.Center.X, 0.1f * deltaTime),
                MathHelper.LerpPrecise(physics.Position.Y, _camera.Center.Y, 0.1F * deltaTime));*/

            // Update camera
            _camera.LookAt(physics.Position);
        }
    }
}