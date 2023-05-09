using UnityEngine;

namespace DeCrawl.Primitives
{
    public enum CardinalDirection
    {
        North, South, West, East, Up, Down, Invalid
    }

    public static class CardinalDirections
    {
        private static bool IsSmall(float value) => Mathf.Abs(value) < 1e-4f;

        public static CardinalDirection AsDirection(this Vector3 globalDirection)
        {
            var smallX = IsSmall(globalDirection.x);
            var smallY = IsSmall(globalDirection.y);
            var smallZ = IsSmall(globalDirection.z);

            if (!smallZ && smallX && smallY)
            {
                return globalDirection.z > 0 ? CardinalDirection.North : CardinalDirection.South;
            }
            if (!smallX && smallY && smallZ)
            {
                return globalDirection.x > 0 ? CardinalDirection.East : CardinalDirection.West;
            }
            if (!smallY && smallX && smallZ)
            {
                return globalDirection.x > 0 ? CardinalDirection.Up : CardinalDirection.Down;
            }

            return CardinalDirection.Invalid;

        }

        /// <summary>
        /// Direction vector to a FaceDirection
        /// </summary>
        /// <param name="globalDirection"></param>
        /// <param name="allowDiagonals">If true, will go for primary direction or when exact diagonals then NE will be N, NW be W and so on. Also disregards Y aspects</param>
        /// <returns></returns>
        public static CardinalDirection AsDirection(this Vector3Int globalDirection, bool allowDiagonals = false)
        {
            var zeroX = globalDirection.x == 0;
            var zeroY = globalDirection.y == 0;
            var zeroZ = globalDirection.z == 0;

            if (!zeroZ && zeroX && zeroY)
            {
                return globalDirection.z > 0 ? CardinalDirection.North : CardinalDirection.South;
            }
            if (!zeroX && zeroY && zeroZ)
            {
                return globalDirection.x > 0 ? CardinalDirection.East : CardinalDirection.West;
            }
            if (!zeroY && zeroX && zeroZ)
            {
                return globalDirection.x > 0 ? CardinalDirection.Up : CardinalDirection.Down;
            }

            if (allowDiagonals)
            {
                if (!zeroX && !zeroZ)
                {
                    if (Mathf.Abs(globalDirection.x) > Mathf.Abs(globalDirection.z))
                    {
                        return globalDirection.x > 0 ? CardinalDirection.East : CardinalDirection.West;
                    }
                    else if (Mathf.Abs(globalDirection.z) > Mathf.Abs(globalDirection.x))
                    {
                        return globalDirection.z > 0 ? CardinalDirection.North : CardinalDirection.South;
                    }

                    var signX = Mathf.Sign(globalDirection.x);
                    var signZ = Mathf.Sign(globalDirection.z);

                    if (signX == 1 && signZ == 1)
                    {
                        return CardinalDirection.North;
                    }
                    else if (signX == 1 && signZ == -1)
                    {
                        return CardinalDirection.East;
                    }
                    else if (signX == -1 && signZ == -1)
                    {
                        return CardinalDirection.South;
                    }
                    else
                    {
                        return CardinalDirection.West;
                    }
                }
                else if (!zeroX && !zeroY && zeroZ)
                {
                    return globalDirection.x > 0 ? CardinalDirection.East : CardinalDirection.West;
                }
                else if (zeroX && !zeroY && !zeroZ)
                {
                    return globalDirection.z > 0 ? CardinalDirection.North : CardinalDirection.South;
                }
            }

            return CardinalDirection.Invalid;

        }


        public static Vector3 AsVector(this CardinalDirection faceDirection)
        {
            switch (faceDirection)
            {
                case CardinalDirection.Up:
                    return Vector3.up;
                case CardinalDirection.Down:
                    return Vector3.down;
                case CardinalDirection.North:
                    return Vector3.forward;
                case CardinalDirection.South:
                    return Vector3.back;
                case CardinalDirection.West:
                    return Vector3.left;
                case CardinalDirection.East:
                    return Vector3.right;
                default:
                    throw new System.ArgumentException($"{faceDirection} is not a known as a vector");
            }
        }

        public static Quaternion AsRotation(this CardinalDirection lookDirection) => Quaternion.LookRotation(lookDirection.AsVector());


        public static Vector3Int AsIntVector(this CardinalDirection faceDirection)
        {
            switch (faceDirection)
            {
                case CardinalDirection.Up:
                    return Vector3Int.up;
                case CardinalDirection.Down:
                    return Vector3Int.down;
                case CardinalDirection.North:
                    return Vector3Int.forward;
                case CardinalDirection.South:
                    return Vector3Int.back;
                case CardinalDirection.West:
                    return Vector3Int.left;
                case CardinalDirection.East:
                    return Vector3Int.right;
                default:
                    throw new System.ArgumentException($"{faceDirection} is not a known as a vector");
            }
        }

        public static CardinalDirection Invert(this CardinalDirection faceDirection)
        {
            switch (faceDirection)
            {
                case CardinalDirection.Up:
                    return CardinalDirection.Down;
                case CardinalDirection.Down:
                    return CardinalDirection.Up;
                case CardinalDirection.North:
                    return CardinalDirection.South;
                case CardinalDirection.South:
                    return CardinalDirection.North;
                case CardinalDirection.West:
                    return CardinalDirection.East;
                case CardinalDirection.East:
                    return CardinalDirection.West;
                default:
                    throw new System.ArgumentException($"{faceDirection} has no inverse");
            }
        }

        public static CardinalDirection RotateCW(this CardinalDirection faceDirection)
        {
            switch (faceDirection)
            {
                case CardinalDirection.Up:
                    return CardinalDirection.Up;
                case CardinalDirection.Down:
                    return CardinalDirection.Down;
                case CardinalDirection.North:
                    return CardinalDirection.East;
                case CardinalDirection.East:
                    return CardinalDirection.South;
                case CardinalDirection.South:
                    return CardinalDirection.West;
                case CardinalDirection.West:
                    return CardinalDirection.North;
                default:
                    throw new System.ArgumentException($"{faceDirection} has no inverse");
            }
        }

        public static CardinalDirection RotateCCW(this CardinalDirection faceDirection)
        {
            switch (faceDirection)
            {
                case CardinalDirection.Up:
                    return CardinalDirection.Up;
                case CardinalDirection.Down:
                    return CardinalDirection.Down;
                case CardinalDirection.North:
                    return CardinalDirection.West;
                case CardinalDirection.West:
                    return CardinalDirection.South;
                case CardinalDirection.South:
                    return CardinalDirection.East;
                case CardinalDirection.East:
                    return CardinalDirection.North;
                default:
                    throw new System.ArgumentException($"{faceDirection} has no inverse");
            }
        }
    }
}
