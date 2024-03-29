using System;
using System.Runtime.CompilerServices;
using MonoGame.Extended;

namespace Retroherz.Math;

/// <summary>
/// Extension methods for float precision rays.
/// </summary>
public static class RayExtensions
{
	// Courtesy of One Lone Coder - based on and modified by Vidar "Voidar" Ermesjø
	// https://github.com/OneLoneCoder/Javidx9/blob/master/PixelGameEngine/SmallerProjects/OneLoneCoder_PGE_Rectangles.cpp

    /// <summary>
    /// Returns true if ray intersects rectangle.
    /// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Intersects(
		this Ray ray,
		(Vector Position, Vector Size) rectangle,
		out Vector contactPoint,
		out Vector contactNormal,
		out float timeHitNear,
		out float timeHitFar,
		bool infinite = false
	)
	{
		contactPoint = Vector.Zero;
		contactNormal = Vector.Zero;
		timeHitNear = 0;
		timeHitFar = 0;

		// Are we in the correct (South-East) quadrant?
		// ... then calculate out-of-bounds offset
		(bool IsOutOfBounds, Vector Offset) state = (
			IsOutOfBounds: rectangle.Position.X < 0 || rectangle.Position.Y < 0,
			Offset: rectangle.Size - ray.Origin
		);

		// To not confuse the algorithm on off-grid bounds we shift rectangle position and ray origin
		// to force upcomming calculations over to the positive axes.
		// - VE
		if (state.IsOutOfBounds)
		{
			rectangle.Position += state.Offset;
			ray.Origin += state.Offset;
			//System.Console.WriteLine("Out of bounds!");
		}

		// Cache division
		Vector inverseDirection = Vector.One / ray.Direction;

		// Calculate intersections with rectangle boundings axes
		Vector targetNear = (rectangle.Position - ray.Origin) * inverseDirection;
		Vector targetFar = (rectangle.Position + rectangle.Size - ray.Origin) * inverseDirection;

		if (float.IsNaN(targetFar.X) || float.IsNaN(targetFar.Y)) return false;
		if (float.IsNaN(targetNear.X) || float.IsNaN(targetNear.Y)) return false;

		/*if (float.IsNegativeInfinity(targetNear.X) || float.IsNegativeInfinity(targetNear.Y))
			Console.WriteLine($"Negative Infinity: {targetNear} direction:{ray.Direction}");

		if (float.IsPositiveInfinity(targetFar.X) || float.IsPositiveInfinity(targetFar.Y))
			Console.WriteLine($"Positive Infinity: {targetFar} direction:{ray.Direction}");*/

		// Sort distances
		if (targetNear.X > targetFar.X) Utils.Swap(ref targetNear.X, ref targetFar.X);
		if (targetNear.Y > targetFar.Y) Utils.Swap(ref targetNear.Y, ref targetFar.Y);

		// Early rejection
		if (targetNear.X > targetFar.Y || targetNear.Y > targetFar.X) return false;

		// Closest 'time' will be the first contact
		timeHitNear = MathF.Max(targetNear.X, targetNear.Y);

		//if (timeHitNear < 0) Console.WriteLine("Tunnelling!");

		// Furthest 'time' is contact on opposite side of rectangle
		timeHitFar = MathF.Min(targetFar.X, targetFar.Y);

		// Reject if ray directon is pointing away from object (can be usefull)
		// Added option - VE
		if (!infinite && timeHitFar < 0) return false;

		// For a correct calculation of contact point we shift ray origin back to the original value
		// - VE
		if (state.IsOutOfBounds) ray.Origin -= state.Offset;

		// Contact point of collision from parametric line equation
		contactPoint = ray.Origin + timeHitNear * ray.Direction;

		if (targetNear.X > targetNear.Y)
			contactNormal = inverseDirection.X < 0 ? Vector.UnitX : -Vector.UnitX;
		else if (targetNear.X < targetNear.Y)
			contactNormal = inverseDirection.Y < 0 ? Vector.UnitY : -Vector.UnitY;
		else
			// Note if targetNear == targetFar, collision is principly in a diagonal
			// so pointless to resolve. By returning a CN={0,0} even though its
			// considered a hit, the resolver wont change anything.

			// Diagonal case will be resolved in collision resolver. Thus we return CN={0,0}.
			// - VE
			contactNormal = Vector.Zero;

		return true;
	}

    /// <summary>
    /// Returns true if ray intersects rectangle.
    /// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Intersects(
		this Ray ray,
		ref (Vector Position, Vector Size) rectangle,
		out Vector contactPoint,
		out Vector contactNormal,
		out float timeHitNear
	)
	{
		float timeHitFar;

		return Intersects(
			ray,
			rectangle,
			out contactPoint,
			out contactNormal,
			out timeHitNear,
			out timeHitFar
		);
	}

	// MonoGame specific

    /// <summary>
    /// Returns true if ray intersects rectangle.
    /// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Intersects(
		this Ray ray,
		ref RectangleF rectangle,
		out Vector contactPoint,
		out Vector contactNormal
	)
	{
		float timeHitNear;
		float timeHitFar;

		return Intersects(
			ray,
			(rectangle.Position, rectangle.Size),
			out contactPoint,
			out contactNormal,
			out timeHitNear,
			out timeHitFar,
			infinite: true
		);
	}

    /// <summary>
    /// Returns true if ray intersects rectangle.
    /// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Intersects(this Ray ray,	(Vector Position, Vector Size) rectangle)
	{
		Vector contactPoint;
		Vector contactNormal;
		float timeHitNear;
		float timeHitFar;

		return Intersects(
			ray,
			(rectangle.Position, rectangle.Size),
			out contactPoint,
			out contactNormal,
			out timeHitNear,
			out timeHitFar,
			infinite: true
		);
	}

    /// <summary>
    /// Returns true if ray intersects rectangle.
    /// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Intersects(this Ray ray,	RectangleF rectangle)
	{
		Vector contactPoint;
		Vector contactNormal;
		float timeHitNear;
		float timeHitFar;

		return Intersects(
			ray,
			(rectangle.Position, rectangle.Size),
			out contactPoint,
			out contactNormal,
			out timeHitNear,
			out timeHitFar,
			infinite: true
		);
	}
}