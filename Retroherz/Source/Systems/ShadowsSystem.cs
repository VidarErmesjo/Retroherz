using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Shapes;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.VectorDraw;
using Retroherz.Components;
using Retroherz.Managers;
using Retroherz.Math;
using Retroherz.Visibility;

namespace Retroherz.Systems;

public partial class ShadowsSystem : EntityUpdateSystem, IDrawSystem
{
	private readonly OrthographicCamera _camera;
	private readonly Rectangle _destinationRectangle;
	private readonly (Effect Blur, Effect Combine, Effect Light) _effect;
	private readonly GraphicsDevice _graphics;
	private readonly (RenderTarget2D Color, RenderTarget2D Light) _renderTarget;
	private readonly SpriteBatch _spriteBatch;
	private readonly TiledMap _tiledMap;
	private readonly VisibilityComputer _visibilityComputer;

	private ComponentMapper<ColliderComponent> _colliderComponentMapper;
	private ComponentMapper<PlayerComponent> _playerComponentMapper;
	private ComponentMapper<PointLightComponent> _pointLightComponentMapper;
	private ComponentMapper<SpriteComponent> _spriteComponentMapper;
	private ComponentMapper<TransformComponent> _transformComponentMapper;

	//private VisibilityComputer _visibiltyComputer;

	public Bag<int> Occluders = new();
	public Bag<int> Illumers = new();

	public ShadowsSystem(GraphicsManager graphicsManager, TiledMap tiledMap)
		: base(Aspect.All(typeof(ColliderComponent), typeof(TransformComponent)))
	{
		_camera = graphicsManager.GetCamera();
		_destinationRectangle = graphicsManager.DestinationRectangle;
		_effect.Blur = graphicsManager.GetEffect("Blur");
		_effect.Combine = graphicsManager.GetEffect("Combine");
		_effect.Light = graphicsManager.GetEffect("Light");
		_graphics = graphicsManager.GetGraphicsDevice();
		_renderTarget.Color = graphicsManager.GetRenderTarget("Color");
		_renderTarget.Light = graphicsManager.GetRenderTarget("Light");	// ... or create it here
		_spriteBatch = graphicsManager.GetSpriteBatch();
		_tiledMap = tiledMap;
		_visibilityComputer = VisibilityComputer.GetInstance();
	}

	public override void Initialize(IComponentMapperService mapperService)
	{
		_colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
		_playerComponentMapper = mapperService.GetMapper<PlayerComponent>();
		_pointLightComponentMapper = mapperService.GetMapper<PointLightComponent>();
		_spriteComponentMapper = mapperService.GetMapper<SpriteComponent>();
		_transformComponentMapper = mapperService.GetMapper<TransformComponent>();
	}

	public override void Update(GameTime gameTime)
	{
		Occluders.Clear();
		Illumers.Clear();

		// "Query"
		foreach (int entityId in ActiveEntities.AsReadOnlySpan())
		{
			if (_spriteComponentMapper.Get(entityId) is null)
				continue;

			/*(ColliderComponent Collider, TransformComponent Transform) occluder = (
				_colliderComponentMapper.Get(entityId),
				_transformComponentMapper.Get(entityId)
			);

			VisibilityComputer.AddOccluder(new RectangleF(occluder.Transform.Position, occluder.Collider.Size));*/

			Occluders.Add(entityId);

			if (_pointLightComponentMapper.Get(entityId) is null)
				continue;

			Illumers.Add(entityId);
		}

		//Console.WriteLine($"Illumers:{Illumers.Count} Occluders:{Occluders.Count}");

		foreach (int illumerId in Illumers.AsReadOnlySpan())
		{
			(ColliderComponent Collider, PointLightComponent Light, TransformComponent Transform) illumer = (
				_colliderComponentMapper.Get(illumerId),
				_pointLightComponentMapper.Get(illumerId),
				_transformComponentMapper.Get(illumerId)
			);

			// IDEA!
			// Pick "collider" entities by rectangle-circle intersections (radius)

			/*_visibiltyComputer = new VisibilityComputer(
				illumer.Transform.Position + illumer.Collider.HalfExtents,
				illumer.Light.Radius
			);*/

			/*foreach (int occluderId in Occluders.AsReadOnlySpan())
			{
				if (occluderId == illumerId)
					continue;

				(ColliderComponent Collider, TransformComponent Transform) occluder = (
					_colliderComponentMapper.Get(occluderId),
					_transformComponentMapper.Get(occluderId)
				);

				VisibilityComputer.AddOccluder(new RectangleF(occluder.Transform.Position, occluder.Collider.Size));
			}*/


			_visibilityComputer.SetIllumer(
				illumer.Transform.Position + illumer.Collider.HalfExtents,
				illumer.Light.Radius
			);
			//_visibilityComputer.AddOccluder(new RectangleF(illumer.Transform.Position, illumer.Collider.Size));

			foreach (int occluderId in Occluders.AsReadOnlySpan())
			{
				if (occluderId == illumerId)
					continue;

				(ColliderComponent Collider, TransformComponent Transform) occluder = (
					_colliderComponentMapper.Get(occluderId),
					_transformComponentMapper.Get(occluderId)
				);

				_visibilityComputer.AddOccluder(new RectangleF(occluder.Transform.Position, occluder.Collider.Size));
			}
			
			/*ReadOnlySpan<Vector> points = VisibilityExtensions.PolyMap.CalculateVisibilityPolygon(
				illumer.Transform.Position + illumer.Collider.HalfExtents,
				illumer.Light.Radius		
			);*/

			ReadOnlySpan<Vector> points = _visibilityComputer.CalculateVisibilityPolygon();

			/*ReadOnlySpan<Vector> points = _tiledMap.CreatePolyMap()
				.CalculateVisibilityPolygon(
					illumer.Transform.Position + illumer.Collider.HalfExtents
					, illumer.Light.Radius
				);*/


			illumer.Light.Primitive = new Primitive()
				.CreateTriangleFan(points, illumer.Transform.Position + illumer.Collider.HalfExtents)
				.ProjectToScreen(_camera);

			/*ReadOnlySpan<Vector> list = VisibilityComputer.Compute(
				illumer.Transform.Position + illumer.Collider.HalfExtents,
				illumer.Light.Radius
			);*/

			/*Span<Vector> vertices = new Vector[list.Count];

			for (int i = 0; i < vertices.Length; i++)
				vertices[i] = list[i];*/

			/*illumer.Light.Primitive = new Primitive()
				.CreateTriangleFan(vertices, illumer.Transform.Position + illumer.Collider.HalfExtents)
				.ProjectToScreen(_camera);*/
		}
	}

	public void Draw(GameTime gameTime)
	{
		// Draw Color
		//_graphics.SetRenderTarget(_renderTarget.Color);
		_graphics.Clear(Color.Transparent);

		_spriteBatch.Begin(
			sortMode: SpriteSortMode.BackToFront,
			blendState: BlendState.AlphaBlend,
			samplerState: SamplerState.PointClamp,
			depthStencilState: DepthStencilState.Default,
			rasterizerState: RasterizerState.CullCounterClockwise,
			effect: null,
			transformMatrix: _camera.GetViewMatrix()
		);
			//ReadOnlySpan<PolyMap> polyMap = _tiledMap.CreatePolyMap();
			foreach (var edge in _visibilityComputer.GetPolyMap())
				_spriteBatch.DrawLine(edge.Start, edge.End, Color.Red);

		_spriteBatch.End();

		// Draw Light
		float ambientLightStrength = 0.0f;

		//_graphics.Clear(Color.White * ambientLightStrength);

		_graphics.BlendState = BlendState.Additive;

		// Samplers states are set by the shader itself            
		_graphics.DepthStencilState = DepthStencilState.None;
		_graphics.RasterizerState = RasterizerState.CullNone;

		foreach (int illumerId in Illumers.AsReadOnlySpan())
		{
			(ColliderComponent Collider, PointLightComponent Light, TransformComponent Transform) illumer = (
				_colliderComponentMapper.Get(illumerId),
				_pointLightComponentMapper.Get(illumerId),
				_transformComponentMapper.Get(illumerId)
			);

			// Apply the effect
			_effect.Light.Parameters["lightSource"].SetValue(illumer.Transform.Position + illumer.Collider.HalfExtents);
			_effect.Light.Parameters["lightColor"].SetValue(illumer.Light.Color.ToVector3() * illumer.Light.Power);
		 	_effect.Light.Parameters["lightRadius"].SetValue(illumer.Light.Radius);
			_effect.Light.Techniques[0].Passes[0].Apply();

			// Render the mesh
			illumer.Light.Primitive.Draw(_graphics);
		}
	}
}